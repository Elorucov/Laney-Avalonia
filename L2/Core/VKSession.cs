using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Execute;
using ELOR.Laney.Execute.Objects;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels;
using ELOR.Laney.Views;
using VKUI.Controls;
using VKUI.Popups;
using ELOR.VKAPILib;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using ELOR.Laney.DataModels;
using Newtonsoft.Json;

namespace ELOR.Laney.Core {
    public sealed class VKSession : ViewModelBase {
        private string _name;
        private Uri? _avatar;
        private ImViewModel _imViewModel;
        private ChatViewModel _currentOpenedChat;

        public int Id { get { return GroupId > 0 ? -GroupId : UserId; } }
        public int UserId { get; private set; }
        public int GroupId { get; private set; }
        public string Name { get { return _name; } private set { _name = value; OnPropertyChanged(); } }
        public Uri? Avatar { get { return _avatar; } private set { _avatar = value; OnPropertyChanged(); } }
        public ImViewModel ImViewModel { get { return _imViewModel; } private set { _imViewModel = value; OnPropertyChanged(); } }
        public ChatViewModel CurrentOpenedChat { get { return _currentOpenedChat; } set { _currentOpenedChat = value; OnPropertyChanged(); } }

        public bool IsGroup { get => GroupId > 0; }
        public VKAPI API { get; private set; }
        public LongPoll LongPoll { get; private set; }
        public MainWindow Window { get; private set; }

        public event EventHandler<int> CurrentOpenedChatChanged;

        #region Binded from UI and tray menu

        public void ShowSessionPopup(Button owner) {
            ActionSheet ash = new ActionSheet {
                Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft
            };

            foreach (var session in VKSession.Sessions) {
                if (session.GroupId == 142317063 || session.GroupId == 176011438) continue; // Temporary
                if (session.Id == Id) continue;
                ActionSheetItem item = new ActionSheetItem {
                    Before = new Avatar {
                        ImageUri = session.Avatar,
                        Initials = session.Name.GetInitials(session.IsGroup),
                        Background = session.Id.GetGradient(),
                        Foreground = new SolidColorBrush(Colors.White),
                        Width = 20,
                        Height = 20
                    },
                    Header = session.Name,
                    Tag = session.Id
                };
                item.Click += TryOpenSessionWindow;
                ash.Items.Add(item);
            }

            if (ash.Items.Count > 0) ash.Items.Add(new ActionSheetItem());

            ActionSheetItem settings = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20GearOutline },
                Header = Localizer.Instance["settings"],
            };
            ActionSheetItem about = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20InfoCircleOutline },
                Header = Localizer.Instance["about"],
            };
            ActionSheetItem logout = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20DoorArrowRightOutline },
                Header = Localizer.Instance["log_out"],
            };
            logout.Classes.Add("Destructive");

            ash.Items.Add(settings);
            ash.Items.Add(about);
            ash.Items.Add(logout);

            List<ActionSheetItem> devmenu = new List<ActionSheetItem>();
            if (DemoMode.IsEnabled) {
                ActionSheetItem demoresize = new ActionSheetItem {
                    Before = new VKIcon { Id = VKIconNames.Icon20GearOutline },
                    Header = "Resize 1134x756",
                };
                demoresize.Click += (a, b) => {
                    Window.WindowState = WindowState.Normal;
                    Window.PlatformImpl.Resize(new Size(1134, 756));
                };
                // devmenu.Add(demoresize);
            }

            if (devmenu.Count > 0) {
                ash.Items.Add(new ActionSheetItem());
                foreach (var item in devmenu) {
                    ash.Items.Add(item);
                }
            }

            ash.ShowAt(owner);
        }

        private static void SetUpTrayMenu() {
            NativeMenu menu = new NativeMenu();

            foreach (var session in VKSession.Sessions) {
                if (session.GroupId == 142317063 || session.GroupId == 176011438) continue; // Temporary

                var item = new NativeMenuItem { Header = session.Name };
                item.Click += (a, b) => TryOpenSessionWindow(session.Id);
                menu.Items.Add(item);
            }

            menu.Items.Add(new NativeMenuItemSeparator());

            var ft = new NativeMenuItem { Header = "Field test" };
            ft.Click += (a, b) => {
                new FieldTestWindow().Show();
            };

            var exit = new NativeMenuItem { Header = Localizer.Instance["exit"] };
            exit.Click += (a, b) => {
                App.Current.DesktopLifetime.Shutdown();
            };

            if (!DemoMode.IsEnabled) menu.Items.Add(ft);
            menu.Items.Add(exit);

            TrayIcon icon = new TrayIcon {
                Icon = new WindowIcon(AssetsManager.GetBitmapFromUri(new Uri("avares://laney/Assets/Logo/Tray/t16mw.png"))),
                Menu = menu,
                IsVisible = true,
                ToolTipText = "Laney"
            };

            TrayIcons icons = new TrayIcons();
            icons.Add(icon);

            Application.Current.SetValue(TrayIcon.IconsProperty, icons);
        }

        #endregion

        #region Internal

        private async void Init(bool dontUpdateSessionsList = false) {
            try {
                Log.Information("Init session ({0})", Id);
                SetUpTrayMenu(); // Чтобы можно было закрыть приложение, если будут проблемы с загрузкой

                if (DemoMode.IsEnabled) {
                    ImViewModel = new ImViewModel(this);
                    return;
                }
                if (API.WebRequestCallback == null) API.WebRequestCallback = LNetExtensions.SendRequestToAPIViaLNetAsync;

                List<VKSession> sessions = new List<VKSession>();
                StartSessionResponse info = await API.StartSessionAsync();

                // Условие должно выполниться всего один раз после запуска первой сессии
                // (обязательно юзер, а не сообщество), здесь происходит создание списка сессий
                // для отображения их в меню и в tray-menu.
                if (!dontUpdateSessionsList) {
                    sessions.Add(this);

                    foreach (var group in info.Groups) {
                        CacheManager.Add(group);
                        if (!group.CanMessage) continue;

                        VKSession gs = new VKSession {
                            UserId = info.User.Id,
                            GroupId = group.Id,
                            Name = group.Name,
                            Avatar = new Uri(group.Photo100),
                            API = new VKAPI(info.User.Id, API.AccessToken, Localizer.Instance["lang"], App.UserAgent),
                        };
                        gs.LongPoll = new LongPoll(gs.API, gs.Id, gs.GroupId);
                        sessions.Add(gs);
                    }

                    _sessions = sessions;
                }

                if (!IsGroup) {
                    var currentUser = info.User;
                    CacheManager.Add(currentUser);

                    Name = currentUser.FullName;
                    Avatar = new Uri(currentUser.Photo100);
                } else {
                    var currentGroup = _sessions.Where(s => s.Id == Id).FirstOrDefault();
                    Name = currentGroup.Name;
                    Avatar = currentGroup.Avatar;
                }

                var lp = info.LongPolls.Where(lps => lps.SessionId == Id).FirstOrDefault();
                if (LongPoll == null) LongPoll = new LongPoll(API, Id, GroupId);
                LongPoll.SetUp(lp.LongPoll);
                LongPoll.StateChanged += LongPoll_StateChanged;
                LongPoll.Run();

                SetUpTrayMenu(); // обновляем tray menu, отображая уже все загружнные сессии
            } catch (Exception ex) {
                Log.Error(ex, "Init failed. Waiting 3 sec. before trying again...");
                await Task.Delay(3000);
                Init(dontUpdateSessionsList);
            }

            if (ImViewModel == null) ImViewModel = new ImViewModel(this);
        }

        private static void TryOpenSessionWindow(object? sender, RoutedEventArgs e) {
            ActionSheetItem item = sender as ActionSheetItem;
            int sessionId = (int)item.Tag;
            TryOpenSessionWindow(sessionId);
        }

        private static void TryOpenSessionWindow(int sessionId) {
            VKSession session = _sessions.Where(s => s.Id == sessionId).FirstOrDefault();

            if (session.Window == null) {
                Log.Information("Creating and showing new window for session {0}", sessionId);
                session.Window = new MainWindow();
                session.Window.DataContext = session;
                session.Init(true);
                session.Window.Show();
            } else {
                Log.Information("Showing/activating window for session {0}", sessionId);
                if (!session.Window.IsVisible) session.Window.Show();
                if (!session.Window.IsActive) session.Window.Activate();
            }
        }

        #endregion

        #region Events

        private async void LongPoll_StateChanged(object sender, DataModels.LongPollState e) {
            await Dispatcher.UIThread.InvokeAsync(() => {
                if (e == DataModels.LongPollState.Working) {
                    Name = Name; // заставит триггерить PropertyChanged в главном окне.
                } else {
                    Window.Title = e.ToString();
                }
            });
        }

        #endregion

        #region Public

        public void GetToChat(int peerId) {
            ChatViewModel chat = CacheManager.GetChat(Id, peerId);
            if (chat == null) {
                chat = new ChatViewModel(this, peerId);
                CacheManager.Add(Id, chat);
            }
            CurrentOpenedChat = chat;
            CurrentOpenedChatChanged?.Invoke(this, chat.PeerId);
            chat.OnDisplayed();
            Window.SwitchToSide(true);
        }

        #endregion

        #region Static

        private static List<VKSession> _sessions = new List<VKSession>();
        public static IReadOnlyList<VKSession> Sessions { get => _sessions.AsReadOnly(); }
        public static VKSession Main { get => _sessions.FirstOrDefault(); }

        public static void StartUserSession(int userId, string accessToken) {
            VKSession session = new VKSession {
                UserId = userId,
                Name = "...",
                API = new VKAPI(userId, accessToken, Localizer.Instance["lang"], App.UserAgent),
                Window = new MainWindow()
            };
            session.LongPoll = new LongPoll(session.API, session.Id, session.GroupId);
            _sessions.Add(session);
            session.Window.DataContext = session;
            session.Init();
            session.Window.Show();
        }

        public static void StartDemoSession(DemoModeSession mainSession) {
            Dictionary<int, List<LongPollActivityInfo>> test = new Dictionary<int, List<LongPollActivityInfo>>();
            test.Add(2000000100, new List<LongPollActivityInfo> { new LongPollActivityInfo(172894294, LongPollActivityType.Typing) });
            string ss = JsonConvert.SerializeObject(test);

            CacheManager.Add(DemoMode.Data.Profiles);
            CacheManager.Add(DemoMode.Data.Groups);
            foreach (var ds in DemoMode.Data.Sessions) {
                var info = CacheManager.GetNameAndAvatar(ds.Id);
                VKSession session = new VKSession {
                    UserId = ds.Id,
                    Name = String.Join(" ", new List<string> { info.Item1, info.Item2 }),
                    Avatar = info.Item3,
                    API = new VKAPI(ds.Id, null, Localizer.Instance["lang"], App.UserAgent),
                    Window = new MainWindow()
                };
                _sessions.Add(session);
                session.Window.DataContext = session;
                if (mainSession.Id == session.Id) {
                    session.Init();
                    session.Window.Show();
                }
            }
            SetUpTrayMenu();
        }

        // Т. к. мы привязываем VKSession к окну, то
        // мы можем получить текущий инстанс VKSession,
        // проверив свойство DataContext у окна или user control
        public static VKSession GetByDataContext(Control control) {
            VKSession session = null;
            do {
                if (control.DataContext is VKSession s) {
                    session = s;
                } else {
                    control = (Control)control.Parent;
                }
            } while (session == null && control.GetType() != typeof(Window));
            return session;

            //if (control != null && control.DataContext is VKSession session) return session;
            //return null;
        }

        #endregion
    }
}
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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using ELOR.Laney.Core.Network;

namespace ELOR.Laney.Core {
    public sealed class VKSession : ViewModelBase {
        private string _name;
        private Uri? _avatar;
        private ConversationsViewModel _conversationsViewModel;
        private ChatViewModel _currentOpenedChat;

        public int Id { get { return GroupId > 0 ? -GroupId : UserId; } }
        public int UserId { get; private set; }
        public int GroupId { get; private set; }
        public string Name { get { return _name; } private set { _name = value; OnPropertyChanged(); } }
        public Uri? Avatar { get { return _avatar; } private set { _avatar = value; OnPropertyChanged(); } }
        public ConversationsViewModel ConversationsViewModel { get { return _conversationsViewModel; } private set { _conversationsViewModel = value; OnPropertyChanged(); } }
        public ChatViewModel CurrentOpenedChat { get { return _currentOpenedChat; } set { _currentOpenedChat = value; OnPropertyChanged(); } }

        public bool IsGroup { get => GroupId > 0; }
        public VKAPI API { get; private set; }
        public MainWindow Window { get; private set; }

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
                        Width = 32,
                        Height = 32
                    },
                    Header = session.Name,
                    Tag = session.Id
                };
                item.Click += TryOpenSessionWindow;
                ash.Items.Add(item);
            }

            if (ash.Items.Count > 0) ash.Items.Add(new ActionSheetItem());

            ActionSheetItem settings = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon28SettingsOutline },
                Header = Localizer.Instance["settings"],
            };
            ActionSheetItem about = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon28InfoOutline },
                Header = Localizer.Instance["about"],
            };
            ActionSheetItem logout = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon28DoorArrowRightOutline },
                Header = Localizer.Instance["log_out"],
            };
            logout.Classes.Add("Destructive");

            ash.Items.Add(settings);
            ash.Items.Add(about);
            ash.Items.Add(logout);

            ash.ShowAt(owner);
        }

        private void SetUpTrayMenu() {
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

            var exit = new NativeMenuItem { Header = "Exit" };
            exit.Click += (a, b) => {
                App.Current.DesktopLifetime.Shutdown();
            };

            menu.Items.Add(ft);
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

                if (API.WebRequestCallback == null) API.WebRequestCallback = LNetExtensions.SendRequestToAPIViaLNetAsync;

                List<VKSession> sessions = new List<VKSession>();
                StartSessionResponse info = await API.StartSessionAsync();

                // Условие должно выполниться всего один раз после запуска первой сессии
                // (обязательно юзер, а не сообщество), здесь происходит создание списка сессий
                // для отображения их в меню и в tray-menu.
                if (!dontUpdateSessionsList) {
                    sessions.Add(this);

                    foreach (var group in info.Groups) {
                        if (!group.CanMessage) continue;
                        sessions.Add(new VKSession {
                            UserId = info.User.Id,
                            GroupId = group.Id,
                            Name = group.Name,
                            Avatar = new Uri(group.Photo100),
                            API = new VKAPI(info.User.Id, API.AccessToken, Localizer.Instance["lang"], App.UserAgent)
                        });
                    }

                    _sessions = sessions;
                }

                if (!IsGroup) {
                    var currentUser = info.User;
                    Name = currentUser.FullName;
                    Avatar = new Uri(currentUser.Photo100);
                } else {
                    var currentGroup = _sessions.Where(s => s.Id == Id).FirstOrDefault();
                    Name = currentGroup.Name;
                    Avatar = currentGroup.Avatar;
                }
                SetUpTrayMenu(); // обновляем tray menu, отображая уже все загружнные сессии
            } catch (Exception ex) {
                Log.Error(ex, "Init failed. Waiting 3 sec. before trying again...");
                await Task.Delay(3000);
                Init(dontUpdateSessionsList);
            }

            if (ConversationsViewModel == null) ConversationsViewModel = new ConversationsViewModel(this);
        }

        private void TryOpenSessionWindow(object? sender, RoutedEventArgs e) {
            ActionSheetItem item = sender as ActionSheetItem;
            int sessionId = (int)item.Tag;
            TryOpenSessionWindow(sessionId);
        }

        private void TryOpenSessionWindow(int sessionId) {
            VKSession session = _sessions.Where(s => s.Id == sessionId).FirstOrDefault();

            if (session.Window == null) {
                Log.Information("Creating and showing new window for session {0}", Id);
                session.Window = new MainWindow();
                session.Window.DataContext = session;
                session.Init(true);
                session.Window.Show();
            } else {
                Log.Information("Showing/activating window for session {0}", Id);
                if (!session.Window.IsVisible) session.Window.Show();
                if (!session.Window.IsActive) session.Window.Activate();
            }
        }

        #endregion

        #region Public

        public void TriggerPropertyChangedEvent(string name) {
            OnPropertyChanged(name);
        }

        public void GetToChat(int peerId) {
            ChatViewModel chat = CacheManager.GetChat(Id, peerId);
            if (chat == null) {
                chat = new ChatViewModel(peerId);
                CacheManager.Add(Id, chat);
            }
            CurrentOpenedChat = chat;
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
            _sessions.Add(session);
            session.Window.DataContext = session;
            session.Init();
            session.Window.Show();
        }

        // Т. к. мы привязываем VKSession к окну, то
        // мы можем получить текущий инстанс VKSession,
        // проверив свойство DataContext у окна или user control
        public static VKSession GetByDataContext(Avalonia.Controls.Control control) {
            if (control != null && control.DataContext is VKSession session) return session;
            return null;
        }

        #endregion
    }
}
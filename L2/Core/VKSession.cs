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
using ELOR.Laney.Views.Modals;
using ELOR.VKAPILib.Objects.HandlerDatas;
using ELOR.Laney.Helpers;
using Avalonia.Controls.Notifications;
using ELOR.Laney.ViewModels.Controls;
using ELOR.Laney.ViewModels.Modals;
using ToastNotifications.Avalonia;
using Avalonia.Dialogs;
using ELOR.VKAPILib.Objects.Messages;
using System.Diagnostics;
using Avalonia.Platform;
using System.Threading;

namespace ELOR.Laney.Core {
    public sealed class VKSession : ViewModelBase {
        private string _name;
        private Uri? _avatar;
        private ImViewModel _imViewModel;
        private ChatViewModel _currentOpenedChat;

        public long Id { get { return GroupId > 0 ? -GroupId : UserId; } }
        public long UserId { get; private set; }
        public long GroupId { get; private set; }
        public string Name { get { return _name; } private set { _name = value; OnPropertyChanged(); } }
        public Uri? Avatar { get { return _avatar; } private set { _avatar = value; OnPropertyChanged(); } }
        public ImViewModel ImViewModel { get { return _imViewModel; } private set { _imViewModel = value; OnPropertyChanged(); } }
        public ChatViewModel CurrentOpenedChat { get { return _currentOpenedChat; } set { _currentOpenedChat = value; OnPropertyChanged(); } }

        public bool IsGroup { get => GroupId > 0; }
        public VKAPI API { get; private set; }
        public LongPoll LongPoll { get; private set; }
        public MainWindow Window { get; private set; }
        public Window ModalWindow { get => GetLastOpenedModalWindow(Window); }

        public event EventHandler<long> CurrentOpenedChatChanged;

        private WindowNotificationManager _notificationManager;
        private static ToastNotificationsManager _systemNotificationManager;

        public List<MessageTemplate> MessageTemplates { get; private set; } = new List<MessageTemplate>();

        #region Binded from UI and tray menu

        int sysNotifTest = 0;
        public void ShowSessionPopup(Button owner) {
            ActionSheet ash = new ActionSheet {
                Placement = PlacementMode.BottomEdgeAlignedLeft
            };

            foreach (var session in Sessions) {
                if (session.Id == Id) continue;
                Avatar ava = new Avatar {
                    Initials = session.Name.GetInitials(session.IsGroup),
                    Background = session.Id.GetGradient(),
                    Foreground = new SolidColorBrush(Colors.White),
                    Width = 20,
                    Height = 20
                };
                ava.SetImageAsync(session.Avatar, ava.Width, ava.Height);
                ActionSheetItem item = new ActionSheetItem {
                    Before = ava,
                    Header = session.Name,
                    Tag = session.Id
                };
                item.Click += TryOpenSessionWindow;
                ash.Items.Add(item);
            }

            if (!IsGroup && !DemoMode.IsEnabled) {
                ActionSheetItem chooseGroups = new ActionSheetItem {
                    Before = new VKIcon { Id = VKIconNames.Icon20More },
                    Header = Localizer.Instance["groups_management"],
                };
                chooseGroups.Click += ChooseGroups_Click;
                ash.Items.Add(chooseGroups);
            }

            //

            if (ash.Items.Count > 0) ash.Items.Add(new ActionSheetItem());

            ActionSheetItem favorites = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20BookmarkOutline },
                Header = Localizer.Instance["favorite_messages"],
            };
            ActionSheetItem important = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20FavoriteOutline },
                Header = Localizer.Instance["important_messages"],
            };

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

            favorites.Click += (a, b) => GoToChat(UserId);
            important.Click += async (a, b) => {
                ImportantMessages im = new ImportantMessages(this);
                var result = await im.ShowDialog<Tuple<long, int>>(Window);
                if (result != null) GoToChat(result.Item1, result.Item2);
            };

            settings.Click += async (a, b) => {
                SettingsWindow sw = new SettingsWindow();
                await sw.ShowDialog(Window);
            };
            about.Click += async (a, b) => {
                About aw = new About();
                await aw.ShowDialog(Window);
            };
            about.ContextRequested += async (a, b) => {
                await new AboutAvaloniaDialog().ShowDialog(Window);
            };
            logout.Click += async (a, b) => {
                if (DemoMode.IsEnabled) return;
                string[] buttons = [Localizer.Instance["yes"], Localizer.Instance["no"]];
                VKUIDialog dlg = new VKUIDialog(Localizer.Instance["log_out"], Localizer.Instance["log_out_confirm"], buttons, 2);
                int result = await dlg.ShowDialog<int>(Window);

                if (result == 1) LogOut();
            };

            if (!IsGroup && !DemoMode.IsEnabled) {
                ash.Items.Add(important);
                ash.Items.Add(favorites);
                ash.Items.Add(new ActionSheetItem());
            }
            ash.Items.Add(settings);
            ash.Items.Add(about);
            ash.Items.Add(logout);

#if RELEASE
#elif BETA
#else

            List<ActionSheetItem> devmenu = new List<ActionSheetItem>();
            if (!DemoMode.IsEnabled) {
                ActionSheetItem captcha = new ActionSheetItem {
                    Before = new VKIcon { Id = VKIconNames.Icon20GearOutline },
                    Header = "Show captcha",
                };
                captcha.Click += async (a, b) => {
                    try {
                        int i = await API.CallMethodAsync<int>("captcha.force");
                        await new VKUIDialog("Result", i.ToString()).ShowDialog<int>(ModalWindow);
                    } catch (Exception ex) {
                        await ExceptionHelper.ShowErrorDialogAsync(ModalWindow, ex, true);
                    }
                };
                devmenu.Add(captcha);

                ActionSheetItem notif = new ActionSheetItem {
                    Before = new VKIcon { Id = VKIconNames.Icon20ArticleOutline },
                    Header = "Show in-app notification",
                };
                notif.Click += (a, b) => {
                    ShowNotification(new Notification("Header", null, NotificationType.Success, TimeSpan.FromSeconds(10)));
                };
                devmenu.Add(notif);

                ActionSheetItem snotif = new ActionSheetItem {
                    Before = new VKIcon { Id = VKIconNames.Icon20NotificationOutline },
                    Header = "Show system notification",
                };
                snotif.Click += async (a, b) => {
                    sysNotifTest++;
                    var ava = await BitmapManager.GetBitmapAsync(new Uri("https://elor.top/res/images/rez_ava.png"));
					var img = await BitmapManager.GetBitmapAsync(new Uri("https://elor.top/res/images/gex_holmes.png"));
                    var t = new ToastNotification(sysNotifTest, Name, $"Rez ({sysNotifTest})", "Hurray for bunny Gex!\nHe sure is a funny Gex!", "in chat\"Geckos\"", ava, img);
                    t.OnClick += async () => {
                        await new VKUIDialog("Result", t.AssociatedObject.ToString()).ShowDialog<int>(ModalWindow);
                    };
                    t.OnSendClick += async (text) => {
                        await new VKUIDialog("Sending message", t.AssociatedObject.ToString() + "\nText: " + text).ShowDialog<int>(ModalWindow);
                    };
                    _systemNotificationManager?.Show(t);
                };
                devmenu.Add(snotif);

                ActionSheetItem imgc = new ActionSheetItem {
                    Before = new VKIcon { Id = VKIconNames.Icon20DeleteOutline },
                    Header = "Clear images cache",
                };
                imgc.Click += (a, b) => BitmapManager.ClearCachedImages();
                devmenu.Add(imgc);

                ActionSheetItem csc = new ActionSheetItem {
                    Before = new VKIcon { Id = VKIconNames.Icon20DeleteOutline },
                    Header = "Clear cached user/group names",
                };
                csc.Click += (a, b) => {
                    CacheManager.ClearUsersAndGroupsCache();
                };
                devmenu.Add(csc);

                ActionSheetItem stemw = new ActionSheetItem {
                    Before = new VKIcon { Id = VKIconNames.Icon20GearOutline },
                    Header = "Open emoji/stickers panel in separated window",
                };
                stemw.Click += (a, b) => {
                    Window stemwnd = new Window {
                        CanResize = false,
                        Width = 400,
                        Height = 438,
                        Content = new Controls.EmojiStickerPicker {
                            Width = 400,
                            Height = 438,
                            DataContext = new EmojiStickerPickerViewModel(this)
                        },
                        Title = "Emoji & stickers"
                    };
                    stemwnd.Show();
#if DEBUG
                    stemwnd.AttachDevTools();
#endif
                };
                devmenu.Add(stemw);
            }

            if (devmenu.Count > 0) {
                ash.Items.Add(new ActionSheetItem());
                foreach (var item in devmenu) {
                    ash.Items.Add(item);
                }
            }

#endif

            ash.ShowAt(owner);
        }

        public static NativeMenu TrayMenu { get; private set; }

#if MAC
        private static void SetUpTrayMenu() {
            TrayMenu = new NativeMenu();

            foreach (var session in Sessions) {
                var item = new NativeMenuItem { Header = session.Name };
                item.Click += (a, b) => TryOpenSessionWindow(session.Id);
                TrayMenu.Items.Add(item);
            }

            TrayMenu.Items.Add(new NativeMenuItemSeparator());

            var ft = new NativeMenuItem { Header = "Field test" };
            ft.Click += (a, b) => {
                new FieldTestWindow().Show();
            };

            var exit = new NativeMenuItem { Header = Localizer.Instance["exit"] };
            exit.Click += (a, b) => {
                App.Current.DesktopLifetime.Shutdown();
            };

#if !RELEASE && !BETA
            TrayMenu.Items.Add(ft);
#endif
            TrayMenu.Items.Add(exit);

            TrayIcon icon = new TrayIcon {
                Icon = new WindowIcon(AssetsManager.GetBitmapFromUri(new Uri(AssetsManager.GetThemeDependentTrayIcon()))),
                Menu = TrayMenu,
                IsVisible = true,
                ToolTipText = "Laney"
            };
            
            icon.Clicked += (a, b) => {
                if (lastSessionId == 0) return;
                var s = Sessions.Where(s => s.Id == lastSessionId).FirstOrDefault();
                if (s != null) s.TryOpenWindow();
            };

            var icons = new TrayIcons { icon };
            Application.Current.SetValue(TrayIcon.IconsProperty, icons);
        }
#else
        private static void SetUpTrayMenu() {
            TrayMenu = new NativeMenu();

            foreach (var session in Sessions) {
                var item = new NativeMenuItem { Header = session.Name };
                item.Click += (a, b) => TryOpenSessionWindow(session.Id);
                TrayMenu.Items.Add(item);
            }

            TrayMenu.Items.Add(new NativeMenuItemSeparator());

            var ft = new NativeMenuItem { Header = "Field test" };
            ft.Click += (a, b) => {
                new FieldTestWindow().Show();
            };

            var exit = new NativeMenuItem { Header = Localizer.Instance["exit"] };
            exit.Click += (a, b) => {
                App.Current.DesktopLifetime.Shutdown();
            };

#if !RELEASE && !BETA
            TrayMenu.Items.Add(ft);
#endif
            TrayMenu.Items.Add(exit);

            TrayIcons icons = Application.Current.GetValue<TrayIcons>(TrayIcon.IconsProperty);

            if (icons != null && icons.Count == 1) {
                icons[0].Menu = TrayMenu;
            } else {
                WindowIcon wi = null;
                try {
                    wi = new WindowIcon(AssetLoader.Open(new Uri(AssetsManager.GetThemeDependentTrayIcon())));
                } catch (Exception ex) {
                    Log.Error(ex, "Failed to open a tray icon!");
                }

                TrayIcon icon = new TrayIcon {
                    Icon = wi,
                    Menu = TrayMenu,
                    IsVisible = true,
                    ToolTipText = "Laney"
                };
                icon.Clicked += (a, b) => {
                    Log.Information($"User clicked on tray icon. Last displayed session id = {lastSessionId}");
                    if (lastSessionId == 0) return;
                    var s = Sessions.Where(s => s.Id == lastSessionId).FirstOrDefault();
                    if (s != null) s.TryOpenWindow();
                };

                icons = new TrayIcons { icon };
                Application.Current.SetValue(TrayIcon.IconsProperty, icons);
            }
        }
#endif

#endregion

        #region Internal

        bool isFirstTimeChatsLoaded = false;
        private async void Init(bool dontUpdateSessionsList = false) {
            try {
                Log.Information("Init session ({0})", Id);
                SetUpTrayMenu(); // Чтобы можно было закрыть приложение, если будут проблемы с загрузкой
                Window.Activated += Window_Activated;

                if (DemoMode.IsEnabled) {
                    ImViewModel = new ImViewModel(this);
                    ImViewModel.LoadConversations();
                    return;
                }
                API.CaptchaHandler = ShowCaptcha;
                if (API.WebRequestCallback == null) API.WebRequestCallback = LNetExtensions.SendRequestToAPIViaLNetAsync;

                List<VKSession> sessions = new List<VKSession>();
                List<long> savedGroupIds = GetAddedGroupIds();
                StartSessionResponse info = await API.StartSessionAsync(savedGroupIds);

                // Условие должно выполниться всего один раз после запуска первой сессии
                // (обязательно юзер, а не сообщество), здесь происходит создание списка сессий
                // для отображения их в меню и в tray-menu.
                if (!dontUpdateSessionsList) {
                    sessions.Add(this);

                    foreach (var group in info.Groups) {
                        CacheManager.Add(group);
                        if (group.CanMessage == 0) continue;

                        VKSession gs = new VKSession {
                            UserId = info.User.Id,
                            GroupId = group.Id,
                            Name = group.Name,
                            Avatar = new Uri(group.Photo100),
                            API = new VKAPI(info.User.Id, API.AccessToken, Localizer.Instance["lang"], App.UserAgent)
                        };
                        gs.LongPoll = new LongPoll(gs.API, gs.Id, gs.GroupId);
                        gs.ImViewModel = new ImViewModel(gs);
                        sessions.Add(gs);

                        var tmp = info.Templates.Where(tmps => tmps.GroupId == group.Id).FirstOrDefault();
                        if (tmp != null) {
                            gs.MessageTemplates = tmp.Items;
                        } else {
                            Log.Warning($"VKSession > Init: Message templates for group {group.Id} not found in response!");
                        }

                        var glp = info.LongPolls.Where(lps => lps.SessionId == group.Id).FirstOrDefault();
                        if (glp != null) {
                            gs.SetUpLongPoll(glp);
                        } else {
                            Log.Warning($"VKSession > Init: LongPoll for group {group.Id} not found in response!");
                        }
                    }

                    _sessions = sessions;

                    // Set online
                    // TODO: сделать параметр для юзера, который позволил бы включить/отключить
                    // отправку онлайна, когда окно закрыто.
                    Thread thread = new Thread(() => {
                        System.Timers.Timer onlineTimer = new System.Timers.Timer(TimeSpan.FromMinutes(4)) {
                            Enabled = true,
                            AutoReset = true,
                        };
                        onlineTimer.Elapsed += async (a, b) => {
                            try {
                                bool result = await API.Account.SetOnlineAsync();
                                Log.Information($"account.setOnline: {result}");
                            } catch (Exception ex) {
                                Log.Error(ex, "An error occured while calling account.setOnline!");
                            }
                        };
                        onlineTimer.Start();
                    });
                    thread.Start();
                }

                if (!IsGroup) {
                    var currentUser = info.User;
                    CacheManager.Add(currentUser);

                    Name = currentUser.FullName;
                    Avatar = new Uri(currentUser.Photo100);

                    VKQueue.Init(info.QueueConfig);
                } else {
                    var currentGroup = _sessions.Where(s => s.Id == Id).FirstOrDefault();
                    Name = currentGroup.Name;
                    Avatar = currentGroup.Avatar;
                }

                CacheManager.SetReactionsAssets(info.ReactionsAssets);

                var lp = info.LongPolls.Where(lps => lps.SessionId == Id).FirstOrDefault();
                SetUpLongPoll(lp);

                // Notifications
                var appLogo = await BitmapManager.GetBitmapAsync(new Uri("avares://laney/Assets/Logo/Tray/t32cw.png"), 16, 16);
                if (_systemNotificationManager == null) _systemNotificationManager = new ToastNotificationsManager(appLogo, (log) => Log.Information($"[CSToast] {log}"));

                SetUpTrayMenu(); // обновляем tray menu, отображая уже все загружнные сессии
            } catch (Exception ex) {
                if (_sessions == null || _sessions.Count == 0) {
                    Log.Error(ex, "Init failed! Waiting 3 sec. before trying again...");
                    await Task.Delay(3000);
                    Init(dontUpdateSessionsList);
                } else {
                    Log.Error(ex, "Init failed!");
                }
            }

            // Load chats
            if (!isFirstTimeChatsLoaded) {
                ImViewModel.LoadConversations();
                isFirstTimeChatsLoaded = true;
            }
        }

        private void SetUpLongPoll(LongPollInfoForSession lp) {
            if (LongPoll == null) LongPoll = new LongPoll(API, Id, GroupId);
            LongPoll.SetUp(lp.LongPoll);
            LongPoll.StateChanged += LongPoll_StateChanged;
            LongPoll.Run();
        }

        private void Window_Activated(object sender, EventArgs e) {
            (sender as Window).Activated -= Window_Activated;
            _notificationManager = new WindowNotificationManager(Window) {
                Position = NotificationPosition.BottomLeft
            };
        }

        private async Task<string> ShowCaptcha(CaptchaHandlerData arg) {
            return await ShowCaptchaAsync(Window, arg.Image);
        }

        private async void ChooseGroups_Click(object sender, RoutedEventArgs e) {
            GroupsPicker gp = new GroupsPicker(this);
            List<long> selectedGroupIds = await gp.ShowDialog<List<long>>(Window);
            if (selectedGroupIds == null) return;
            Settings.Set(Settings.GROUPS, String.Join(',', selectedGroupIds));

            UpdateGroupSessions(selectedGroupIds);
        }

        private static void TryOpenSessionWindow(object? sender, RoutedEventArgs e) {
            ActionSheetItem item = sender as ActionSheetItem;
            long sessionId = (long)item.Tag;
            TryOpenSessionWindow(sessionId);
        }

        private static void TryOpenSessionWindow(long sessionId) {
            VKSession session = _sessions.Where(s => s.Id == sessionId).FirstOrDefault();

            if (session.Window == null) {
                Log.Information("Creating and showing new window for session {0}", sessionId);
                session.Window = new MainWindow();
                session.Window.DataContext = session;
                session.Window.Activated += (a, b) => lastSessionId = ((a as Window).DataContext as VKSession).Id;
                session.Init(true);
                session.Window.Show();
            } else {
                Log.Information("Showing/activating window for session {0}", sessionId);
                session.ShowAndActivate();
            }
        }

        private void ShowAndActivate() {
            if (!Window.IsVisible) Window.Show();
            if (!Window.IsActive) Window.Activate();
        }

        private Window GetLastOpenedModalWindow(Window window) {
            // В приложении главное окно может иметь только одно дочернее (диалоговое) окно.
            var ows = window?.OwnedWindows;
            if (ows == null) return null;
            if (ows.Count == 0) return window;
            if (ows.Count > 1) throw new ArgumentException("Session's main window cannot have 2 and more child windows!");

            var fow = ows[0];
            return GetLastOpenedModalWindow(fow);
        }

        private async void UpdateGroupSessions(List<long> groupIds) {
            try {
                foreach (VKSession s in _sessions) {
                    if (s.IsGroup) { // TODO: Shutdown method for VKSession.
                        s.LongPoll.Stop();
                        s.ModalWindow?.Close();
                        s.Window?.Close();
                        s.Window = null;
                        s.CurrentOpenedChat = null;
                        BitmapManager.ClearCachedImages(); // free RAM.
                    }
                }

                List<VKSession> sessions = new List<VKSession> { Main };

                var wd = new VKUIWaitDialog<StartSessionResponse>();
                StartSessionResponse response = await wd.ShowAsync(Window, API.GetGroupsWithLongPollAsync(groupIds));

                foreach (var group in response.Groups) {
                    CacheManager.Add(group);
                    if (group.CanMessage == 0) continue;

                    VKSession gs = new VKSession {
                        UserId = Main.Id,
                        GroupId = group.Id,
                        Name = group.Name,
                        Avatar = new Uri(group.Photo100),
                        API = new VKAPI(Main.Id, API.AccessToken, Localizer.Instance["lang"], App.UserAgent),
                    };
                    gs.LongPoll = new LongPoll(gs.API, gs.Id, gs.GroupId);
                    sessions.Add(gs);

                    var tmp = response.Templates.Where(tmps => tmps.GroupId == group.Id).FirstOrDefault();
                    if (tmp != null) {
                        gs.MessageTemplates = tmp.Items;
                    } else {
                        Log.Warning($"VKSession > UpdateGroupSessions: Message templates for group {group.Id} not found in response!");
                    }

                    var glp = response.LongPolls.Where(lps => lps.SessionId == group.Id).FirstOrDefault();
                    if (glp != null) {
                        gs.SetUpLongPoll(glp);
                    } else {
                        Log.Warning($"VKSession > UpdateGroupSessions: LongPoll for group {group.Id} not found in response!");
                    }
                }

                _sessions = sessions;
                SetUpTrayMenu();
            } catch (Exception ex) {
                if (await ExceptionHelper.ShowErrorDialogAsync(Window, ex)) UpdateGroupSessions(groupIds);
            }
        }

        #endregion

        #region Events

        private async void LongPoll_StateChanged(object sender, LongPollState e) {
            if (Window == null) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                if (e == LongPollState.Working) {
                    Name = Name; // заставит триггерить PropertyChanged в главном окне.
                } else {
                    Window.Title = e.ToString();
                }
            });
        }

        #endregion

        #region Public

        byte gcCollectTriggerCounter = 0;

        public void GoToChat(long peerId, int messageId = -1) {
            if (peerId == 0) {
                CurrentOpenedChat = null;
                CurrentOpenedChatChanged?.Invoke(this, 0);
                Window.SwitchToSide(false);
                return;
            }

            ChatViewModel chat = CacheManager.GetChat(Id, peerId);
            Log.Information("VKSession: getting to chat {0}. cmid: {1}; cached: {2}", peerId, messageId, chat != null);
            if (chat == null) {
                chat = new ChatViewModel(this, peerId);
                CacheManager.Add(Id, chat);
            }
            CurrentOpenedChat = chat;
            CurrentOpenedChatChanged?.Invoke(this, chat.PeerId);
            chat.OnDisplayed(messageId);
            Window.SwitchToSide(true);
            if (gcCollectTriggerCounter >= 2) {
                gcCollectTriggerCounter = 0;
                BitmapManager.ClearCachedImages();
            } else {
                gcCollectTriggerCounter++;
            }
        }

        public async void Share(long fromPeerId, List<MessageViewModel> messages) {
            if (DemoMode.IsEnabled) return;
            SharingViewModel user = new SharingViewModel(Main, GroupId);
            SharingViewModel group = IsGroup ? new SharingViewModel(this, 0) : null;
            SharingView dlg = new SharingView(user, group);
            // session, peer_id, group_id (if message from group to user session)
            Tuple<VKSession, long, long>  result = await dlg.ShowDialog<Tuple<VKSession, long, long>>(ModalWindow);

            if (result != null) {
                result.Item1.ShowAndActivate();
                result.Item1.GoToChat(result.Item2);
                result.Item1.CurrentOpenedChat.Composer.AddForwardedMessages(fromPeerId, messages, result.Item3);
            }
        }

        public void TryOpenWindow() {
            TryOpenSessionWindow(Id);
        }

        public void ShowNotification(Notification notification) {
            _notificationManager?.Show(notification);
        }

        public void ShowSystemNotification(ToastNotification notification) {
            _systemNotificationManager?.Show(notification);
        }

        #endregion

        #region Static

        private static List<VKSession> _sessions = new List<VKSession>();
        public static IReadOnlyList<VKSession> Sessions { get => _sessions.AsReadOnly(); }
        public static VKSession Main { get => _sessions.FirstOrDefault(); }
        private static long lastSessionId = 0;

        public static async void StartUserSession(long userId, string accessToken) {
            VKSession session = new VKSession {
                UserId = userId,
                Name = "...",
                API = new VKAPI(userId, accessToken, Localizer.Instance["lang"], App.UserAgent),
                Window = new MainWindow()
            };
            session.LongPoll = new LongPoll(session.API, session.Id, session.GroupId);
            _sessions.Add(session);
            session.Window.DataContext = session;
            session.ImViewModel = new ImViewModel(session);
            session.Window.Activated += (a, b) => lastSessionId = ((a as Window).DataContext as VKSession).Id;
            session.Init();
            session.Window.Show();

            Settings.SettingChanged += Settings_SettingChanged;

            await Task.Delay(2000); // чтобы метод api не выполнялся одновременно с другими и не поймать ошибку 6.
            StickersManager.InitKeywords();
        }

        private static void Settings_SettingChanged(string key, object value) {
            switch (key) {
                case Settings.STICKERS_SUGGEST:
                    StickersManager.InitKeywords();
                    break;
            }
        }

        public static void StartDemoSession(DemoModeSession mainSession) {
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

        public static async void LogOut() {
            Settings.SetBatch(new Dictionary<string, object> {
                { Settings.VK_USER_ID, null },
                { Settings.VK_TOKEN, null }
            });

            var cprc = Process.GetCurrentProcess();
            Process.Start(cprc.MainModule.FileName, Environment.CommandLine.Replace(" -delay=1000", "") + " -delay=1000");
            await Task.Delay(200);
            App.Current.DesktopLifetime.Shutdown(-1);
        }

        public static async Task<string> ShowCaptchaAsync(Window parent, Uri image) {
            return await Task.Factory.StartNew(() => {
                string code = null;

                Dispatcher.UIThread.InvokeAsync(async () => {
                    Image captchaImg = new Image {
                        Width = 130,
                        Height = 50
                    };
                    captchaImg.SetUriSourceAsync(image, captchaImg.Width, captchaImg.Height);
                    TextBox codeTxt = new TextBox {
                        Width = 130,
                        MaxLength = 10,
                        Margin = new Thickness(0, 12, 0, 0)
                    };

                    StackPanel panel = new StackPanel {
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                    };
                    panel.Children.Add(captchaImg);
                    panel.Children.Add(codeTxt);

                    VKUIDialog dialog = new VKUIDialog("Enter code", null);
                    dialog.DialogContent = panel;
                    int result = await dialog.ShowDialog<int>(parent);
                    if (result == 1) code = codeTxt.Text;
                }).Wait();

                return code;
            });
        }

        public static List<long> GetAddedGroupIds() {
            List<long> ids = new List<long>();
            try {
                string str = Settings.Get(Settings.GROUPS, "");
                var split = str.Split(',');
                foreach (string sid in split) {
                    long gid = 0;
                    if (Int64.TryParse(sid, out gid)) ids.Add(gid);
                }
            } catch (Exception ex) {
                Log.Error(ex, "VKSession: cannot get added groups!");
            }
            return ids;
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
                    if (control == null) return null;
                }
            } while (session == null && control.GetType() != typeof(Window));
            return session;
        }

        #endregion
    }
}
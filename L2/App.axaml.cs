using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Styling;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.Views.Modals;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ELOR.Laney {
    public sealed class App : Application {
        private static App _current;
        public static new App Current => _current;
        public ClassicDesktopStyleApplicationLifetime DesktopLifetime { get; private set; }
        public double DPI { get; private set; } = 2;

        public override void Initialize() {
            _current = this;

            AvaloniaXamlLoader.Load(this);

            this.ActualThemeVariantChanged += App_ActualThemeVariantChanged;
            ChangeTheme(Settings.AppTheme);
            Settings.SettingChanged += Settings_SettingChanged;
        }

        public static bool HasCmdLineValue(string key) {
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args) {
                if (arg.StartsWith($"-{key}")) return true;
            }
            return false;
        }

        public static string GetCmdLineValue(string key) {
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args) {
                if (arg.StartsWith($"-{key}")) {
                    string[] p = arg.Split("=");
                    if (p.Length == 2) return p[1];
                }
            }
            return null;
        }

        public override void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is ClassicDesktopStyleApplicationLifetime desktop) {
                DesktopLifetime = desktop;
                if (Platform == OSPlatform.FreeBSD) desktop.Shutdown();
                Prepare();
                if (DesktopLifetime.MainWindow != null) return; // признак того, что открыто окно expired info.

                if (Program.Mode == LaunchMode.Default) {
                    // Demo mode
                    if (DemoMode.Check()) {
                        var usessions = DemoMode.Data.Sessions.Where(s => s.Id.IsUser());
                        int ucount = usessions.Count();
                        if (ucount == 0) throw new Exception("No user session found!");
                        if (ucount > 1) throw new Exception("There can be only 1 user session!");
                        VKSession.StartDemoSession(usessions.FirstOrDefault());
                        desktop.MainWindow = VKSession.Main.Window;
                    } else {
                        try {
                            long uid = Settings.Get<long>(Settings.VK_USER_ID);
                            string token = Settings.Get<string>(Settings.VK_TOKEN);
                            string nonce = Settings.Get<string>(Settings.VK_TOKEN + "1");
                            string tag = Settings.Get<string>(Settings.VK_TOKEN + "2");
                            string dt = Encryption.Decrypt(AssetsManager.BinaryPayload.Skip(576).Take(32).OrderDescending().ToArray(), token, nonce, tag);
                            if (uid > 0 && !String.IsNullOrEmpty(dt)) {
                                Log.Information($"Authorized user: {uid}");
                                VKSession.StartUserSession(uid, dt);
                                desktop.MainWindow = VKSession.Main.Window;
                            } else {
                                Log.Information($"Not authorized. Opening sign in window...");
                                desktop.MainWindow = new Views.SignInWindow();
                            }
                        } catch (Exception ex) {
                            Log.Error(ex, $"Cannot check authorization. Opening sign in window...");
                            desktop.MainWindow = new Views.SignInWindow();
                        }
                    }
                    PlatformSettings.ColorValuesChanged += (a, b) => UpdateTrayIcon();
                } else if (Program.Mode == LaunchMode.APIConsole) {
                    desktop.MainWindow = new APIConsoleWindow();
                }

                DPI = desktop.MainWindow.DesktopScaling;
                Log.Information($"Maximal DPI: {DPI}");
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void Prepare() {
            Debug.WriteLine("Getting and loading language...");
            string lang = Settings.Get(Settings.LANGUAGE, Constants.DefaultLang);
            bool langLoaded = Localizer.Instance.LoadLanguage(lang);
            Debug.WriteLine("Language loaded!");

            AudioPlayer.InitInstances();

#if RELEASE
#else
            if (IsExpired) {
                DesktopLifetime.MainWindow = new VKUIDialog(Localizer.Instance["error"], "This version is expired!");
                DesktopLifetime.MainWindow.Closed += (a, b) => Process.GetCurrentProcess().Kill();
                DesktopLifetime.MainWindow.Show();
            }

            // Checking all avalonia resources.
            // Надо для сравнения между обычной компиляцией и AOT
            // var uris = AssetLoader.GetAssets(new Uri("avares://laney/"), new Uri("avares://laney/"));
            // Log.Information("Found resources:");
            // foreach (var uri in uris) {
            //     Log.Information($"> {uri}");
            // }
            
#endif
        }

        private static void UpdateTrayIcon() {
            TrayIcons icons = Application.Current.GetValue(TrayIcon.IconsProperty);
            if (icons != null && icons.Count > 0) {
                TrayIcon icon = icons[0];
                icon.Icon = new WindowIcon(AssetsManager.GetBitmapFromUri(new Uri(AssetsManager.GetThemeDependentTrayIcon())));
            }
        }

        #region Theme

        public List<Action<ThemeVariant>> ThemeChangedActions = new List<Action<ThemeVariant>>();

        private void App_ActualThemeVariantChanged(object sender, EventArgs e) {
            foreach (var action in CollectionsMarshal.AsSpan(_current.ThemeChangedActions)) {
                action?.Invoke(_current.ActualThemeVariant);
            }
        }

        public static void ChangeTheme(int id) {
            switch (id) {
                case 1:
                    _current.RequestedThemeVariant = ThemeVariant.Light;
                    break;
                case 2:
                    _current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                default:
                    _current.RequestedThemeVariant = ThemeVariant.Default;
                    break;
            }
        }

        public static T GetResource<T>(string key) {
            object resource = null;
            if (App.Current.TryFindResource(key, out resource) && resource is T) {
                return (T)resource;
            } else {
                if (resource == null) {
                    Log.Error("Resource \"{0}\" is not found!", key);
                } else {
                    Log.Error("Resource \"{0}\" is not {1}, but {2}", key, typeof(T), resource.GetType());
                }
                return default(T);
            }
        }

        #endregion

        private void Settings_SettingChanged(string key, object value) {
            switch (key) {
                case Settings.THEME:
                    ChangeTheme((int)value);
                    break;
            }
        }

        public DataTemplate GetCommonTemplate(string key) {
            // 12 - это порядковый номер (с нуля) CommonTemplates.axaml в App.axaml > MergedDictionaries
            var dictionary = App.Current.Resources.MergedDictionaries[12] as ResourceDictionary;
            return dictionary[key] as DataTemplate;
        }

        #region Platform, version and other infos

        public static OSPlatform Platform => GetCurrentPlatform();

        private static OSPlatform GetCurrentPlatform() {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return OSPlatform.Windows;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return OSPlatform.OSX;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return OSPlatform.Linux;
            return OSPlatform.FreeBSD; // not supported
        }

        private static string _buildInfo;
        public static string BuildInfoFull => _buildInfo ?? GetFullBuildInfo();
        public static string BuildInfo => GetBuildInfo();
        public static DateTime BuildTime => GetBuildTime();

#if RELEASE
#else
        public static DateTime ExpirationDate => BuildTime.Date.AddDays(60);
        public static bool IsExpired => DateTime.Now.Date > ExpirationDate;
#endif

        public static string UserAgent => GetUserAgent();

        private static string GetFullBuildInfo() {
            Assembly assembly = Assembly.GetEntryAssembly();
            if (assembly != null) {
                var attr = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                _buildInfo = attr;
                return attr;
            }
            return string.Empty;
        }

        private static string GetBuildInfo() {
            string ver = BuildInfoFull;
            string[] sections = ver.Split('-');
            return $"{sections[0]} {sections[1]}-{sections[2]}";
        }

        private static DateTime GetBuildTime() {
            string ver = BuildInfoFull;
            string[] sections = ver.Split('-');
            string datetime = $"{sections[4]}-{sections[5]}";
            if (datetime.Contains('+')) datetime = datetime.Split('+')[0];
            var date = DateTime.ParseExact(datetime, "yyMMdd-HHmm", CultureInfo.InvariantCulture);
            return date;
        }

        private static string GetUserAgent() {
            string ver = BuildInfoFull;
            string[] sections = ver.Split('-');
            return $"LaneyMessenger (2; {sections[0]}; {sections[1]}; {sections[2]})";
        }

        public static List<string> UsedLibs { get; } = new List<string> {
            "Avalonia.Labs.Qr",
            "Avalonia.Skia.Lottie by Wiesław Šoltés",
            "ColorTextBlock.Avalonia by whistyun",
            "jp2masa.Avalonia.Flexbox",
            "ManagedBass",
            "Serilog",
            "Unicode.net",
        };

#endregion

        #region Paths

        public static string LocalDataPath { get => GetLocalDataPath(); }

        private static string GetLocalDataPath() {
            string custom = GetCmdLineValue("ldp");
            if (!string.IsNullOrEmpty(custom) && Path.EndsInDirectorySeparator(custom)) {
                return custom;
            } else {
                string appdataroot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(appdataroot, "ELOR", "Laney");
            }
        }

        #endregion
    }
}
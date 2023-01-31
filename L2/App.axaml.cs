using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Themes.Simple;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Core.Network;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VKUI;

namespace ELOR.Laney {
    public sealed class App : Application {
        private static App _current;
        public static new App Current => _current;
        public ClassicDesktopStyleApplicationLifetime DesktopLifetime { get; private set; }
        public double DPI { get; private set; }

        public override void Initialize() {
            _current = this;

            AvaloniaXamlLoader.Load(this);

            ChangeTheme(Settings.AppTheme);
            Settings.SettingChanged += Settings_SettingChanged;
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
            string lang = Settings.Get(Settings.LANGUAGE, Constants.DefaultLang);
            Localizer.Instance.LoadLanguage(lang);
            if (ApplicationLifetime is ClassicDesktopStyleApplicationLifetime desktop) {
                DesktopLifetime = desktop;
                if (Platform == OSPlatform.FreeBSD) desktop.Shutdown();

                // Demo mode
                if (DemoMode.Check()) {
                    var usessions = DemoMode.Data.Sessions.Where(s => s.Id > 0);
                    int ucount = usessions.Count();
                    if (ucount == 0) throw new Exception("No user session found!");
                    if (ucount > 1) throw new Exception("There can be only 1 user session!");
                    VKSession.StartDemoSession(usessions.FirstOrDefault());
                    desktop.MainWindow = VKSession.Main.Window;
                } else {
                    int uid = Settings.Get<int>(Settings.VK_USER_ID);
                    string token = Settings.Get<string>(Settings.VK_TOKEN);
                    if (uid > 0 && !String.IsNullOrEmpty(token)) {
                        Log.Information($"Authorized user: {uid}");
                        VKSession.StartUserSession(uid, token);
                        desktop.MainWindow = VKSession.Main.Window;
                    } else {
                        Log.Information($"Not authorized. Opening sign in window...");
                        desktop.MainWindow = new Views.SignInWindow();
                    }
                }

                DPI = desktop.MainWindow.Screens.All.Select(s => s.Scaling).Max();
                Log.Information($"Maximal DPI: {DPI}");
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async Task<HttpResponseMessage> WebRequestCallback(Uri arg) {
            return await LNet.GetAsync(arg);
        }

        public VKUIScheme CurrentScheme { get; private set; }
        public List<Action<VKUIScheme>> ThemeChanged = new List<Action<VKUIScheme>>();

        public static void SwitchTheme(VKUIScheme scheme) {
            SimpleTheme simple = (SimpleTheme)_current.Styles[0]!;
            simple.Mode = scheme == VKUIScheme.BrightLight ? SimpleThemeMode.Light : SimpleThemeMode.Dark;

            VKUITheme.Current.Scheme = scheme;
            _current.CurrentScheme = scheme;
            foreach (Action<VKUIScheme> action in CollectionsMarshal.AsSpan(_current.ThemeChanged)) {
                action.Invoke(scheme);
            }
        }

        public static void ChangeTheme(int id) {
            switch (id) {
                case 1:
                    SwitchTheme(VKUIScheme.BrightLight);
                    break;
                case 2:
                    SwitchTheme(VKUIScheme.SpaceGray);
                    break;
            }
        }

        public static void ToggleTheme() {
            SwitchTheme(_current.CurrentScheme == VKUIScheme.BrightLight ? VKUIScheme.SpaceGray : VKUIScheme.BrightLight);
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

        private void Settings_SettingChanged(string key, object value) {
            switch (key) {
                case Settings.THEME:
                    ChangeTheme((int)value);
                    break;
            }
        }

        #region Platform

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

        private static string GetUserAgent() {
            string ver = BuildInfoFull;
            string[] sections = ver.Split('-');
            return $"LaneyMessenger (2; {sections[0]}; {sections[1]}; {sections[2]})";
        }

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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Themes.Simple;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Core.Network;
using System;
using System.Diagnostics;
using System.IO;
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

        public override void Initialize() {
            _current = this;

            AvaloniaXamlLoader.Load(this);
            SwitchTheme(VKUIScheme.BrightLight);

            // в macOS нельзя вроде запустить более одного процесса одной программы.
            if (Platform == OSPlatform.OSX) {
                Settings.Initialize();
            } else {
                if (!Design.IsDesignMode && IsAlreadyRunning()) Process.GetCurrentProcess().Kill();
            }
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

        private static bool IsAlreadyRunning() {
            try {
                Settings.Initialize();
                return false;
            } catch {
                return true;
            }
        }

        public override void OnFrameworkInitializationCompleted() {
            Localizer.Instance.LoadLanguage("ru-RU");
            if (ApplicationLifetime is ClassicDesktopStyleApplicationLifetime desktop) {
                DesktopLifetime = desktop;
                if (Platform == OSPlatform.FreeBSD) desktop.Shutdown();

                // Settings up web request callbacks
                VKUITheme.Current.WebRequestCallback = WebRequestCallback;

                int uid = Settings.Get<int>(Settings.VK_USER_ID);
                string token = Settings.Get<string>(Settings.VK_TOKEN);
                if (uid > 0 && !String.IsNullOrEmpty(token)) {
                    VKSession.StartUserSession(uid, token);
                    desktop.MainWindow = VKSession.Main.Window;
                } else {
                    desktop.MainWindow = new Views.SignInWindow();
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async Task<HttpResponseMessage> WebRequestCallback(Uri arg) {
            return await LNet.GetAsync(arg);
        }

        public VKUIScheme CurrentScheme { get; private set; }

        public static void SwitchTheme(VKUIScheme scheme) {
            SimpleTheme simple = (SimpleTheme)_current.Styles[0]!;
            simple.Mode = scheme == VKUIScheme.BrightLight ? SimpleThemeMode.Light : SimpleThemeMode.Dark;

            VKUITheme.Current.Scheme = scheme;
            _current.CurrentScheme = scheme;
        }

        public static void ToggleTheme() {
            SwitchTheme(_current.CurrentScheme == VKUIScheme.BrightLight ? VKUIScheme.SpaceGray : VKUIScheme.BrightLight);
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
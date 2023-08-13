using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ELOR.Laney.Views {
    public sealed partial class SignInWindow : Window {
        public SignInWindow() {
            InitializeComponent();
            Log.Information($"{nameof(SignInWindow)} initialized.");

            Activated += (a, b) => {
                Program.StopStopwatch();
                Log.Information($"{nameof(SignInWindow)} activated. Launch time: {Program.LaunchTime} ms.");
            };

            LangPicker.ItemsSource = Localizer.SupportedLanguages;
            var lid = Settings.Get(Settings.LANGUAGE, Constants.DefaultLang);
            LangPicker.SelectedItem = Localizer.SupportedLanguages.Where(l => l.Item1 == lid).FirstOrDefault();
            LangPicker.SelectionChanged += (a, b) => {
                Tuple<string, string> selected = LangPicker.SelectedItem as Tuple<string, string>;
                if (selected == null) return;
                Settings.Set(Settings.LANGUAGE, selected.Item1);
                Localizer.Instance.LoadLanguage(selected.Item1);
            };

            VersionInfo.Text = $"v. {App.BuildInfo}";
            // VersionInfo.Text += $"\nApp folder: {App.LocalDataPath}";

#if WIN
            (AuthWorkaroundCB.Parent as Panel).Children.Remove(AuthWorkaroundCB);
#endif
        }

        private async void SignIn(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Button button = sender as Button;
            button.IsEnabled = false;

            // var result = await AuthManager.AuthWithTokenAsync(this);
            Tuple<long, string> result = new Tuple<long, string>(0, String.Empty);
            if (ExternalBrowserCB.IsChecked.Value) {
                result = await AuthManager.AuthViaExternalBrowserAsync();
            } else {
#if WIN
                result = await AuthManager.AuthWithOAuthAsync();
#else
                result = await AuthManager.AuthWithOAuthAsync(AuthWorkaroundCB.IsChecked.Value);
#endif
            }

            if (result.Item1 != 0) {
                Settings.SetBatch(new Dictionary<string, object> {
                    { Settings.VK_USER_ID, result.Item1 },
                    { Settings.VK_TOKEN, result.Item2 }
                });
                VKSession.StartUserSession(result.Item1, result.Item2);
                App.Current.DesktopLifetime.MainWindow = VKSession.Main.Window;
                Close();
            }

            button.IsEnabled = true;
        }
    }
}
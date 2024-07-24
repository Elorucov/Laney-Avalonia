using ELOR.Laney.Core.Localization;
using ELOR.Laney.Core;
using System;
using VKUI.Controls;
using System.Linq;
using System.IO;

namespace ELOR.Laney.Views.SignIn {
    public partial class MainPage : Page {
        public MainPage() {
            InitializeComponent();

            LangPicker.ItemsSource = Localizer.SupportedLanguages;
            var lid = Settings.Get(Settings.LANGUAGE, Constants.DefaultLang);
            LangPicker.SelectedItem = Localizer.SupportedLanguages.Where(l => l.Item1 == lid).FirstOrDefault();
            LangPicker.SelectionChanged += (a, b) => {
                Tuple<string, string> selected = LangPicker.SelectedItem as Tuple<string, string>;
                if (selected == null) return;
                Settings.Set(Settings.LANGUAGE, selected.Item1);
                Localizer.Instance.LoadLanguage(selected.Item1);
                NavigationRouter.NavigateToAsync(new MainPage(), NavigationMode.Clear);
            };

#if RELEASE
            VersionInfo.Text = $"v{App.BuildInfo}";
#elif BETA
            VersionInfo.Text = $"v{App.BuildInfo} (BETA)";
            Middle.Children.Add(new Avalonia.Controls.TextBlock { 
                FontSize = 12,
                Margin = new Avalonia.Thickness(0,36,0,0),
                Text = $"Logs folder:\n{Path.Combine(App.LocalDataPath, "logs")}"
            });
#else
            VersionInfo.Text = $"v{App.BuildInfo} (DEV)";
            Middle.Children.Add(new Avalonia.Controls.TextBlock { 
                FontSize = 12,
                Margin = new Avalonia.Thickness(0,36,0,0),
                Text = $"Logs folder:\n{Path.Combine(App.LocalDataPath, "logs")}"
            });
#endif
        }

        private void GoToExternalAuthPage(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            NavigationRouter.NavigateToAsync(new ExternalBrowserAuthPage());
        }

        private void GoToDirectAuthPage(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            NavigationRouter.NavigateToAsync(new QRAuthPage());
        }
    }
}
using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Views.Modals;
using System;
using System.IO;
using System.Linq;
using VKUI.Controls;

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
                Localizer.LoadLanguage(selected.Item1);
                NavigationRouter.NavigateToAsync(new MainPage(), NavigationMode.Clear);
            };

            VersionInfo.Text = $"v{App.BuildInfo}";

#if !RELEASE
            Middle.Children.Add(new TextBlock {
                FontSize = 12,
                Margin = new Avalonia.Thickness(0, 72, 0, 0),
                Text = $"Logs folder:\n{Path.Combine(App.LocalDataPath, "logs")}"
            });
#endif
        }

        private async void GoToDirectAuthPage(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            await NavigationRouter.NavigateToAsync(new QRAuthPage());
        }

        private void Page_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            App.UpdateBranding(Logo.Child as Grid);
        }
    }
}
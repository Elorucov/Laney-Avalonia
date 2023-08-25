using ELOR.Laney.Core.Localization;
using ELOR.Laney.Core;
using System;
using VKUI.Controls;
using System.Linq;

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

            VersionInfo.Text = $"v. {App.BuildInfo}";
        }

        private void GoToExternalAuthPage(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            NavigationRouter.NavigateToAsync(new ExternalBrowserAuthPage());
        }

        private void GoToDirectAuthPage(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            NavigationRouter.NavigateToAsync(new DirectAuthPage());
        }
    }
}
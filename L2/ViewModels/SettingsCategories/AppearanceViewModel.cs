using ELOR.Laney.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ELOR.Laney.ViewModels.SettingsCategories {
    public class AppearanceViewModel : CommonViewModel {
        public ObservableCollection<Tuple<int, string>> AppThemes { get; private set; } = new ObservableCollection<Tuple<int, string>> {
            new Tuple<int, string>(1, "Light"),
            new Tuple<int, string>(2, "Dark")
        };

        public Tuple<int, string> CurrentAppTheme { get { return GetTheme(); } set { ChangeTheme(value); OnPropertyChanged(); } }
        public bool ChatItemMoreRows { get { return Settings.ChatItemMoreRows; } set { Settings.ChatItemMoreRows = value; OnPropertyChanged(); } }

        private Tuple<int, string> GetTheme() {
            return AppThemes.Where(l => l.Item1 == Settings.AppTheme).FirstOrDefault();
        }

        private void ChangeTheme(Tuple<int, string> value) {
            Settings.Set(Settings.THEME, value.Item1);
            App.ChangeTheme(value.Item1);
            OnPropertyChanged(nameof(CurrentAppTheme));
        }
    }
}

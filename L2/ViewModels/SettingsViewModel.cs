using ELOR.Laney.Core.Localization;
using ELOR.Laney.DataModels;
using ELOR.Laney.ViewModels.SettingsCategories;
using ELOR.Laney.Views.SettingsCategories;
using System.Collections.ObjectModel;
using System.Linq;
using VKUI.Controls;

namespace ELOR.Laney.ViewModels {
    public class SettingsViewModel : ViewModelBase {
        private ObservableCollection<SettingsCategory> _categories;
        private SettingsCategory _selectedCategory;

        public ObservableCollection<SettingsCategory> Categories { get { return _categories; } private set { _categories = value; OnPropertyChanged(); } }
        public SettingsCategory SelectedCategory { get { return _selectedCategory; } set { _selectedCategory = value; OnPropertyChanged(); } }

        public SettingsViewModel() {
            Categories = new ObservableCollection<SettingsCategory> { 
                new SettingsCategory(VKIconNames.Icon28SettingsOutline, Localizer.Instance["settings_general"], new General(), new GeneralViewModel()),
                new SettingsCategory(VKIconNames.Icon28PaletteOutline, Localizer.Instance["settings_appearance"], new Appearance(), null),
                new SettingsCategory(VKIconNames.Icon28Notifications, Localizer.Instance["settings_notifications"], new NotificationsPage(), null),
                new SettingsCategory(VKIconNames.Icon28PrivacyOutline, Localizer.Instance["settings_privacy"], new Privacy(), null),
                // new SettingsCategory(VKIconNames.Icon28BlockOutline, Localizer.Instance["settings_blacklist"], new DebugPage(), null),
                new SettingsCategory(VKIconNames.Icon28BugOutline, "Debug", new DebugPage(), null),
            };
            SelectedCategory = Categories.FirstOrDefault();
        }
    }
}
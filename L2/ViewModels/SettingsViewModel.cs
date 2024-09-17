using ELOR.Laney.Core;
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
                new SettingsCategory(VKIconNames.Icon28SettingsOutline, Assets.i18n.Resources.settings_general, new General(), new GeneralViewModel()),
                new SettingsCategory(VKIconNames.Icon28PaletteOutline, Assets.i18n.Resources.settings_appearance, new Appearance(), new AppearanceViewModel()),
                new SettingsCategory(VKIconNames.Icon28Notifications, Assets.i18n.Resources.settings_notifications, new NotificationsPage(), new NotificationsViewModel()),
                new SettingsCategory(VKIconNames.Icon28PrivacyOutline, Assets.i18n.Resources.settings_privacy, new Privacy(), null),
#if RELEASE
#else
                new SettingsCategory(VKIconNames.Icon28BugOutline, "Debug", new DebugPage(), null)
#endif
            };

#if RELEASE
            if (Settings.Get("god", false)) Categories.Add(new SettingsCategory(VKIconNames.Icon28BugOutline, "Debug", new DebugPage(), null));
#endif

            SelectedCategory = Categories.FirstOrDefault();
        }
    }
}
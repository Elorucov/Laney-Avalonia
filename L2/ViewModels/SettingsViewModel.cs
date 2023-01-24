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
                new SettingsCategory(VKIconNames.Icon28SettingsOutline, "General", new General(), new GeneralViewModel()),
                new SettingsCategory(VKIconNames.Icon28SettingsOutline, "Debug", new DebugPage(), null),
            };
            SelectedCategory = Categories.FirstOrDefault();
        }
    }
}
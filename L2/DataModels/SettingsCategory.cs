using Avalonia.Controls;
using ELOR.Laney.ViewModels;

namespace ELOR.Laney.DataModels {
    public class SettingsCategory {
        public string IconId { get; private set; }
        public string Title { get; private set; }
        public UserControl View { get; private set; }
        public CommonViewModel ViewModel { get; private set; }

        public SettingsCategory(string icon, string title, UserControl view, CommonViewModel viewModel) {
            IconId = icon;
            Title = title;
            View = view;
            ViewModel = viewModel;
        }
    }
}
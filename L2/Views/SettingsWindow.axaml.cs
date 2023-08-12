using Avalonia.Controls;
using ELOR.Laney.DataModels;
using ELOR.Laney.ViewModels;
using System.Runtime.InteropServices;
using VKUI.Windows;

namespace ELOR.Laney.Views {
    public partial class SettingsWindow : DialogWindow {
        SettingsViewModel ViewModel { get => DataContext as SettingsViewModel; }

        public SettingsWindow() {
            InitializeComponent();
#if LINUX
            TitleBar.IsVisible = false;
#endif

            DataContext = new SettingsViewModel();
        }

        private void CategoriesList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SettingsCategory sc = CategoriesList.SelectedItem as SettingsCategory;
            if (sc == null) {
                ViewModel.SelectedCategory = e.RemovedItems[0] as SettingsCategory;
                return;
            }
            ContentPanel.Content = sc.View;
            ContentPanel.DataContext = sc.ViewModel;
        }
    }
}
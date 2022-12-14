using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.ViewModels.Controls;

namespace ELOR.Laney.Controls {
    public partial class Composer : UserControl {
        private ComposerViewModel ViewModel { get { return DataContext as ComposerViewModel; } }

        public Composer() {
            InitializeComponent();
        }

        private void BotKbdButton_Click(object sender, RoutedEventArgs e) {
            BotKeyboardToggle.IsChecked = !BotKeyboardToggle.IsChecked;
        }

        private void ShowAttachmentPickerContextMenu(object sender, RoutedEventArgs e) {
            ViewModel.ShowAttachmentPickerContextMenu(sender as Button);
        }
    }
}
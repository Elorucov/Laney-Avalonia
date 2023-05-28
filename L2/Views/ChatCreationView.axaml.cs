using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.ViewModels;
using ELOR.VKAPILib.Objects;
using VKUI.Controls;

namespace ELOR.Laney.Views {
    public partial class ChatCreationView : Page {
        private ChatCreationViewModel ViewModel { get { return DataContext as ChatCreationViewModel; } }

        public ChatCreationView() {
            InitializeComponent();
            BackButton.Click += (a, b) => NavigationRouter.BackAsync();
        }

        private void ChatCreationView_Loaded(object sender, RoutedEventArgs e) {
            DataContext = new ChatCreationViewModel(VKSession.GetByDataContext(this), () => NavigationRouter.BackAsync());
        }

        private void OnFriendRemoveButtonClick(object sender, RoutedEventArgs e) {
            User friend = (sender as Control).DataContext as User;
            ViewModel.RemoveFriendFromSelected(friend);
        }
    }
}
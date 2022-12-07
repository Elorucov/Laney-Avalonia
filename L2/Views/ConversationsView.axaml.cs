using Avalonia.Controls;
using Avalonia.Input;
using ELOR.Laney.Core;
using ELOR.Laney.ViewModels;

namespace ELOR.Laney.Views {
    public sealed partial class ConversationsView : UserControl {
        private VKSession Session { get { return VKSession.GetByDataContext(this); } }

        public ConversationsView() {
            InitializeComponent();
            AvatarButton.Click += (a, b) => {
                Session.ShowSessionPopup(AvatarButton);
            };
            NewConvButton.Click += (a, b) => {
                App.ToggleTheme();
            };
            SearchButton.Click += (a, b) => {
                throw new System.Exception("This is a crash. Not bandicoot, but a crash.");
            };
        }

        private void ListBoxItemTapped(object sender, TappedEventArgs args) {
            ListBox listBox = sender as ListBox;
            ChatViewModel cvm = listBox.SelectedItem as ChatViewModel;
            if (cvm == null) return;
            Session.GetToChat(cvm.PeerId);
        }
    }
}
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels;
using VKUI.Controls;
using VKUI.Popups;

namespace ELOR.Laney.Views {
    public sealed partial class ConversationsView : UserControl {
        private VKSession Session { get { return VKSession.GetByDataContext(this); } }

        public ConversationsView() {
            InitializeComponent();
            NewConvButton.Click += (a, b) => {
                App.ToggleTheme();
            };
            SearchButton.Click += (a, b) => {
                throw new System.Exception("This is a crash. Not bandicoot, but a crash.");
            };
        }

        private void ListBoxItemTapped(object sender, RoutedEventArgs args) {
            ListBox listBox = sender as ListBox;
            ChatViewModel cvm = listBox.SelectedItem as ChatViewModel;
            if (cvm == null) return;
            Session.GetToChat(cvm.PeerId);
        }
    }
}
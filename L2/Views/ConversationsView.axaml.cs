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
        public ConversationsView() {
            InitializeComponent();
            NewConvButton.Click += (a, b) => {
                App.ToggleTheme();
            };
            SearchButton.Click += (a, b) => {
                object x = "a";
                int i = 1 + (int)x;
            };
        }

        private void ListBoxItemTapped(object sender, RoutedEventArgs args) {
            ListBox listBox = sender as ListBox;
            ChatViewModel cvm = listBox.SelectedItem as ChatViewModel;
            if (cvm == null) return;
            VKSession session = VKSession.GetByDataContext(this);
            session.GetToChat(cvm.PeerId);
            session.Window.SwitchToSide(true);
        }
    }
}
using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels;
using System;

namespace ELOR.Laney.Views {
    public sealed partial class ImView : UserControl {
        private VKSession Session { get { return VKSession.GetByDataContext(this); } }

        public ImView() {
            InitializeComponent();
            AvatarButton.Click += (a, b) => {
                Session.ShowSessionPopup(AvatarButton);
            };
            NewConvButton.Click += (a, b) => {
                App.ToggleTheme();
            };
            SearchButton.Click += (a, b) => {
                throw new Exception("This is a crash. Not bandicoot, but a crash.");
            };
            chatsListScroll.RegisterIncrementalLoadingEvent(() => Session.ImViewModel.LoadConversations());
        }

        private void OnChatClick(object? sender, RoutedEventArgs args) {
            Button b = sender as Button;
            ChatViewModel cvm = b.Content as ChatViewModel;
            if (cvm == null) return;
            Session.GetToChat(cvm.PeerId);
        }
    }
}
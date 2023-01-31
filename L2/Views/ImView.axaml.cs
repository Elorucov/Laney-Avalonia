using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels;
using System;
using System.ComponentModel;

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
                // throw new Exception("This is a crash. Not bandicoot, but a crash.");
                //Window w = Session.Window;
                //w.WindowState = WindowState.Normal;
                //w.PlatformImpl.Resize(new Size(1134, 756));
                ExceptionHelper.ShowNotImplementedDialogAsync(Session.Window);
            };

            DataContextChanged += ImView_DataContextChanged;
            ChatsList.Loaded += ChatsList_Loaded;
        }

        private void ImView_DataContextChanged(object sender, EventArgs e) {
            DataContextChanged -= ImView_DataContextChanged;
            Session.PropertyChanged += Session_PropertyChanged;
        }

        private void Session_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(VKSession.CurrentOpenedChat)) {
                ChatsList.SelectedItem = Session.CurrentOpenedChat;
            }
        }

        private void ChatsList_Loaded(object sender, RoutedEventArgs e) {
            ChatsList.Loaded -= ChatsList_Loaded;
            ChatsList.SelectionChanged += ChatsList_SelectionChanged;
            new ItemsPresenterWidthFixer(ChatsList);
            new ListBoxAutoScrollHelper(ChatsList);
            (ChatsList.Scroll as ScrollViewer).RegisterIncrementalLoadingEvent(() => Session.ImViewModel.LoadConversations());
        }

        private void ChatsList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count == 0 && e.RemovedItems.Count > 0) {
                ChatsList.SelectedItem = Session.CurrentOpenedChat;
                return;
            }
            ChatViewModel cvm = e.AddedItems[0] as ChatViewModel;
            if (cvm == null) return;
            Session.GetToChat(cvm.PeerId);
        }
    }
}
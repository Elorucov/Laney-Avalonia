using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels.Modals;
using System;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals {
    public partial class PeerProfile : DialogWindow {
        PeerProfileViewModel ViewModel { get => DataContext as PeerProfileViewModel; }

        public PeerProfile() { }

        public PeerProfile(VKSession session, long peerId) {
            InitializeComponent();

            DataContext = new PeerProfileViewModel(session, peerId);
            ViewModel.CloseWindowRequested += ViewModel_CloseWindowRequested;

            if (peerId.IsChat()) {
                Tabs.Items.Remove(UserInfoTab);
            } else {
                Tabs.Items.Remove(ChatMembersTab);
            }

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            // RelativeSource is not working when CompiledBindings=true!
            FirstButton.CommandParameter = FirstButton;
            SecondButton.CommandParameter = SecondButton;
            ThirdButton.CommandParameter = ThirdButton;
            MoreButton.CommandParameter = MoreButton;
#if LINUX
            TitleBar.IsVisible = false;
#endif
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(PeerProfileViewModel.IsLoading) && !ViewModel.IsLoading) {
                if (ViewModel.Placeholder == null) ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

                // Remove members tab for channels.
                if (ViewModel.Id.IsChat() && ViewModel.DisplayedMembers.Count == 0) Tabs.Items.Remove(ChatMembersTab);
            }
        }

        private void MoreButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            //if (MoreButton.CommandParameter != null)
        }

        private void ViewModel_CloseWindowRequested(object sender, EventArgs e) {
            (sender as PeerProfileViewModel).CloseWindowRequested -= ViewModel_CloseWindowRequested;
            Close();
        }
    }
}
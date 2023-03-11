using Avalonia.Markup.Xaml.Templates;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.ViewModels.Modals;
using System;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals {
    public partial class PeerProfile : DialogWindow {
        PeerProfileViewModel ViewModel { get => DataContext as PeerProfileViewModel; }

        public PeerProfile() { }

        public PeerProfile(VKSession session, int peerId) {
            InitializeComponent();

            if (peerId > 2000000000) {
                FirstTab.Header = Localizer.Instance["members"];
                FirstTabContent.ContentTemplate = (DataTemplate)Resources["ChatMembersContentTemplate"];
            }

            DataContext = new PeerProfileViewModel(session, peerId);
            ViewModel.CloseWindowRequested += ViewModel_CloseWindowRequested;

#if LINUX
            TitleBar.IsVisible = false;
#endif
        }

        private void ViewModel_CloseWindowRequested(object sender, EventArgs e) {
            (sender as PeerProfileViewModel).CloseWindowRequested -= ViewModel_CloseWindowRequested;
            Close();
        }
    }
}
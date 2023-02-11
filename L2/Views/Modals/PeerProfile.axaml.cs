using Avalonia.Markup.Xaml.Templates;
using ELOR.Laney.Core;
using ELOR.Laney.ViewModels.Modals;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals {
    public partial class PeerProfile : DialogWindow {
        PeerProfileViewModel ViewModel { get => DataContext as PeerProfileViewModel; }

        public PeerProfile() { }

        public PeerProfile(VKSession session, int peerId) {
            InitializeComponent();

            if (peerId > 2000000000) FirstTabContent.ContentTemplate = (DataTemplate)Resources["ChatMembersContentTemplate"];

            DataContext = new PeerProfileViewModel(session, peerId);
#if MAC
            TitleBar.CanShowTitle = true;
#elif LINUX
            TitleBar.IsVisible = false;
#endif
        }
    }
}

#define WIN
#define LINUX
#define MAC

using VKUI.Windows;

namespace ELOR.Laney.Views.Modals {
    public partial class PeerProfile : DialogWindow {
        public PeerProfile() {
            InitializeComponent();

#if LINUX
            TitleBar.IsVisible = false;
#elif MAC
            TitleBar.CanShowTitle = true;
#endif

        }
    }
}

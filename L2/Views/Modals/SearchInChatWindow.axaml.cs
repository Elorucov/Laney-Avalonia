using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels.Controls;
using ELOR.Laney.ViewModels.Modals;
using ELOR.VKAPILib.Objects;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals {
    public partial class SearchInChatWindow : DialogWindow {
        private VKSession session;
        private long peerId;
        private SearchInChatViewModel ViewModel { get { return Root.DataContext as SearchInChatViewModel; } }

        public SearchInChatWindow() {
            InitializeComponent();
        }

        public SearchInChatWindow(VKSession session, long peerId, Window owner) {
            InitializeComponent();
            this.session = session;
            this.peerId = peerId;
            UpdateWindowTitle();

#if LINUX
            TitleBar.IsVisible = false;
#endif

            DataContext = session;
            Root.DataContext = new SearchInChatViewModel(session, peerId);
            session.CurrentOpenedChatChanged += Session_CurrentOpenedChatChanged;

            // TODO: set transparency for system window itself, not for a content on window!
            Activated += (a, b) => Opacity = 1;
            Deactivated += (a, b) => Opacity = 0.4;

            owner.Deactivated += Owner_Deactivated;
            owner.Closing += Owner_Closing;
        }

        private void Owner_Deactivated(object sender, System.EventArgs e) {
            Window owner = sender as Window;
            if (owner.WindowState == WindowState.Minimized) {
                owner.Deactivated -= Owner_Deactivated;
                Close();
            }
        }

        private void Owner_Closing(object sender, System.EventArgs e) {
            Window owner = sender as Window;
            owner.Closing -= Owner_Closing;
            Close();
        }

        private void UpdateWindowTitle() {
            if (peerId.IsUser()) {
                User u = CacheManager.GetUser(peerId);
                if (u == null) return;
                Title = Localizer.Instance.GetFormatted("msg_search_user", u.FirstNameIns, u.LastNameIns);
            } else if (peerId.IsGroup()) {
                Group g = CacheManager.GetGroup(peerId);
                if (g == null) return;
                Title = Localizer.Instance.GetFormatted("msg_search_group", g.Name);
            } else if (peerId.IsChat()) {
                var c = CacheManager.GetChat(session.Id, peerId);
                if (c == null) return;
                Title = Localizer.Instance.GetFormatted("msg_search_chat", c.Title);
            }
        }

        private void Session_CurrentOpenedChatChanged(object sender, long e) {
            if (peerId != e) {
                session.CurrentOpenedChatChanged -= Session_CurrentOpenedChatChanged;
                Close();
            }
        }

        private void SearchInChatWindow_Loaded(object sender, RoutedEventArgs e) {
            MessagesSV.RegisterIncrementalLoadingEvent(() => ViewModel.DoSearch());
            SearchBox.Focus();
        }

        private void OnSearchBoxKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) ViewModel.DoSearch(true);
        }

        private void OnMessageSelected(object sender, RoutedEventArgs e) {
            MessageViewModel item = (sender as Control).DataContext as MessageViewModel;
            session.GoToChat(item.PeerId, item.ConversationMessageId);
        }
    }
}
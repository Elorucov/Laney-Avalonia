using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.DataModels;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels;
using VKUI.Controls;

namespace ELOR.Laney.Views {
    public partial class SearchView : Page {
        private SearchViewModel ViewModel { get { return DataContext as SearchViewModel; } }

        public SearchView() {
            InitializeComponent();
            BackButton.Click += async (a, b) => await NavigationRouter.BackAsync();
        }

        private void SearchView_Loaded(object sender, RoutedEventArgs e) {
            DataContext = new SearchViewModel(VKSession.GetByDataContext(this));
            MessagesSV.RegisterIncrementalLoadingEvent(async () => await ViewModel.SearchMessagesAsync());
        }

        private void OnSearchBoxKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) new System.Action(async () => await ViewModel.DoSearchAsync())();
        }

        private void OnChatSelected(object sender, RoutedEventArgs e) {
            Entity item = (sender as Control).DataContext as Entity;
            VKSession.GetByDataContext(this).GoToChat(item.Id);
        }

        private void OnMessageSelected(object sender, RoutedEventArgs e) {
            FoundMessageItem item = (sender as Control).DataContext as FoundMessageItem;
            VKSession.GetByDataContext(this).GoToChat(item.PeerId, item.Id);
        }
    }
}
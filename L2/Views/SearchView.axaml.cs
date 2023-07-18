using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.DataModels;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels;
using System;
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
            MessagesSV.RegisterIncrementalLoadingEvent(ViewModel.SearchMessages);
        }

        private void OnSearchBoxKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) ViewModel.DoSearch();
        }

        private void OnChatSelected(object sender, RoutedEventArgs e) {
            Tuple<int, Uri, string> item = (sender as Control).DataContext as Tuple<int, Uri, string>;
            VKSession.GetByDataContext(this).GetToChat(item.Item1);
        }

        private void OnMessageSelected(object sender, RoutedEventArgs e) {
            FoundMessageItem item = (sender as Control).DataContext as FoundMessageItem;
            VKSession.GetByDataContext(this).GetToChat(item.PeerId, item.Id);
        }
    }
}
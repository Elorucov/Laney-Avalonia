using Avalonia.Controls;
using Avalonia.Input;
using ELOR.Laney.Core;
using ELOR.Laney.DataModels;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels.Modals;
using System;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals {
    public partial class SharingView : DialogWindow {
        SharingViewModel user;
        SharingViewModel group;
        SharingViewModel CurrentViewModel { get { return DataContext as SharingViewModel; } }

        public SharingView() {
            InitializeComponent();
        }

        public SharingView(SharingViewModel user, SharingViewModel group) {
            InitializeComponent();
            this.user = user;
            this.group = group;

            switch (user.Type) {
                case SharingContentType.Messages:
                    Title = Assets.i18n.Resources.sharing_messages;
                    break;
            }

            DataContextChanged += (a, b) => {
                CurrentViewModel?.OnDisplayed();
            };

            var handler = new ListBoxItemClickHandler<Entity>(ChatsList, ItemClicked);

            if (group != null) {
                SessionSwitcherContainer.IsVisible = true;
                this.FixDialogWindows(TitleBar, Content);
                DataContext = group;
            } else {
                Grid.SetRow(Content, 1);
                TitleBar.CanShowTitle = true;
#if LINUX
            TitleBar.IsVisible = false;
#endif
                DataContext = user;
            }
        }

        private void ItemClicked(Entity chat) {
            Close(new Tuple<VKSession, long, long>(CurrentViewModel.Session, chat.Id, CurrentViewModel.GroupId));
        }

        private void OnSearchBoxKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) new Action(async () => await CurrentViewModel.SearchChatsAsync())();
        }

        private void SessionSwitcher_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            switch (SessionSwitcher?.SelectedIndex) {
                case 0:
                    DataContext = group;
                    break;
                case 1:
                    DataContext = user;
                    break;
            }
        }
    }
}
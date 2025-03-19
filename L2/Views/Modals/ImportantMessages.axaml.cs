using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels.Controls;
using ELOR.Laney.ViewModels.Modals;
using System;
using System.Collections.Generic;
using VKUI.Controls;
using VKUI.Popups;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals {
    public partial class ImportantMessages : DialogWindow {
        public ImportantMessages() {
            InitializeComponent();
        }

        private ImportantMessagesViewModel ViewModel { get { return DataContext as ImportantMessagesViewModel; } }
        private VKSession session;

        public ImportantMessages(VKSession session) {
            InitializeComponent();
            this.session = session;
            Tag = session;
            DataContext = new ImportantMessagesViewModel(session);

#if LINUX
            TitleBar.IsVisible = false;
#endif

            Loaded += async (a, b) => {
                var il = new IncrementalLoader(MessagesListSV, async () => await ViewModel.LoadAsync());
                await ViewModel.LoadAsync(0);
            };
        }


        private void OnMessageSelected(object sender, RoutedEventArgs e) {
            MessageViewModel item = (sender as Control).DataContext as MessageViewModel;
            Close(item);
        }

        private void GoToOffset(object sender, RoutedEventArgs e) {
            int offset = 0;
            if (Int32.TryParse(MessagesOffset.Text, out offset)) {
                new System.Action(async () => {
                    ViewModel.Messages.Clear();
                    await ViewModel.LoadAsync(offset);
                })();
            }
        }

        private void MessageContextRequested(object sender, ContextRequestedEventArgs e) {
            Control p = sender as Control;
            MessageViewModel message = p?.DataContext as MessageViewModel;
            if (message == null) return;

            ActionSheet ash = new ActionSheet();

            ActionSheetItem debug = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20BugOutline },
                Header = $"ID: {message.Id}, CMID: {message.ConversationMessageId}"
            };
            ActionSheetItem go = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20MessageArrowRightOutline },
                Header = Assets.i18n.Resources.go_to_message,
            };
            ActionSheetItem unmark = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20UnfavoriteOutline },
                Header = Assets.i18n.Resources.unmark_important,
            };

            go.Click += (a, b) => {
                Close(new Tuple<long, int>(message.PeerId, message.ConversationMessageId));
            };

            unmark.Click += async (a, b) => {
                try {
                    var response = await session.API.Messages.MarkAsImportantAsync(message.PeerId, new List<int> { message.Id }, false);
                    ViewModel.RemoveMessageFromLoaded(message);
                } catch (Exception ex) {
                    await ExceptionHelper.ShowErrorDialogAsync(this, ex, true);
                }
            };

            if (Settings.ShowDevItemsInContextMenus) ash.Items.Add(debug);
            ash.Items.Add(go);
            if (message.IsImportant) ash.Items.Add(unmark);


            if (ash.Items.Count > 0) ash.ShowAt(p, true);
        }
    }
}
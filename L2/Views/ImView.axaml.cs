using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels;
using Serilog;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using VKUI.Controls;

namespace ELOR.Laney.Views {
    public sealed partial class ImView : Page {
        private VKSession Session { get { return VKSession.GetByDataContext(this); } }

        public ImView() {
            InitializeComponent();
            AvatarButton.Click += (a, b) => {
                Session.ShowSessionPopup(AvatarButton);
            };
            NewConvButton.Click += (a, b) => {
                NavigationRouter.NavigateToAsync(new ChatCreationView());
            };
            SearchButton.Click += (a, b) => {
                NavigationRouter.NavigateToAsync(new SearchView());
            };

            ChatsList.Loaded += ChatsList_Loaded;

            ChatsList.ItemTemplate = App.GetResource<DataTemplate>(Settings.ChatItemMoreRows ? "ChatItemTemplate3Row" : "ChatItemTemplate2Row");
            Settings.SettingChanged += Settings_SettingChanged;
        }

        private void Settings_SettingChanged(string key, object value) {
            switch (key) {
                case Settings.CHAT_ITEM_MORE_ROWS:
                    DataTemplate template = App.GetResource<DataTemplate>((bool)value ? "ChatItemTemplate3Row" : "ChatItemTemplate2Row");
                    ChatsList.ItemTemplate = template;

                    // Костыль для того, чтобы шаблон действительно сменился.
                    ChatsList.ItemsSource = null;
                    var prop = ChatsList.GetObservable(ListBox.DataContextProperty)
                        .OfType<VKSession>()
                        .Select(v => v.ImViewModel.SortedChats);
                    ChatsList.Bind(ListBox.ItemsSourceProperty, prop);
                    break;
            }
        }

        private void ChatsList_Loaded(object sender, RoutedEventArgs e) {
            ChatsList.Loaded -= ChatsList_Loaded;
            ChatsList.SelectionChanged += ChatsList_SelectionChanged;
            new ItemsPresenterWidthFixer(ChatsList);
            new ListBoxAutoScrollHelper(ChatsList);
            TryRegisterIncrementalLoadingEvent();
        }

        private async void TryRegisterIncrementalLoadingEvent() {
            await Task.Delay(1000);
            try {
                (ChatsList.Scroll as ScrollViewer)?.RegisterIncrementalLoadingEvent(Session.ImViewModel.LoadConversations);
            } catch (Exception ex) {
                Log.Error(ex, $"A problem has occured while registering incremental loading event for ChatsList!");
                TryRegisterIncrementalLoadingEvent();
            }
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
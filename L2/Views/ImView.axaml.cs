using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace ELOR.Laney.Views {
    public sealed partial class ImView : UserControl {
        private VKSession Session { get { return VKSession.GetByDataContext(this); } }

        public ImView() {
            InitializeComponent();
            AvatarButton.Click += (a, b) => {
                Session.ShowSessionPopup(AvatarButton);
            };
            NewConvButton.Click += (a, b) => {
                ExceptionHelper.ShowNotImplementedDialogAsync(Session.Window);
            };
            SearchButton.Click += (a, b) => {
                // throw new Exception("This is a crash. Not bandicoot, but a crash.");
                //Window w = Session.Window;
                //w.WindowState = WindowState.Normal;
                //w.PlatformImpl.Resize(new Size(1134, 756));
                ExceptionHelper.ShowNotImplementedDialogAsync(Session.Window);
            };

            DataContextChanged += ImView_DataContextChanged;
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
                    ChatsList.Bind(ListBox.ItemsProperty, prop);
                    break;
            }
        }

        private void ImView_DataContextChanged(object sender, EventArgs e) {
            DataContextChanged -= ImView_DataContextChanged;
        }

        private void ChatsList_Loaded(object sender, RoutedEventArgs e) {
            ChatsList.Loaded -= ChatsList_Loaded;
            ChatsList.SelectionChanged += ChatsList_SelectionChanged;
            new ItemsPresenterWidthFixer(ChatsList);
            new ListBoxAutoScrollHelper(ChatsList);
            (ChatsList.Scroll as ScrollViewer).RegisterIncrementalLoadingEvent(() => Session.ImViewModel.LoadConversations());
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
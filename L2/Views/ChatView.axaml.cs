using Avalonia;
using Avalonia.Controls;
using System;
using ELOR.Laney.ViewModels;
using System.Threading.Tasks;
using ELOR.Laney.Controls;
using ELOR.Laney.ViewModels.Controls;
using Serilog;

namespace ELOR.Laney.Views {
    public sealed partial class ChatView : UserControl, IMainWindowRightView {
        ChatViewModel Chat { get; set; }

        public ChatView() {
            InitializeComponent();
            BackButton.Click += (a, b) => BackButtonClick?.Invoke(this, null);
            DataContextChanged += ChatView_DataContextChanged;
            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        }

        public event EventHandler BackButtonClick;
        public void ChangeBackButtonVisibility(bool isVisible) {
            BackButton.IsVisible = isVisible;
        }

        private void ChatView_DataContextChanged(object sender, EventArgs e) {
            if (Chat != null) {
                Chat.ScrollToMessageRequested -= ScrollToMessage;
                Chat.MessagesChunkLoaded -= TrySaveScroll;
            }

            Chat = DataContext as ChatViewModel;
            Chat.ScrollToMessageRequested += ScrollToMessage;
            Chat.MessagesChunkLoaded += TrySaveScroll;
        }

        double oldScrollViewerHeight = 0;
        bool needToSaveScroll = false;
        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            double trigger = 40;
            double h = scrollViewer.Extent.Height - scrollViewer.DesiredSize.Height;
            double y = scrollViewer.Offset.Y;
            dbgScrollInfo.Text = $"{Math.Round(y)}/{h}";

            if (needToSaveScroll && oldScrollViewerHeight != h) {
                double diff = h - oldScrollViewerHeight;
                double newpos = y + diff;

                scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
                try {
                    if (scrollViewer.CheckAccess()) {
                        while (scrollViewer.Offset.Y != newpos) {
                            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, newpos);
                            await Task.Delay(32).ConfigureAwait(false);
                        }
                    }
                } catch (Exception ex) {
                    Log.Error(ex, "Unable to save scroll position after old messages are loaded!");
                }
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;

                needToSaveScroll = false;
                oldScrollViewerHeight = h;
                return;
            }

            oldScrollViewerHeight = h;
            if (y < trigger) {
                Chat.LoadPreviousMessages();
            } else if (y > h - trigger) {
                Chat.LoadNextMessages();
            }
        }

        private void TrySaveScroll(object sender, bool e) {
            if (e) return; // нужно "сохранить" скролл при прогрузке предыдущих сообщений.
            needToSaveScroll = true;
        }

        private void ScrollToMessage(object sender, int messageId) {
            itemsPresenter.ScrollIntoView(Chat.DisplayedMessages.IndexOf(Chat.DisplayedMessages.GetById(messageId)));
        }
    }
}
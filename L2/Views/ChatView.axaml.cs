using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using VKUI.Controls;
using System;
using ELOR.Laney.ViewModels;
using ELOR.Laney.Core;
using Serilog;
using Avalonia.VisualTree;
using System.Threading.Tasks;
using System.Linq;

namespace ELOR.Laney.Views {
    public sealed partial class ChatView : UserControl, IMainWindowRightView {
        ChatViewModel Chat { get { return DataContext as ChatViewModel; } }

        public ChatView() {
            InitializeComponent();
            BackButton.Click += (a, b) => BackButtonClick?.Invoke(this, null);
            DataContextChanged += ChatView_DataContextChanged;
            // scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        }

        public event EventHandler BackButtonClick;
        public void ChangeBackButtonVisibility(bool isVisible) {
            BackButton.IsVisible = isVisible;
        }

        private void ChatView_DataContextChanged(object sender, EventArgs e) {
            Chat.ScrollToMessageCallback = ScrollToMessage;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            //double trigger = 40;
            //double h = scrollViewer.Extent.Height;
            //double y = scrollViewer.Offset.Y;
            //if (y < trigger) {
            //    Chat.LoadPreviousMessages();
            //} else if (y > h - trigger) {
            //    Chat.LoadNextMessages();
            //}
        }

        private async void ScrollToMessage(int messageId) {
            //int indexInGroup = -1;
            //var group = Chat.DisplayedMessages.GroupedMessages.GetGroupThatHasContainsMessage(messageId, out indexInGroup);

            //if (group == null) {
            //    Log.Warning($"Cannot scroll to message {messageId}: group that contains this message is not found!");
            //    return;
            //}

            //int groupIndex = Chat.DisplayedMessages.GroupedMessages.IndexOf(group);
            //var element = itemsRepeater.GetOrCreateElement(groupIndex);

            //// чекайте xaml datatemplate, это stackpanel
            //// первый элемент внутри — дата, а второй — ListBox с сообщениями (с ItemsRepeater есть проблемы!)
            //if (element is StackPanel sp) {
            //    ListBox lb = sp.Children[1] as ListBox;
            //    lb.ScrollIntoView(indexInGroup);
            //} else {
            //    Log.Warning($"Cannot scroll to message {messageId}: group UI is wrong type!");
            //}
        }
    }
}
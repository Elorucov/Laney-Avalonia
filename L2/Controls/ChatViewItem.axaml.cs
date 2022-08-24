using Avalonia;
using Avalonia.Controls.Primitives;
using ELOR.Laney.ViewModels.Controls;

namespace ELOR.Laney.Controls {
    public class ChatViewItem : TemplatedControl {
        #region Properties

        public static readonly StyledProperty<MessageViewModel> MessageProperty =
            AvaloniaProperty.Register<ChatViewItem, MessageViewModel>(nameof(Message));

        public MessageViewModel Message {
            get => GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        #endregion

        #region Template elements

        #endregion
    }
}
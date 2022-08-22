using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using ELOR.Laney.Core;
using ELOR.Laney.ViewModels.Controls;
using System;
using System.ComponentModel;
using VKUI.Controls;

namespace ELOR.Laney.Controls {
    public class MessageBubble : TemplatedControl {

        #region Properties

        public static readonly StyledProperty<MessageViewModel> MessageProperty =
            AvaloniaProperty.Register<MessageBubble, MessageViewModel>(nameof(Message));

        public MessageViewModel Message {
            get => GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        #endregion

        #region Internal

        bool IsOutgoing => Message.SenderId == VKSession.GetByDataContext(this).Id;
        bool IsChat => Message.PeerId > 2000000000;

        #endregion

        #region Constants

        const string BACKGROUND_INCOMING = "IncomingMessageBackground";
        const string BACKGROUND_OUTGOING = "OutgoingMessageBackground";

        #endregion

        #region Template elements

        Grid BubbleRoot;
        Border BubbleBackground;
        Avatar SenderAvatar;
        TextBlock SenderName;
        TextBlock MessageText;

        #endregion

        bool isUILoaded = false;
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            BubbleRoot = e.NameScope.Find<Grid>(nameof(BubbleRoot));
            BubbleBackground = e.NameScope.Find<Border>(nameof(BubbleBackground));
            SenderAvatar = e.NameScope.Find<Avatar>(nameof(SenderAvatar));
            SenderName = e.NameScope.Find<TextBlock>(nameof(SenderName));
            MessageText = e.NameScope.Find<TextBlock>(nameof(MessageText));
            isUILoaded = true;
            RenderElement();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);

            if (change.Property == MessageProperty) {
                if (change.OldValue is MessageViewModel oldm) {
                    oldm.PropertyChanged -= Message_PropertyChanged;
                }
                if (Message == null) {
                    IsVisible = false;
                    return;
                }
                IsVisible = true;
                Message.PropertyChanged += Message_PropertyChanged;
                RenderElement();
            }
        }

        private void Message_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            ChangeUI();
        }

        private void RenderElement() {
            if (!isUILoaded) return;

            // Outgoing
            BubbleRoot.HorizontalAlignment = IsOutgoing ? HorizontalAlignment.Right : HorizontalAlignment.Left;

            var bbc = BubbleBackground.Classes;
            if (bbc.Count > 0)bbc.Remove(IsOutgoing ? BACKGROUND_INCOMING : BACKGROUND_OUTGOING);
            bbc.Add(IsOutgoing ? BACKGROUND_OUTGOING : BACKGROUND_INCOMING);

            // Avatar
            SenderAvatar.IsVisible = IsChat && !IsOutgoing;

            ChangeUI();
        }

        // Смена некоторых частей UI сообщения, которые не влияют
        // в целом на само облачко.
        // Конечно, можно и через TemplateBinding такие вещи делать,
        // но code-behind лучше.
        private void ChangeUI() {
            // Avatar visibility
            SenderAvatar.Opacity = Message.IsSenderAvatarVisible ? 1 : 0;

            var mtm = MessageText.Margin;
            MessageText.Margin = new Thickness(mtm.Left, Message.IsSenderNameVisible ? 0 : 9, mtm.Right, mtm.Bottom);
        }
    }
}

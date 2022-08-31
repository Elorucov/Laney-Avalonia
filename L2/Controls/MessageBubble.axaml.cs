using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using ELOR.Laney.Controls.Attachments;
using ELOR.Laney.Core;
using ELOR.Laney.ViewModels.Controls;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
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
        const string BACKGROUND_STICKER = "StickerMessageBackground";

        #endregion

        #region Template elements

        Grid BubbleRoot;
        Border BubbleBackground;
        Avatar SenderAvatar;
        Border SenderNameWrap;
        TextBlock SenderName;
        Button ReplyMessageButton;
        TextBlock MessageText;
        StackPanel MessageAttachments;

        bool isUILoaded = false;
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            BubbleRoot = e.NameScope.Find<Grid>(nameof(BubbleRoot));
            BubbleBackground = e.NameScope.Find<Border>(nameof(BubbleBackground));
            SenderAvatar = e.NameScope.Find<Avatar>(nameof(SenderAvatar));
            SenderNameWrap = e.NameScope.Find<Border>(nameof(SenderNameWrap));
            SenderName = e.NameScope.Find<TextBlock>(nameof(SenderName));
            ReplyMessageButton = e.NameScope.Find<Button>(nameof(ReplyMessageButton));
            MessageText = e.NameScope.Find<TextBlock>(nameof(MessageText));
            MessageAttachments = e.NameScope.Find<StackPanel>(nameof(MessageAttachments));
            isUILoaded = true;
            RenderElement();
        }

        #endregion

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

            // Checking message content
            bool isSticker = false;
            bool isGraffiti = false;
            bool isImage = false;

            // 0 — по умолчанию, 1 — рамка, 2 — прозрачное
            byte backgroundType = 0;

            if (Message.Attachments.Count == 1) {
                switch (Message.Attachments[0].Type) {
                    case AttachmentType.Sticker:
                        isSticker = true;
                        break;
                    case AttachmentType.Graffiti:
                        isGraffiti = true;
                        break;
                    case AttachmentType.Photo:
                    case AttachmentType.Video:
                        isImage = true;
                        break;
                    case AttachmentType.Document:
                        isImage = Message.Attachments[0].Document?.Preview != null;
                        break;
                }
            }

            if (String.IsNullOrEmpty(Message.Text) && Message.ReplyMessage == null &&
                Message.ForwardedMessages.Count == 0 && Message.Location == null &&
                Message.Keyboard == null && (isSticker || isGraffiti || isImage)) {
                backgroundType = 2;
            } else if (String.IsNullOrEmpty(Message.Text) && Message.ForwardedMessages.Count == 0 &&
                Message.Location == null && Message.Keyboard == null && (isSticker || isGraffiti)) {
                backgroundType = 1;
            }

            // Outgoing
            BubbleRoot.HorizontalAlignment = IsOutgoing ? HorizontalAlignment.Right : HorizontalAlignment.Left;

            // Bubble background
            var bbc = BubbleBackground.Classes;
            bbc.Clear();
            if (backgroundType == 0) {
                bbc.Add(IsOutgoing ? BACKGROUND_OUTGOING : BACKGROUND_INCOMING);
            } else if (backgroundType == 1) {
                bbc.Add(BACKGROUND_STICKER);
            }

            // Avatar
            SenderAvatar.IsVisible = IsChat && !IsOutgoing;

            // Sender name
            SenderNameWrap.IsVisible = backgroundType != 2;

            // Message bubble width
            if (isSticker) {
                BubbleRoot.Width = 168;
            } else if (Message.Attachments.Count > 1 || Message.ForwardedMessages.Count > 1 ||
                Message.Location != null || Message.Keyboard != null) {
                BubbleRoot.Width = 320;
            } else {
                BubbleRoot.Width = Double.NaN;
            }

            // Attachments
            AddAttachments(Message.Attachments);

            ChangeUI();
        }

        // Смена некоторых частей UI сообщения, которые не влияют
        // в целом на само облачко.
        // Конечно, можно и через TemplateBinding такие вещи делать,
        // но code-behind лучше.
        private void ChangeUI() {
            // Avatar visibility
            SenderAvatar.Opacity = Message.IsSenderAvatarVisible ? 1 : 0;

            // Reply msg button margin-top
            double replyTopMargin = Message.IsSenderNameVisible ? 6 : 10;
            var rmm = ReplyMessageButton.Margin;
            ReplyMessageButton.Margin = new Thickness(rmm.Left, replyTopMargin, rmm.Right, rmm.Bottom);

            // Text margin-top
            double textTopMargin = Message.IsSenderNameVisible || Message.ReplyMessage != null ? 0 : 8; // или 2 : 8 
            var mtm = MessageText.Margin;
            MessageText.Margin = new Thickness(mtm.Left, textTopMargin, mtm.Right, mtm.Bottom);
        }

        private void AddAttachments(List<Attachment> attachments) {
            MessageAttachments.Children.Clear();

            foreach (Attachment atch in attachments) {
                MessageAttachments.Children.Add(new BasicAttachment {
                    Title = atch.TypeString,
                    Subtitle = atch.TypeString,
                    Margin = new Thickness(8, 0, 8, 8),
                    Icon = (StreamGeometry)VKUI.VKUITheme.Icons["Icon24Done"]
                });
            }
        }
    }
}
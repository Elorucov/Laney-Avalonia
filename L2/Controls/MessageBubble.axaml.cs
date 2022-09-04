using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using ELOR.Laney.Controls.Attachments;
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
        const string BACKGROUND_GIFT = "GiftMessageBackground";
        const string BACKGROUND_BORDER = "BorderMessageBackground";
        const string BACKGROUND_TRANSPARENT = "TransparentMessageBackground";

        public const double BUBBLE_FIXED_WIDTH = 320;
        public const double STICKER_WIDTH = 168; // 168 в макете figma vk ipad, 176 — в vk ios, 
                                                 // 184 — android, 148 — android with reply

        #endregion

        #region Template elements

        Grid BubbleRoot;
        Border BubbleBackground;
        Avatar SenderAvatar;
        Border SenderNameWrap;
        TextBlock SenderName;
        Button ReplyMessageButton;
        RichTextBlock MessageText;
        AttachmentsContainer MessageAttachments;

        bool isUILoaded = false;
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            BubbleRoot = e.NameScope.Find<Grid>(nameof(BubbleRoot));
            BubbleBackground = e.NameScope.Find<Border>(nameof(BubbleBackground));
            SenderAvatar = e.NameScope.Find<Avatar>(nameof(SenderAvatar));
            SenderNameWrap = e.NameScope.Find<Border>(nameof(SenderNameWrap));
            SenderName = e.NameScope.Find<TextBlock>(nameof(SenderName));
            ReplyMessageButton = e.NameScope.Find<Button>(nameof(ReplyMessageButton));
            MessageText = e.NameScope.Find<RichTextBlock>(nameof(MessageText));
            MessageAttachments = e.NameScope.Find<AttachmentsContainer>(nameof(MessageAttachments));
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
            if (e.PropertyName == nameof(MessageViewModel.Text)) {
                SetText(Message.Text);
            }
            ChangeUI();

            switch (e.PropertyName) {
                case nameof(MessageViewModel.Text):
                    SetText(Message.Text);
                    break;
                case nameof(MessageViewModel.State):
                case nameof(MessageViewModel.IsImportant):
                case nameof(MessageViewModel.EditTime):
                case nameof(MessageViewModel.IsSenderNameVisible):
                    ChangeUI();
                    break;
            }
        }

        private void RenderElement() {
            if (!isUILoaded) return;

            // Outgoing
            BubbleRoot.HorizontalAlignment = IsOutgoing ? HorizontalAlignment.Right : HorizontalAlignment.Left;

            MessageUIType uiType = Message.UIType;
            bool hasReply = Message.ReplyMessage != null;
            bool singleImage = uiType == MessageUIType.SingleImage
                || (uiType == MessageUIType.Sticker && !hasReply)
                || (uiType == MessageUIType.Graffiti && !hasReply);

            // Bubble background
            var bbc = BubbleBackground.Classes;
            bbc.Clear();
            if (singleImage) {
                bbc.Add(BACKGROUND_TRANSPARENT);
            } else if ((uiType == MessageUIType.Sticker || uiType == MessageUIType.Graffiti) && hasReply) {
                bbc.Add(BACKGROUND_BORDER);
            } else if (uiType == MessageUIType.Gift) {
                bbc.Add(BACKGROUND_GIFT);
            } else {
                bbc.Add(IsOutgoing ? BACKGROUND_OUTGOING : BACKGROUND_INCOMING);
            }

            // Avatar
            SenderAvatar.IsVisible = IsChat && !IsOutgoing;

            // Sender name
            SenderNameWrap.IsVisible = !singleImage;

            // Message bubble width
            if (uiType == MessageUIType.Sticker) {
                // при BACKGROUND_BORDER у стикера будет отступ в 8px по сторонам.
                BubbleRoot.Width = hasReply ? STICKER_WIDTH + 16 : STICKER_WIDTH;
            } else if (uiType == MessageUIType.Graffiti) {
                // при BACKGROUND_BORDER у граффити будет отступ в 8px по сторонам.
                BubbleRoot.Width = hasReply ? BUBBLE_FIXED_WIDTH : BUBBLE_FIXED_WIDTH - 8;
                
            } else if (uiType == MessageUIType.Complex) {
                BubbleRoot.Width = BUBBLE_FIXED_WIDTH;
            } else {
                BubbleRoot.Width = Double.NaN;
            }

            // Attachments margin
            double amargin = 0;
            if (!hasReply) {
                if (uiType == MessageUIType.Sticker) {
                    amargin = -8;
                } else if (uiType == MessageUIType.SingleImage || uiType == MessageUIType.Graffiti) {
                    amargin = -4;
                }
            }
            MessageAttachments.Margin = new Thickness(amargin, 0, amargin, amargin);

            // Attachments
            MessageAttachments.Attachments = Message.Attachments;

            // Text
            SetText(Message.Text);

            // UI
            ChangeUI();
        }

        private void SetText(string text) {
            MessageText.Inlines.Clear();
            if (String.IsNullOrEmpty(text)) return;

            string[] t = text.Split('\n');
            for (int i = 0; i < t.Length; i++) {
                MessageText.Inlines.Add(new Run {
                    Text = t[i], 
                    // Почему-то игнорируются размер и шрифт из самого MessageText.
                    FontSize = MessageText.FontSize,
                    FontFamily = MessageText.FontFamily
                });
                if (i < t.Length - 1) MessageText.Inlines.Add(new LineBreak());
            }

            // Empty space for sent time/status
            if (Message.Attachments.Count == 0 && Message.ForwardedMessages.Count == 0) {
                MessageText.Inlines.Add(new InlineUIContainer {
                    Child = new Border {
                        Background = new SolidColorBrush(Color.FromArgb(0, 0, 122, 204)),
                        Width = 72,
                        Height = MessageText.LineHeight
                    }
                });
            }
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

            // Attachments margin-top
            double atchTopMargin = 0;

            if (Message.UIType == MessageUIType.Complex && Message.ReplyMessage == null && String.IsNullOrEmpty(Message.Text)) {
                atchTopMargin = Message.ContainsMultipleImages ? 4 : 8;
            }
            var mam = MessageAttachments.Margin;
            MessageAttachments.Margin = new Thickness(mam.Left, atchTopMargin, mam.Right, mam.Bottom);
        }
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using ColorTextBlock.Avalonia;
using ELOR.Laney.Controls.Attachments;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels.Controls;
using ELOR.VKAPILib.Objects;
using System;
using VKUI.Controls;

namespace ELOR.Laney.Controls {
    public class PostUI : TemplatedControl {
        #region Properties

        public static readonly StyledProperty<object> PostProperty =
            AvaloniaProperty.Register<MessageBubble, object>(nameof(Post));

        public object Post {
            get => GetValue(PostProperty);
            set => SetValue(PostProperty, value);
        }

        #endregion

        #region Template elements

        Avatar Avatar;
        TextBlock Author;
        TextBlock PostInfo;
        CTextBlock PostText;
        AttachmentsContainer Attachments;
        Border Map;
        Border ForwardedMessagesContainer;
        StackPanel ForwardedMessagesStack;

        bool isUILoaded = false;
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            Avatar = e.NameScope.Find<Avatar>(nameof(Avatar));
            Author = e.NameScope.Find<TextBlock>(nameof(Author));
            PostInfo = e.NameScope.Find<TextBlock>(nameof(PostInfo));
            PostText = e.NameScope.Find<CTextBlock>(nameof(PostText));
            Attachments = e.NameScope.Find<AttachmentsContainer>(nameof(Attachments));
            Map = e.NameScope.Find<Border>(nameof(Map));
            ForwardedMessagesContainer = e.NameScope.Find<Border>(nameof(ForwardedMessagesContainer));
            ForwardedMessagesStack = e.NameScope.Find<StackPanel>(nameof(ForwardedMessagesStack));

            isUILoaded = true;
            if (Post is MessageViewModel message) {
                RenderElement(message);
            } else if (Post is WallPost post) {
                // RenderElement(post);
            } else {
                throw new ArgumentException($"Post property must be {nameof(WallPost)} or {nameof(MessageViewModel)}");
            }
        }

        #endregion

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);
            if (!isUILoaded) return;

            if (change.Property == PostProperty) {
                if (Post == null) return;
                if (Post is MessageViewModel message) {
                    RenderElement(message);
                } else if (Post is WallPost post) {
                    // RenderElement(post);
                } else {
                    throw new ArgumentException($"Post property must be {nameof(WallPost)} or {nameof(MessageViewModel)}");
                }
            }
        }

        private void RenderElement(MessageViewModel message) {
            Avatar.Background = message.SenderId.GetGradient();
            Avatar.Initials = message.SenderName.GetInitials();
            Avatar.SetImageAsync(message.SenderAvatar, Avatar.Width, Avatar.Height);

            Author.Text = message.SenderName;
            PostInfo.Text = message.SentTime.ToHumanizedString(true);

            TextParser.SetText(message.Text, PostText, OnLinkClicked);
            PostText.IsVisible = !String.IsNullOrEmpty(message.Text);

            Attachments.IsVisible = message.Attachments.Count > 0;
            if (message.Attachments.Count > 0) {
                Attachments.Width = Width - Attachments.Margin.Left;
                Attachments.Gift = message.Gift;
                Attachments.Attachments = message.Attachments;
            }

            Map.Width = Width - Map.Margin.Left;
            Map.Height = Map.Width / 2;
            Map.IsVisible = message.Location != null;

            ForwardedMessagesStack.Children.Clear();
            ForwardedMessagesContainer.IsVisible = message.ReplyMessage != null || message.ForwardedMessages.Count > 0;
            var fmcmargin = ForwardedMessagesContainer.Margin;
            var fmcborder = ForwardedMessagesContainer.BorderThickness;
            var fmsmargin = ForwardedMessagesStack.Margin;
            double fmwidth = fmcmargin.Left + fmcmargin.Right + fmcborder.Left + fmsmargin.Left;
            if (message.ReplyMessage != null) {
                ForwardedMessagesStack.Children.Add(new PostUI {
                    Width = Width - fmwidth,
                    Post = message.ReplyMessage
                });
            }
            foreach (var msg in message.ForwardedMessages) {
                ForwardedMessagesStack.Children.Add(new PostUI {
                    Width = Width - fmwidth,
                    Post = msg
                });
            }
        }

        private void OnLinkClicked(string link) {
            Router.LaunchLink(VKSession.GetByDataContext(this), link);
        }
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using ColorTextBlock.Avalonia;
using ELOR.Laney.Controls.Attachments;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels.Controls;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using VKUI.Controls;

namespace ELOR.Laney.Controls {
    public class PostUI : TemplatedControl {
        const int MAX_DISPLAYED_FORWARDED_MESSAGES = 1;
        VKSession session;

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
        Button ReplyMessageButton;
        CompactMessage Reply;
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
            ReplyMessageButton = e.NameScope.Find<Button>(nameof(ReplyMessageButton));
            Reply = e.NameScope.Find<CompactMessage>(nameof(Reply));
            Attachments = e.NameScope.Find<AttachmentsContainer>(nameof(Attachments));
            Map = e.NameScope.Find<Border>(nameof(Map));
            ForwardedMessagesContainer = e.NameScope.Find<Border>(nameof(ForwardedMessagesContainer));
            ForwardedMessagesStack = e.NameScope.Find<StackPanel>(nameof(ForwardedMessagesStack));

            ReplyMessageButton.Click += ReplyMessageButton_Click;

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
            session = message.OwnerSession;
            Avatar.Background = message.SenderId.GetGradient();
            Avatar.Initials = message.SenderName.GetInitials();
            Avatar.SetImageAsync(message.SenderAvatar, Avatar.Width, Avatar.Height);

            Author.Text = message.SenderName;
            PostInfo.Text = message.SentTime.ToHumanizedString(true);

            if (message.ReplyMessage != null) { 
                Reply.Message = message.ReplyMessage;
                ReplyMessageButton.IsVisible = true;
            }

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
            ForwardedMessagesContainer.IsVisible = message.ForwardedMessages.Count > 0;
            var fmcmargin = ForwardedMessagesContainer.Margin;
            var fmcborder = ForwardedMessagesContainer.BorderThickness;
            var fmsmargin = ForwardedMessagesStack.Margin;
            double fmwidth = fmcmargin.Left + fmcmargin.Right + fmcborder.Left + fmsmargin.Left;
            foreach (var msg in message.ForwardedMessages.Take(MAX_DISPLAYED_FORWARDED_MESSAGES)) {
                ForwardedMessagesStack.Children.Add(new PostUI {
                    Width = Width - fmwidth,
                    Post = msg
                });
            }
            if (message.ForwardedMessages?.Count > MAX_DISPLAYED_FORWARDED_MESSAGES) {
                int nextFwds = message.ForwardedMessages.Count - MAX_DISPLAYED_FORWARDED_MESSAGES;

                Button fwdsButton = new Button {
                    Padding = new Thickness(0),
                    MinHeight = 16,
                    Margin = new Thickness(0, -6, 0, 0),
                    ContentTemplate = App.Current.GetCommonTemplate("ForwardedMessagesInfoTemplateAccent"),
                    Content = Localizer.GetDeclensionFormatted(nextFwds, "forwarded_message_more")
                };
                fwdsButton.Classes.Add("Tertiary");
                fwdsButton.Click += async (a, b) => {
                    StandaloneMessageViewer smv = new StandaloneMessageViewer(message.OwnerSession, message.ForwardedMessages);
                    await smv.ShowDialog(message.OwnerSession.ModalWindow);
                };
                ForwardedMessagesStack.Children.Add(fwdsButton);
            }
        }

        private async void OnLinkClicked(string link) {
            await Router.LaunchLink(session, link);
        }

        private void ReplyMessageButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            var message = Reply.Message;
            session.GoToMessage(message);
        }
    }
}

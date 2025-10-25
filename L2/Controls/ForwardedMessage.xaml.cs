using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using ColorTextBlock.Avalonia;
using ELOR.Laney.Controls.Attachments;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Linq;
using VKUI.Controls;

namespace ELOR.Laney.Controls {
    public class ForwardedMessage : TemplatedControl {
        VKSession session;

        #region Properties

        public static readonly StyledProperty<Message> MessageProperty =
            AvaloniaProperty.Register<ForwardedMessage, Message>(nameof(Message));

        public Message Message {
            get => GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public System.Action SnippetMessageClick { get; set; }

        #endregion

        #region Template elements

        Avatar Avatar;
        TextBlock Author;
        TextBlock MessageInfo;
        CTextBlock MessageText;
        Button ReplyMessageButton;
        CompactMessage Reply;
        AttachmentsContainer Attachments;
        Rectangle Map;

        Button CMButton;
        Avatar CMSenderAvatar;
        TextBlock CMSenderName;
        TextBlock CMText;
        TextBlock CMMore;

        bool isUILoaded = false;
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            Avatar = e.NameScope.Find<Avatar>(nameof(Avatar));
            Author = e.NameScope.Find<TextBlock>(nameof(Author));
            MessageInfo = e.NameScope.Find<TextBlock>(nameof(MessageInfo));
            MessageText = e.NameScope.Find<CTextBlock>(nameof(MessageText));
            ReplyMessageButton = e.NameScope.Find<Button>(nameof(ReplyMessageButton));
            Reply = e.NameScope.Find<CompactMessage>(nameof(Reply));
            Attachments = e.NameScope.Find<AttachmentsContainer>(nameof(Attachments));
            Map = e.NameScope.Find<Rectangle>(nameof(Map));
            CMButton = e.NameScope.Find<Button>(nameof(CMButton));
            CMSenderAvatar = e.NameScope.Find<Avatar>(nameof(CMSenderAvatar));
            CMSenderName = e.NameScope.Find<TextBlock>(nameof(CMSenderName));
            CMText = e.NameScope.Find<TextBlock>(nameof(CMText));
            CMMore = e.NameScope.Find<TextBlock>(nameof(CMMore));

            isUILoaded = true;
            RenderElement(Message);
            ReplyMessageButton.Click += ReplyMessageButton_Click;
            CMButton.Click += OnSnippetMessageButtonClick;
            Unloaded += ForwardedMessage_Unloaded;
        }

        private void ForwardedMessage_Unloaded(object sender, RoutedEventArgs e) {
            ReplyMessageButton.Click -= ReplyMessageButton_Click;
            CMButton.Click -= OnSnippetMessageButtonClick;
            Unloaded -= ForwardedMessage_Unloaded;
        }

        #endregion

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);
            if (!isUILoaded) return;

            if (Message == null) {
                ClearUI();
                return;
            }
            RenderElement(Message);
        }

        private void ClearUI() {
            Avatar.Initials = null;
            Avatar.Image = null;
            Author.Text = null;
            MessageInfo.Text = null;
            Reply.Message = null;
            ReplyMessageButton.IsVisible = false;
            MessageText.IsVisible = false;
            MessageText.Content.Clear();
            Attachments.IsVisible = false;
            Attachments.Attachments = null;
            Attachments.Gift = null;
            Map.IsVisible = false;
        }

        private void RenderElement(Message message) {
            session = VKSession.GetByDataContext(this);
            Avatar.Background = message.FromId.GetGradient();

            var data = CacheManager.GetNameAndAvatar(message.FromId);
            string author = String.Join(" ", new[] { data.Item1, data.Item2 });
            Avatar.Initials = author.GetInitials();
            Avatar.SetImage(data.Item3, Avatar.Width, Avatar.Height);
            Author.Text = author;

            MessageInfo.Text = message.DateTime.ToHumanizedString(true);

            if (message.ReplyMessage != null) {
                Reply.Message = message.ReplyMessage;
                ReplyMessageButton.IsVisible = true;
            } else {
                Reply.Message = null;
                ReplyMessageButton.IsVisible = false;
            }

            TextParser.SetText(message.Text, MessageText, OnLinkClicked);
            MessageText.IsVisible = !String.IsNullOrEmpty(message.Text);

            Attachments.IsVisible = message.Attachments.Count > 0;
            if (message.Attachments.Count > 0) {
                Attachments.Width = Width - Attachments.Margin.Left;
                Attachments.Gift = message.Attachments?.SingleOrDefault(a => a.Gift != null)?.Gift;
                Attachments.Owner = CacheManager.GetNameOnly(message.FromId);
                Attachments.Attachments = message.Attachments;
            }

            TrySetupMap(message);

            if (message.ForwardedMessages?.Count > 0) {
                int msgsCount = message.ForwardedMessages.Count;
                var fmsg = message.ForwardedMessages.FirstOrDefault();

                var fmsgData = CacheManager.GetNameAndAvatar(fmsg.FromId);
                string fmsgAuthor = String.Join(" ", new[] { fmsgData.Item1, fmsgData.Item2 });
                CMSenderAvatar.Initials = fmsgAuthor.GetInitials();
                CMSenderAvatar.SetImage(fmsgData.Item3, CMSenderAvatar.Width, CMSenderAvatar.Height);
                CMSenderName.Text = fmsgAuthor;
                CMText.Text = fmsg.ToNormalString();

                if (msgsCount > 1) {
                    CMMore.Text = Localizer.GetDeclensionFormatted(msgsCount - 1, "forwarded_message_more");
                    CMMore.IsVisible = true;
                } else {
                    CMMore.IsVisible = false;
                }

                CMButton.IsVisible = true;
            } else {
                CMButton.IsVisible = false;
            }
        }

        private void TrySetupMap(Message message) {
            if (Bounds.Width == 0) {
                Log.Warning($"ForwardedMessage > TrySetupMap: UI is not ready, so its width is 0. Trying in next frame...");
                var tl = TopLevel.GetTopLevel(this);
                tl.RequestAnimationFrame((t) => TrySetupMap(message));
                return;
            }
            Log.Warning($"ForwardedMessage > TrySetupMap: UI width is {Bounds.Width}.");
            Map.Width = Bounds.Width - Map.Margin.Left;
            Map.Height = Map.Width / 2;
            Map.IsVisible = message.Geo != null;
            if (message.Geo != null) {
                var glong = message.Geo.Coordinates.Longitude.ToString().Replace(",", ".");
                var glat = message.Geo.Coordinates.Latitude.ToString().Replace(",", ".");
                var w = Math.Ceiling(Map.Width * App.Current.DPI);
                var h = Math.Ceiling(Map.Height * App.Current.DPI);
                Map.SetImageFill(new Uri($"https://static-maps.yandex.ru/1.x/?ll={glong},{glat}&size={w},{h}&z=12&lang=ru_RU&l=pmap&pt={glong},{glat},vkbkm"), Map.Width, Map.Height);
            }
        }

        private void OnLinkClicked(string link) {
            new System.Action(async () => await Router.LaunchLink(session, link))();
        }

        private void ReplyMessageButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            var message = Reply.Message;
            session.GoToMessage(message);
        }

        private void OnSnippetMessageButtonClick(object sender, RoutedEventArgs e) {
            SnippetMessageClick?.Invoke();
        }
    }
}

using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels.Controls;
using ELOR.VKAPILib.Objects;
using System;

namespace ELOR.Laney.DataModels {
    public class FoundMessageItem {
        public int Id { get; private set; }
        public long PeerId { get; private set; }
        public string PeerName { get; private set; }
        public Uri PeerAvatar { get; private set; }
        public string Text { get; private set; }
        public DateTime SentDate { get; private set; }
        // public string NormalizedLastMessageTime { get { return SentDate.ToHumanizedTimeOrDateString(); } }

        public FoundMessageItem(bool isOutgoing, Message message, Conversation conversation = null) {
            MessageViewModel msg = new MessageViewModel(message);
            if (conversation.Peer.Type == PeerType.Chat) {
                PeerName = conversation.ChatSettings.Title;
                if (!String.IsNullOrEmpty(conversation.ChatSettings.Photo?.MediumUrl)) PeerAvatar = new Uri(conversation.ChatSettings.Photo.MediumUrl);

                string sname;
                if (!isOutgoing) {
                    var senderInfo = CacheManager.GetNameOnly(message.FromId, true);
                    sname = senderInfo;
                } else {
                    sname = Localizer.Instance["you"];
                }
                Text = $"{sname}: {msg}";
            } else {
                var info = CacheManager.GetNameAndAvatar(conversation.Peer.Id);
                PeerName = String.Join(" ", new string[] { info.Item1, info.Item2 });
                PeerAvatar = info.Item3;

                string youSign = isOutgoing ? $"{Localizer.Instance["you"]}: " : String.Empty;
                Text = youSign + msg.ToString();
            }

            Id = message.ConversationMessageId;
            PeerId = message.PeerId;
            SentDate = message.DateTime;
        }
    }
}
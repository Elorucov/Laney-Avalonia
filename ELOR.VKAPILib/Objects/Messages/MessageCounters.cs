using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Messages {
    public sealed class FoldersCounterItem {
        public FoldersCounterItem() { }

        [JsonPropertyName("folder_id")]
        public int FolderId { get; set; }

        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }

        [JsonPropertyName("unmuted_count")]
        public int UnmutedCount { get; set; }
    }

    public sealed class MessageCounters {
        public MessageCounters() { }

        [JsonPropertyName("ad_tag")]
        public int AdTag { get; set; }

        [JsonPropertyName("business_notify")]
        public int BusinessNotify { get; set; }

        [JsonPropertyName("business_notify_all")]
        public int BusinessNotifyAll { get; set; }

        [JsonPropertyName("calls")]
        public int Calls { get; set; }

        [JsonPropertyName("important")]
        public int Important { get; set; }

        [JsonPropertyName("message_requests")]
        public int MessageRequests { get; set; }

        [JsonPropertyName("messages")]
        public int Messages { get; set; }

        [JsonPropertyName("messages_archive")]
        public int MessagesArchive { get; set; }

        [JsonPropertyName("messages_archive_mentions_count")]
        public int MessagesArchiveMentionsCount { get; set; }

        [JsonPropertyName("messages_archive_unread")]
        public int MessagesArchiveUnread { get; set; }

        [JsonPropertyName("messages_archive_unread_unmuted")]
        public int MessagesArchiveUnreadUnmuted { get; set; }

        [JsonPropertyName("messages_folders")]
        public List<FoldersCounterItem> MessagesFolders { get; set; }

        [JsonPropertyName("messages_unread_unmuted")]
        public int MessagesUnreadUnmuted { get; set; }

        [JsonPropertyName("unanswered")]
        public int Unanswered { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Messages {
    public sealed class  DiffChangedObjects {
        public DiffChangedObjects() { }

        [JsonPropertyName("delete_items")]
        public List<long> DeleteItems { get; set; }

        [JsonPropertyName("items")]
        public List<long> Items { get; set; }
    }

    public sealed class DiffUpdatedCmid {
        public DiffUpdatedCmid() { }

        [JsonPropertyName("cmid")]
        public int ConversationMessageId { get; set; }

        [JsonPropertyName("updated_flags")]
        public long UpdatedFlags { get; set; }
    }

    public sealed class DiffNewMessages {
        public DiffNewMessages() { }

        [JsonPropertyName("cmids")]
        public List<int> ConversationMessageIds { get; set; }

        [JsonPropertyName("expired_cmids")]
        public List<int> ExpiredCMIDs { get; set; }

        [JsonPropertyName("mention_cmids")]
        public List<int> MentionCMIDs { get; set; }
    }

    public sealed class ConversationDiff {
        public ConversationDiff() { }

        [JsonPropertyName("in_read_cmid")]
        public int InReadCmid { get; set; }

        [JsonPropertyName("is_archived")]
        public bool IsArchived { get; set; }

        [JsonPropertyName("new_msgs")]
        public DiffNewMessages NewMessages { get; set; }

        [JsonPropertyName("out_read_cmid")]
        public int OutReadCmid { get; set; }

        [JsonPropertyName("peer_id")]
        public long PeerId { get; set; }

        [JsonPropertyName("sort_major_id")]
        public int SortMajorId { get; set; }

        [JsonPropertyName("sort_minor_id")]
        public int SortMinorId { get; set; }

        [JsonPropertyName("unread_count")]
        public int UnreadCount { get; set; }

        [JsonPropertyName("version")]
        public long Version { get; set; }
    }

    public sealed class DiffCMIDRange {
        public DiffCMIDRange() { }

        [JsonPropertyName("max")]
        public int Max { get; set; }

        [JsonPropertyName("min")]
        public int Min { get; set; }
    }

    public sealed class DiffConversationInfo {
        public DiffConversationInfo() { }

        [JsonPropertyName("cmids_flags")]
        public List<DiffUpdatedCmid> CMIDsFlags { get; set; }

        [JsonPropertyName("cmids_updated_reactions")]
        public List<int> CMIDsUpdatedReactions { get; set; }

        [JsonPropertyName("conversation")]
        public Conversation Conversation { get; set; }

        [JsonPropertyName("conversation_diff")]
        public ConversationDiff ConversationDiff { get; set; }

        [JsonPropertyName("invalidate")]
        public bool Invalidate { get; set; }

        [JsonPropertyName("members_changed")]
        public bool MembersChanged { get; set; }

        [JsonPropertyName("message")] // Yes, "message". Without "s".
        public List<Message> Messages { get; set; }

        [JsonPropertyName("range_deleted_cmids")]
        public List<DiffCMIDRange> RangeDeletedCMIDs { get; set; }

        [JsonPropertyName("range_updated_cmids")]
        public List<DiffCMIDRange> RangeUpdatedCMIDs { get; set; }
    }

    // TODO: folders
    public sealed class GetDiffResponse {
        public GetDiffResponse() { }

        [JsonPropertyName("changed_objects")]
        public DiffChangedObjects ChangedObjects { get; set; }

        [JsonPropertyName("conversations_info")]
        public List<DiffConversationInfo> ConversationsInfo { get; set; }

        [JsonPropertyName("invalidate_all")]
        public bool InvalidateAll { get; set; }

        [JsonPropertyName("counters")]
        public MessageCounters Counters { get; set; }

        [JsonPropertyName("credentials")]
        public LongPollServerInfo Credentials { get; set; }

        [JsonPropertyName("server_time")]
        public long ServerTime { get; set; }

        [JsonPropertyName("server_version")]
        public long ServerVersion { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }
    }
}

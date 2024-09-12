using ELOR.VKAPILib.Attributes;
using ELOR.VKAPILib.Objects;
using ELOR.VKAPILib.Objects.Messages;
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ELOR.VKAPILib.Methods {
    public enum ConversationsFilter {
        [EnumMember(Value = "all")]
        All,

        [EnumMember(Value = "unread")]
        Unread,

        [EnumMember(Value = "important")]
        Important,

        [EnumMember(Value = "unanswered")]
        Unanswered,

        [EnumMember(Value = "message_request")]
        MessageRequest,

        [EnumMember(Value = "business_notify")]
        BusinessNotify
    }
    
    public enum HistoryAttachmentMediaType {
        [EnumMember(Value = "photo")]
        Photo,

        [EnumMember(Value = "video")]
        Video,

        [EnumMember(Value = "audio")]
        Audio,

        [EnumMember(Value = "doc")]
        Doc,

        [EnumMember(Value = "link")]
        Link,

        [EnumMember(Value = "market")]
        Market,

        [EnumMember(Value = "wall")]
        Wall,

        [EnumMember(Value = "share")]
        Share,

        [EnumMember(Value = "graffiti")]
        Graffiti,

        [EnumMember(Value = "audio_message")]
        AudioMessage
    }
    
    public enum MessageIntent {
        None,

        [EnumMember(Value = "promo_newsletter")]
        PromoNewsletter,

        [EnumMember(Value = "bot_ad_invite")]
        BotAdInvite,

        [EnumMember(Value = "bot_ad_promo")]
        BotAdPromo
    }
    
    public enum ActivityType {
        [EnumMember(Value = "typing")]
        Typing,

        [EnumMember(Value = "audiomessage")]
        Audiomessage,

        [EnumMember(Value = "photo")]
        Photo,

        [EnumMember(Value = "video")]
        Video,

        [EnumMember(Value = "file")]
        File,
    }

    public class MessagesMethods : MethodsSectionBase {
        internal MessagesMethods(VKAPI api) : base(api) { }

        /// <summary>Adds a new user to a chat.</summary>
        /// <param name="chatId">Chat ID.</param>
        /// <param name="userId">ID of the user to be added to the chat.</param>
        /// <param name="visibleMessagesCount">Visible messages count.</param>
        /// <param name="groupId">Group ID.</param>
        public async Task<bool> AddChatUserAsync(long groupId, long chatId, long userId, int visibleMessagesCount = 0) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "chat_id", chatId.ToString() },
                { "user_id", userId.ToString() },
                { "visible_messages_count", visibleMessagesCount.ToString() }
            };
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            return await API.CallMethodAsync<int>("messages.addChatUser", parameters) == 1;
        }

        /// <remarks>This method is undocumented!</remarks>
        /// <summary>Create a new template.</summary>
        /// <param name="groupId">Group ID.</param>
        public async Task<AddTemplateResponse> AddTemplateAsync(long groupId, string name, string text) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "group_id", groupId.ToString() },
                { "name", name },
                { "text", text }
            };
            return await API.CallMethodAsync<AddTemplateResponse>("messages.addTemplate", parameters);
        }

        /// <summary>Allows sending messages from community to the current user.</summary>
        /// <param name="groupId">Group ID.</param>
        /// <param name="key">Random string, can be used for the user identification. It returns with message_allow event in Callback API.</param>
        public async Task<bool> AllowMessagesFromGroupAsync(long groupId, string key = "") {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "group_id", groupId.ToString() },
                { "key", key }
            };
            return await API.CallMethodAsync<int>("messages.allowMessagesFromGroup", parameters) == 1;
        }

        /// <summary>Creates a chat with several participants.</summary>
        /// <param name="groupId">Group ID.</param>
        /// <param name="userIds">IDs of the users to be added to the chat.</param>
        /// <param name="title">Chat title.</param>
        public async Task<CreateChatResponse> CreateChatAsync(long groupId, List<long> userIds, string title = "") {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("user_ids", userIds.Combine());
            parameters.Add("title", title);
            return await API.CallMethodAsync<CreateChatResponse>("messages.createChat", parameters);
        }

        /// <summary>Deletes one or more messages.</summary>
        /// <param name="groupId">Group ID.</param>
        /// <param name="peerId">Peer ID.</param>
        /// <param name="cmids">Message IDs in conversation.</param>
        /// <param name="spam">true — to mark message as spam.</param>
        /// <param name="deleteForAll">true — to delete message for all (in 24 hours from the sending time).</param>
        public async Task<List<MessageDeleteResponse>> DeleteAsync(long groupId, long peerId, List<int> cmids, bool spam, bool deleteForAll) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("cmids", cmids.Combine());
            if (spam) parameters.Add("spam", "1");
            if (deleteForAll) parameters.Add("delete_for_all", "1");
            return await API.CallMethodAsync<List<MessageDeleteResponse>>("messages.delete", parameters);
        }

        /// <summary>Deletes a chat's cover picture.</summary>
        /// <param name="groupId">Group ID.</param>
        /// <param name="chatId">Chat ID.</param>
        public async Task<SetChatPhotoResponse> DeleteChatPhotoAsync(long groupId, long chatId) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("chat_id", chatId.ToString());
            return await API.CallMethodAsync<SetChatPhotoResponse>("messages.deleteChatPhoto", parameters);
        }

        /// <summary>Deletes private messages in a conversation.</summary>
        /// <param name="groupId">Group ID.</param>
        /// <param name="peerId">Destination ID.</param>
        public async Task<DeleteConversationResponse> DeleteConversationAsync(long groupId,  long peerId) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "peer_id", peerId.ToString() }
            };
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            return await API.CallMethodAsync<DeleteConversationResponse>("messages.deleteConversation", parameters);
        }

        /// <summary>Delete a reaction from message.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="cmId">ID of message in conv.</param>
        /// <param name="reactionId">Reaction ID.</param>
        public async Task<bool> DeleteReactionAsync(long groupId, long peerId, int cmId) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("cmid", cmId.ToString());
            return await API.CallMethodAsync<int>("messages.deleteReaction", parameters) == 1;
        }

        /// <remarks>This method is undocumented!</remarks>
        /// <summary>Delete template.</summary>
        /// <param name="groupId">Group ID.</param>
        public async Task<bool> DeleteTemplateAsync(long groupId, int templateId) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "group_id", groupId.ToString() },
                { "template_id", templateId.ToString() }
            };
            return await API.CallMethodAsync<bool>("messages.deleteTemplate", parameters);
        }

        /// <summary>Denies sending message from community to the current user.</summary>
        /// <param name="groupId">Group ID.</param>
        public async Task<bool> DenyMessagesFromGroupAsync(long groupId) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "group_id", groupId.ToString() }
            };
            return await API.CallMethodAsync<int>("messages.denyMessagesFromGroup", parameters) == 1;
        }

        /// <summary>Edits the message. You can edit sent message during 24 hours.</summary>
        /// <param name="groupId">Group ID.</param>
        /// <param name="peerId">Destination ID.</param>
        /// <param name="cmid">Message ID in conversation.</param>
        /// <param name="message">(Required if attachment parameter is null.) Text of the message.</param>
        /// <param name="latitude">Geographical latitude of a check-in, in degrees (from -90 to 90).</param>
        /// <param name="longitude">Geographical longitude of a check-in, in degrees (from -180 to 180).</param>
        /// <param name="attachment">(Required if message parameter is null.) List of objects attached to the message.</param>
        /// <param name="keepForwardedMessages">true — to keep forwarded, messages.</param>
        /// <param name="keepSnippets">true — to keep attached snippets.</param>
        /// <param name="dontParseLinks">Don't parse links in message.</param>
        public async Task<int> EditAsync(long groupId, long peerId, int cmid, string message, double latitude, double longitude, List<string> attachment, bool keepForwardedMessages, bool keepSnippets, bool dontParseLinks) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("conversation_message_id", cmid.ToString());
            if (!String.IsNullOrEmpty(message)) parameters.Add("message", message);
            if (latitude > 0) parameters.Add("lat", latitude.ToString());
            if (longitude > 0) parameters.Add("long", longitude.ToString());
            if (!attachment.IsNullOrEmpty()) parameters.Add("attachment", attachment.Combine());
            if (keepForwardedMessages) parameters.Add("keep_forward_messages", "1");
            if (keepSnippets) parameters.Add("keep_snippets", "1");
            if (dontParseLinks) parameters.Add("dont_parse_links", "1");
            return await API.CallMethodAsync<int>("messages.edit", parameters);
        }

        /// <summary>Edits the title of a chat.</summary>
        /// <param name="chatId">Chat ID.</param>
        /// <param name="title">New title of the chat.</param>
        /// <param name="permissions">Permissions.</param>
        public async Task<bool> EditChatAsync(long chatId, string title, string permissions = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "chat_id", chatId.ToString() }
            };
            if (!String.IsNullOrEmpty(title)) parameters.Add("title", title);
            if (!String.IsNullOrEmpty(permissions)) parameters.Add("permissions", permissions);
            return await API.CallMethodAsync<bool>("messages.editChat", parameters);
        }

        /// <remarks>This method is undocumented!</remarks>
        /// <summary>Edit template.</summary>
        /// <param name="groupId">Group ID.</param>
        public async Task<bool> EditTemplateAsync(long groupId, int templateId, string name, string text) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "group_id", groupId.ToString() },
                { "template_id", templateId.ToString() },
                { "name", name },
                { "text", text }
            };
            return await API.CallMethodAsync<bool>("messages.editTemplate", parameters);
        }

        /// <summary>Returns messages by their ids as part of a conversation.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="peerId">Peer ID.</param>
        /// <param name="conversationMessageIds">Conversation message IDs.</param>
        /// <param name="extended">true – return additional information about users and communities in users and communities fields.</param>
        /// <param name="fields">List of additional fields for users and communities.</param>
        public async Task<MessagesList> GetByConversationMessageIdAsync(long groupId, long peerId, List<int> conversationMessageIds, bool extended = false, List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("conversation_message_ids", conversationMessageIds.Combine());
            if (extended) parameters.Add("extended", "1");
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<MessagesList>("messages.getByConversationMessageId", parameters);
        }

        /// <summary>Returns messages by their IDs.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="messageIds">Message IDs.</param>
        /// <param name="extended">true – return additional information about users and communities in users and communities fields.</param>
        /// <param name="fields">List of additional fields for users and communities.</param>
        public async Task<MessagesList> GetByIdAsync(long groupId, List<int> messageIds, int previewLength, bool extended = false, List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("message_ids", messageIds.Combine());
            if (previewLength > 0) parameters.Add("preview_length", previewLength.ToString());
            if (extended) parameters.Add("extended", "1");
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<MessagesList>("messages.getById", parameters);
        }

        /// <summary>Allows to receive chat preview by the invitation link.</summary>
        /// <param name="link">Invitation link.</param>
        /// <param name="fields">List of additional fields for users and communities.</param>
        public async Task<ChatPreviewResponse> GetChatPreviewAsync(string link, List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "link", link }
            };
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<ChatPreviewResponse>("messages.getChatPreview", parameters);
        }

        /// <summary>Returns a list of conversations.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="fields">List of additional fields for users and communities.</param>
        /// <param name="filter">Types of conversations to return.</param>
        /// <param name="count">Number of conversations to return.</param>
        /// <param name="offset">Offset needed to return a specific subset of conversations.</param>
        /// <param name="extended">true – return additional information about users and communities in users and communities fields.</param>
        public async Task<ConversationsResponse> GetConversationsAsync(long groupId, List<string> fields, ConversationsFilter filter, bool extended = false, int count = 60, int offset = 0) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("filter", filter.ToEnumMemberAttribute());
            parameters.Add("count", count.ToString());
            parameters.Add("offset", offset.ToString());
            if (extended) parameters.Add("extended", "1");
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<ConversationsResponse>("messages.getConversations", parameters);
        }

        /// <summary>Returns conversations by their IDs.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="peerIds">list of destination IDs.</param>
        /// <param name="extended">true – return additional information about users and communities in users and communities fields.</param>
        /// <param name="fields">List of additional fields for users and communities.</param>
        public async Task<ConversationsList> GetConversationsByIdAsync(long groupId, List<long> peerIds, bool extended = false, List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_ids", peerIds.Combine());
            if (extended) parameters.Add("extended", "1");
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<ConversationsList>("messages.getConversationsById", parameters);
        }

        /// <summary>Returns message history for the specified user or group chat.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="peerId">Peer ID.</param>
        /// <param name="offset">Offset needed to return a specific subset of messages.</param>
        /// <param name="count">Number of messages to return.</param>
        /// <param name="startMessageId">Starting message ID from which to return history.</param>
        /// <param name="extended">true – return additional information about users and communities in users and communities fields.</param>
        /// <param name="fields">List of additional fields for users and communities.</param>
        /// <param name="rev">Sort order.</param>
        public async Task<MessagesHistoryResponse> GetHistoryAsync(long groupId, long peerId, int offset, int count, int startMessageId, bool extended = false, List<string> fields = null, bool rev = false) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("offset", offset.ToString());
            parameters.Add("count", count.ToString());
            parameters.Add("start_message_id", startMessageId.ToString());
            if (rev) parameters.Add("rev", "1");
            if (extended) parameters.Add("extended", "1");
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<MessagesHistoryResponse>("messages.getHistory", parameters);
        }

        /// <summary>Returns media files from the dialog or group chat.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="peerId">Peer ID.</param>
        /// <param name="mediaType">Type of media files to return.</param>
        /// <param name="startFrom">Message ID to start return results from.</param>
        /// <param name="count">Number of messages to return.</param>
        /// <param name="photoSizes">true — to return photo sizes.</param>
        /// <param name="fields">List of additional fields for users and communities.</param>
        /// <param name="preserveOrder">Preserve order.</param>
        /// <param name="maxForwardsLevel">Preserve order.</param>
        public async Task<ConversationAttachmentsResponse> GetHistoryAttachmentsAsync(long groupId, long peerId, HistoryAttachmentMediaType mediaType, int startFrom, int count, bool photoSizes, bool preserveOrder = false, int maxForwardsLevel = 45, List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("media_type", mediaType.ToEnumMemberAttribute());
            parameters.Add("start_from", startFrom.ToString());
            parameters.Add("count", count.ToString());
            if (photoSizes) parameters.Add("photo_sizes", "1");
            if (preserveOrder) parameters.Add("preserve_order", "1");
            parameters.Add("max_forwards_level", maxForwardsLevel.ToString());
            parameters.Add("extended", "1");
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<ConversationAttachmentsResponse>("messages.getHistoryAttachments", parameters);
        }

        /// <summary>Returns a list of user's important messages.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="offset">Offset needed to return a specific subset of messages.</param>
        /// <param name="count">Number of messages to return.</param>
        /// <param name="startMessageId">Starting message ID from which to return list.</param>
        /// <param name="previewLength">Preview length.</param>
        /// <param name="extended">true – return additional information about users and communities in users and communities fields.</param>
        /// <param name="fields">List of additional fields for users and communities.</param>
        public async Task<ImportantMessagesResponse> GetImportantMessagesAsync(long groupId, int offset, int count, int startMessageId, int previewLength = 0, bool extended = false, List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("offset", offset.ToString());
            parameters.Add("count", count.ToString());
            parameters.Add("start_message_id", startMessageId.ToString());
            if (previewLength > 0) parameters.Add("preview_length", previewLength.ToString());
            if (extended) parameters.Add("extended", "1");
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<ImportantMessagesResponse>("messages.getImportantMessages", parameters);
        }

        /// <summary>Receives a link to invite a user to the chat.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="peerId">Peer ID.</param>
        /// <param name="reset">true — to generate new link (revoke previous).</param>
        public async Task<ChatLink> GetInviteLinkAsync(long groupId, long peerId, bool reset = false) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            if (reset) parameters.Add("reset", "1");
            return await API.CallMethodAsync<ChatLink>("messages.getInviteLink", parameters);
        }

        /// <summary>Returns a user's current status and date of last activity.</summary>
        /// <param name="userId">User ID.</param>
        public async Task<LastActivity> GetLastActivityAsync(long userId) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "user_id", userId.ToString() }
            };
            return await API.CallMethodAsync<LastActivity>("messages.getLastActivity", parameters);
        }

        /// <summary>Returns updates in user's private messages.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="ts">Last value of the ts parameter returned from the Long Poll server.</param>
        /// <param name="pts">Last value of pts parameter returned from the Long Poll server.</param>
        /// <param name="previewLength">Number of characters after which to truncate a previewed message. To preview the full message, specify 0.</param>
        /// <param name="onlines">true — to return history with online users only.</param>
        /// <param name="fields">Additional profile fileds to return.</param>
        /// <param name="eventsLimit">Maximum number of events to return. (minimum 1000)</param>
        /// <param name="msgsLimit">Maximum number of messages to return. (minimum 200)</param>
        /// <param name="maxMsgId">Maximum ID of the message among existing ones in the local copy.</param>
        public async Task<LongPollHistoryResponse> GetLongPollHistoryAsync(long groupId, int version, int ts, int pts, int previewLength, bool onlines, int eventsLimit = 1000, int msgsLimit = 200, int maxMsgId = 0, List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if(groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("lp_version", version.ToString());
            parameters.Add("ts", ts.ToString());
            parameters.Add("pts", pts.ToString());
            if (previewLength > 0) parameters.Add("preview_length", previewLength.ToString());
            parameters.Add("events_limit", eventsLimit.ToString());
            parameters.Add("msgs_limit", msgsLimit.ToString());
            if (maxMsgId > 0) parameters.Add("max_msg_id", maxMsgId.ToString());
            parameters.Add("credentials", "1");
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<LongPollHistoryResponse>("messages.getLongPollHistory", parameters);
        }

        /// <summary>Returns data required for connection to a Long Poll server.</summary>
        /// <param name="needPts">true — to return the pts field, needed for the GetLongPollHistoryAsync method.</param>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        public async Task<LongPollServerInfo> GetLongPollServerAsync(bool needPts, int version, long groupId) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            if (needPts) parameters.Add("need_pts", "1");
            parameters.Add("lp_version", version.ToString());
            return await API.CallMethodAsync<LongPollServerInfo>("messages.getLongPollServer", parameters);
        }

        /// <summary>Returns members who read message.</summary>
        /// <param name="peerId">Peer ID.</param>
        /// <param name="cmid">Conversation message ID.</param>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        public async Task<LongList> GetMessageReadPeersAsync(long groupId, long peerId, int cmid, int offset = 0, int count = 50, List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "peer_id", peerId.ToString() },
                { "cmid", cmid.ToString() },
                { "offset_major_id", offset.ToString() },
                { "count", count.ToString() },
                { "extended", "1" }
            };
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<LongList>("messages.getMessageReadPeers", parameters);
        }

        /// <summary>Returns reacted members.</summary>
        /// <param name="peerId">Peer ID.</param>
        /// <param name="cmid">Conversation message ID.</param>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        public async Task<ReactedPeersList> GetReactedPeersAsync(long groupId, long peerId, int cmid) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "peer_id", peerId.ToString() },
                { "cmid", cmid.ToString() },
                { "extended", "1" },
                { "fields", "photo_100,photo_50" }
            };
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            return await API.CallMethodAsync<ReactedPeersList>("messages.getReactedPeers", parameters);
        }

        /// <summary>Returns a list of recently used graffities.</summary>
        /// <param name="limit">Group ID.</param>
        public async Task<List<Document>> GetRecentGraffitiesAsync(int limit = 20) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "limit", limit.ToString() }
            };
            return await API.CallMethodAsync<List<Document>>("messages.getRecentGraffities", parameters);
        }

        /// <remarks>This method is undocumented!</remarks>
        /// <summary>Returns templates.</summary>
        /// <param name="groupId">Group ID.</param>
        public async Task<MessageTemplatesList> GetTemplatesAsync(long groupId, int extended = 1) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "group_id", groupId.ToString() }
            };
            if (extended != 0) parameters.Add("extended", "1");
            return await API.CallMethodAsync<MessageTemplatesList>("messages.getTemplates", parameters);
        }

        /// <summary>Returns information whether sending messages from the community to current user is allowed.</summary>
        /// <param name="groupId">Group ID.</param>
        /// <param name="userId">User ID.</param>
        public async Task<IsAllowedResponse> IsMessagesFromGroupAllowedAsync(long groupId, long userId) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "group_id", groupId.ToString() },
                { "user_id", userId.ToString() }
            };
            return await API.CallMethodAsync<IsAllowedResponse>("messages.isMessagesFromGroupAllowed", parameters);
        }

        /// <summary>Allows to enter the chat by the invitation link.</summary>
        /// <param name="link">Invitation link.</param>
        public async Task<JoinChatResponse> JoinChatByInviteLinkAsync(string link) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "link", link }
            };
            return await API.CallMethodAsync<JoinChatResponse>("messages.joinChatByInviteLink", parameters);
        }

        /// <summary>Marks/unmarks the conversation as answered.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="peerId">Peer ID.</param>
        /// <param name="answered">true — to mark conversation as answered, false — unmark.</param>
        public async Task<bool> MarkAsAnsweredConversationAsync(long groupId, long peerId, bool answered) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("answered", answered ? "1" : "0");
            return await API.CallMethodAsync<bool>("messages.markAsAnsweredConversation", parameters);
        }

        /// <summary>Marks and unmarks messages as important (starred). Only from 5.217</summary>
        /// <param name="peerId">Peer ID.</param>
        /// <param name="cmids">IDs of messages in conversation to mark as important.</param>
        /// <param name="important">true — to add a star (mark as important), false — to remove the star.</param>
        public async Task<MarkAsImportantResponse> MarkAsImportantAsync(long peerId, List<int> cmids, bool important) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "peer_id", peerId.ToString() },
                { "cmids", cmids.Combine() },
                { "important", important ? "1" : "0" }
            };
            return await API.CallMethodAsync<MarkAsImportantResponse>("messages.markAsImportant", parameters);
        }

        /// <summary>Marks/unmarks the conversation as important.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="peerId">Peer ID.</param>
        /// <param name="important">true — to mark conversation as important, false — unmark.</param>
        public async Task<bool> MarkAsImportantConversationAsync(long groupId, long peerId, bool important) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("important", important ? "1" : "0");
            return await API.CallMethodAsync<bool>("messages.markAsImportantConversation", parameters);
        }

        /// <summary>Marks messages as read.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="upToCMID">IDs of messages in conversation to mark as read.</param>
        /// <param name="peerId">Destination ID.</param>
        public async Task<bool> MarkAsReadAsync(long groupId, long peerId, int upToCMID, bool markConversationAsRead = false) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            if (upToCMID > 0) parameters.Add("up_to_cmid", upToCMID.ToString());
            if (markConversationAsRead) parameters.Add("mark_conversation_as_read", "1");

            return await API.CallMethodAsync<int>("messages.markAsRead", parameters) == 1;
        }

        /// <remarks>This method is undocumented!</remarks>
        /// <summary>Marks conversation as unread.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="peerId">Destination ID.</param>
        public async Task<bool> MarkAsUnreadConversationAsync(long groupId, long peerId) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            return await API.CallMethodAsync<bool>("messages.markAsUnreadConversation", parameters);
        }

        /// <summary>Marks reactions as read.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="peerId">Destination ID.</param>
        /// <param name="cmIds">List of IDs of messages with reactions in conversation to mark as read.</param>
        public async Task<bool> MarkReactionsAsReadAsync(long groupId, long peerId, List<int> cmIds) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("cmids", cmIds.Combine());

            return await API.CallMethodAsync<int>("messages.markReactionsAsRead", parameters) == 1;
        }

        /// <summary>Pins a message.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="peerId">Destination ID.</param>
        /// <param name="conversationMessageId">ID of message in conv. to pin.</param>
        public async Task<Message> PinAsync(long groupId, long peerId, int conversationMessageId) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("conversation_message_id", conversationMessageId.ToString());
            return await API.CallMethodAsync<Message>("messages.pin", parameters);
        }

        /// <summary>Allows the current user to leave a chat or, if the current user started the chat, allows the user to remove another user from the chat.</summary>
        /// <param name="chatId">Chat ID.</param>
        /// <param name="memberId">ID of the member to be removed.</param>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        public async Task<bool> RemoveChatUserAsync(long groupId, long chatId, long memberId) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "chat_id", chatId.ToString() },
                { "member_id", memberId.ToString() }
            };
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            return await API.CallMethodAsync<int>("messages.removeChatUser", parameters) == 1;
        }

        /// <summary>Restores a deleted message.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="messageId">ID of a previously-deleted message to restore.</param>
        public async Task<bool> RestoreAsync(long groupId, int messageId) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("message_id", messageId.ToString());
            return await API.CallMethodAsync<bool>("messages.restore", parameters);
        }

        /// <summary>Returns a list of the current user's private messages that match search criteria.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="query">Search query.</param>
        /// <param name="peerId">Destination ID.</param>
        /// <param name="date">Date to search messages have been sent before it.</param>
        /// <param name="previewLength">Number of characters after which to truncate a previewed message. To preview the full message, specify 0.</param>
        /// <param name="offset">Offset needed to return a specific subset of messages.</param>
        /// <param name="count">Number of messages to return.</param>
        /// <param name="extended">true — to return additional profiles and groups array with users and communities objects.</param>
        /// <param name="fields">List of additional fields for profiles and communities to be returned.</param>
        public async Task<MessagesList> SearchAsync(long groupId, string query, long peerId, DateTime? date = null, int previewLength = 0, int offset = 0, int count = 20, bool extended = false, List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("q", query);
            if (peerId > 0) parameters.Add("peer_id", peerId.ToString());
            if (date != null) parameters.Add("date", date.Value.ToVKFormat());
            if (previewLength > 0) parameters.Add("preview_length", previewLength.ToString());
            parameters.Add("offset", offset.ToString());
            parameters.Add("count", count.ToString());
            if (extended) parameters.Add("extended", "1");
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<MessagesList>("messages.search", parameters);
        }

        /// <summary>Returns a list of conversations that match search criteria.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="query">Search query.</param>
        /// <param name="count">Maximum number of results.</param>
        /// <param name="extended">true — return additional fields.</param>
        /// <param name="fields">List of additional fields for profiles and communities to be returned.</param>
        public async Task<ConversationsList> SearchConversationsAsync(long groupId, string query, int count = 20, bool extended = false, List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("q", query);
            parameters.Add("count", count.ToString());
            if(extended) parameters.Add("extended", "1");
            if(!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<ConversationsList>("messages.searchConversations", parameters);
        }

        /// <summary>Sends a message.</summary>
        /// <param name="groupId">Group ID.</param>
        /// <param name="peerId">Destination ID.</param>
        /// <param name="randomId">Unique identifier to avoid resending the message.</param>
        /// <param name="message">(Required if attachment parameter is null.) Text of the message.</param>
        /// <param name="latitude">Geographical latitude of a check-in, in degrees (from -90 to 90).</param>
        /// <param name="longitude">Geographical longitude of a check-in, in degrees (from -180 to 180).</param>
        /// <param name="attachment">(Required if message parameter is null.) List of objects attached to the message.</param>
        /// <param name="replyTo">Id of replied message.</param>
        /// <param name="forwardMessages">ID of forwarded messages. Listed messages of the sender will be shown in the message body at the recipient's.</param>
        /// <param name="stickerId">Sticker id.</param>
        /// <param name="keyboard">Keyboard (for bots).</param>
        /// <param name="payload">Payload of message.</param>
        /// <param name="dontParseLinks">true — links will not attach snippet.</param>
        /// <param name="disableMentions">true — mention of user will not generate notification for him.</param>
        public async Task<MessageSendResponse> SendAsync(long groupId, long peerId, int randomId, string message, double latitude, double longitude, List<string> attachment, string forward, int stickerId, string keyboard = null, string payload = null, bool dontParseLinks = false, bool disableMentions = false, MessageIntent intent = MessageIntent.None) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("random_id", randomId.ToString());
            if (!String.IsNullOrEmpty(message)) parameters.Add("message", message);
            if (latitude > 0) parameters.Add("lat", latitude.ToString().Replace(",", "."));
            if (longitude > 0) parameters.Add("long", longitude.ToString().Replace(",", "."));
            if (!attachment.IsNullOrEmpty()) parameters.Add("attachment", attachment.Combine());
            if (!String.IsNullOrEmpty(forward)) parameters.Add("forward", forward);
            if (stickerId > 0) parameters.Add("sticker_id", stickerId.ToString());
            if (!String.IsNullOrEmpty(keyboard)) parameters.Add("keyboard", keyboard); // TODO for bots: Parse keyboard object instead of string
            if (!String.IsNullOrEmpty(payload)) parameters.Add("payload", payload);
            if (dontParseLinks) parameters.Add("dont_parse_links", "1");
            if (disableMentions) parameters.Add("disable_mentions", "1");
            return await API.CallMethodAsync<MessageSendResponse>("messages.send", parameters);
        }

        /// <summary>Sends a reaction to message.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="cmId">ID of message in conv.</param>
        /// <param name="reactionId">Reaction ID.</param>
        public async Task<bool> SendReactionAsync(long groupId, long peerId, int cmId, int reactionId) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("cmid", cmId.ToString());
            parameters.Add("reaction_id", reactionId.ToString());
            return await API.CallMethodAsync<int>("messages.sendReaction", parameters) == 1;
        }

        /// <remarks>This method is undocumented!</remarks>
        /// <summary>Send event to bot.</summary>
        /// <param name="peerId">Destination ID.</param>
        /// <param name="paload">Payload from clicked bot-button.</param>
        /// <param name="messageId">Message ID of bot-keyboard owner (if keyboard is inline).</param>
        /// <param name="authorId">Bot-keyboard's author ID (if it is conversation keyboard).</param>
        public async Task<string> SendMessageEventAsync(long peerId, string payload, int messageId = 0, long authorId = 0) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "peer_id", peerId.ToString() }
            };
            if (messageId > 0) parameters.Add("message_id", messageId.ToString());
            if (authorId != 0) parameters.Add("author_id", authorId.ToString());
            parameters.Add("payload", payload.ToString());
            return await API.CallMethodAsync<string>("messages.sendMessageEvent", parameters);
        }

        /// <summary>Changes the status of a user as typing in a conversation.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="peerId">Destination ID.</param>
        /// <param name="type">Activity type (Typing — user has started to type, Audiomessage — user has started to record audiomessage).</param>
        public async Task<bool> SetActivityAsync(long groupId, long peerId, ActivityType type) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("type", type.ToEnumMemberAttribute());
            return await API.CallMethodAsync<bool>("messages.setActivity", parameters);
        }

        /// <summary>Sets a previously-uploaded picture as the cover picture of a chat.</summary>
        /// <param name="file">Upload URL from the response field returned by the Photos.GetChatUploadServerAsync method upon successfully uploading an image.</param>
        public async Task<SetChatPhotoResponse> SetChatPhotoAsync(string file) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("file", file);
            return await API.CallMethodAsync<SetChatPhotoResponse>("messages.setChatPhoto", parameters);
        }

        /// <remarks>This method is undocumented!</remarks>
        /// <summary>Set role for chat member.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="peerId">Peer ID.</param>
        /// <param name="memberId">Member ID.</param>
        /// <param name="role">Role (values: admin, member).</param>
        public async Task<bool> SetMemberRoleAsync(long groupId, long peerId, long memberId, string role) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("member_id", memberId.ToString());
            parameters.Add("role", role);
            return await API.CallMethodAsync<bool>("messages.setMemberRole", parameters);
        }

        /// <summary>Unpins a message.</summary>
        /// <param name="groupId">Group ID (for community messages with a user access token).</param>
        /// <param name="peerId">Destination ID.</param>
        public async Task<bool> UnpinAsync(long groupId, long peerId) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if(groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            return await API.CallMethodAsync<int>("messages.unpin", parameters) == 1;
        }
    }
}
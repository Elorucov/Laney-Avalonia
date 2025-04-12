using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class MessagesHistoryResponse {
        public MessagesHistoryResponse() { }

        [JsonPropertyName("items")]
        public List<Message> Items { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }

        [JsonPropertyName("conversations")]
        public List<Conversation> Conversations { get; set; }
    }

    //

    public class MessageReaction {
        [JsonPropertyName("reaction_id")]
        public int ReactionId { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("user_ids")]
        public List<long> UserIds { get; set; }
    }

    public class ReactedMember {
        [JsonPropertyName("reaction_id")]
        public int ReactionId { get; set; }

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }
    }

    public class GeoCoordinates {
        public GeoCoordinates() { }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }

    public class GeoPlace {
        public GeoPlace() { }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("created")]
        public int Created { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }
    }

    public class Geo {
        public Geo() { }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("coordinates")]
        public GeoCoordinates Coordinates { get; set; }

        [JsonPropertyName("place")]
        public GeoPlace Place { get; set; }
    }

    public class Action {
        public Action() { }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("member_id")]
        public long MemberId { get; set; }

        [JsonIgnore] // TODO: remove it.
        public long FromId { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("old_text")]
        public string OldText { get; set; }

        [JsonPropertyName("conversation_message_id")]
        public int ConversationMessageId { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("style")]
        public string Style { get; set; }
    }

    public class Message {
        static uint _instances;
        public static uint Instances => _instances;

        public Message() {
            _instances++;
        }

        ~Message() {
            _instances--;
        }

        [JsonIgnore]
        public DateTime DateTime { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }

        [JsonIgnore]
        public DateTime UpdateTime { get { return DateTimeOffset.FromUnixTimeSeconds(UpdateTimeUnix).DateTime.ToLocalTime(); } }

        //

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("conversation_message_id")]
        public int ConversationMessageId { get; set; }

        [JsonPropertyName("date")]
        public long DateUnix { get; set; }

        [JsonPropertyName("update_time")]
        public long UpdateTimeUnix { get; set; }

        [JsonPropertyName("peer_id")]
        public long PeerId { get; set; }

        [JsonPropertyName("from_id")]
        public long FromId { get; set; }

        [JsonPropertyName("admin_author_id")]
        public long AdminAuthorId { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("random_id")]
        public int RandomId { get; set; }

        [JsonPropertyName("attachments")]
        public List<Attachment> Attachments { get; set; }

        [JsonPropertyName("important")]
        public bool Important { get; set; }

        [JsonPropertyName("geo")]
        public Geo Geo { get; set; }

        [JsonPropertyName("payload")]
        public string PayLoad { get; set; }

        [JsonPropertyName("keyboard")]
        public BotKeyboard Keyboard { get; set; }

        [JsonPropertyName("fwd_messages")]
        public List<Message> ForwardedMessages { get; set; }

        [JsonPropertyName("reply_message")]
        public Message ReplyMessage { get; set; }

        [JsonPropertyName("action")]
        public Action Action { get; set; }

        [JsonPropertyName("template")]
        public BotTemplate Template { get; set; }

        [JsonPropertyName("expire_ttl")]
        public int ExpireTTL { get; set; }

        [JsonPropertyName("ttl")]
        public int TTL { get; set; }

        [JsonPropertyName("is_expired")]
        public bool IsExpired { get; set; }

        [JsonPropertyName("is_unavailable")]
        public bool IsUnavailable { get; set; }

        [JsonPropertyName("is_silent")]
        public bool IsSilent { get; set; }

        [JsonPropertyName("reaction_id")]
        public int ReactionId { get; set; }

        [JsonPropertyName("reactions")]
        public List<MessageReaction> Reactions { get; set; }

        [JsonIgnore]
        public bool IsPartial { get; private set; }

        [JsonIgnore]
        public List<long> MentionedUsers { get; private set; }

        public static Message BuildFromLP(JsonArray msg, long currentUserId, Func<long, bool> infoCached, out bool needToGetFullMsgFromAPI, out Exception exception) {
            exception = null;
            try {
                int cmId = (int)msg[1];
                int flags = (int)msg[2];
                int minor = (int)msg[3];
                long peer = (long)msg[4];
                int timestamp = (int)msg[5];
                string text = (string)msg[6];
                JsonObject additional = msg[7].AsObject();
                JsonObject attachments = msg[8].AsObject();
                int randomId = (int)msg[9];
                int id = (int)msg[10];
                int updateTimestamp = (int)msg[11];
                needToGetFullMsgFromAPI = false;

                bool outbound = (2 & flags) != 0;
                bool important = (8 & flags) != 0;

                Message message = new Message {
                    Id = id,
                    ConversationMessageId = cmId,
                    RandomId = randomId,
                    PeerId = peer,
                    DateUnix = timestamp,
                    UpdateTimeUnix = updateTimestamp,
                    Important = important
                };

                if (additional.ContainsKey("from")) {
                    // ¯\_(ツ)_/¯
                    message.FromId = Convert.ToInt64((string)additional["from"]);
                } else {
                    message.FromId = outbound ? currentUserId : peer;
                }

                bool senderInfoCached = (bool)(infoCached?.Invoke(message.FromId));
                if (!senderInfoCached) needToGetFullMsgFromAPI = true;

                if (!String.IsNullOrEmpty(text))
                    message.Text = text.Replace("<br>", "\n").Replace("&quot;", "\"").Replace("&amp;", "&")
                                       .Replace("&lt;", "<").Replace("&gt;", ">");

                if (additional.ContainsKey("payload")) message.PayLoad = additional["payload"].ToString();
                if (additional.ContainsKey("expire_ttl")) message.ExpireTTL = (int)additional["expire_ttl"];
                if (additional.ContainsKey("ttl")) message.ExpireTTL = (int)additional["ttl"];
                if (additional.ContainsKey("is_expired")) message.IsExpired = true;
                if (additional.ContainsKey("is_silent")) message.IsSilent = true;
                if (additional.ContainsKey("keyboard")) {
                    message.Keyboard = (BotKeyboard)additional["keyboard"].Deserialize(typeof(BotKeyboard), BuildInJsonContext.Default);
                    message.Keyboard.AuthorId = message.FromId;
                }
                if (additional.ContainsKey("marked_users")) {
                    var markedUsers = additional["marked_users"].AsArray();
                    foreach (var node in markedUsers) {
                        var markedroot = node.AsArray();
                        if ((int)markedroot[0] == 1 && markedroot.Count > 1) {
                            if (markedroot[1].GetValueKind() == JsonValueKind.String) { // all & online
                                message.MentionedUsers = new List<long>(); // пустой список — признак пуша всех
                            } else if (markedroot[1].AsArray() is JsonArray markedIds) {
                                message.MentionedUsers = new List<long>();
                                foreach (var mids in markedIds) {
                                    long mid = (long)mids;
                                    if (mid != 0) message.MentionedUsers.Add(mid);
                                }
                            }
                        }
                    }
                }

                if (attachments.Count > 0) {
                    if (attachments.ContainsKey("attach1_type") && attachments["attach1_type"].ToString() == "sticker") {
                        if (attachments.ContainsKey("attachments_count") && (string)attachments["attachments_count"] == "1") {
                            var parsedAtchs = (List<Attachment>)JsonSerializer.Deserialize((string)attachments["attachments"], typeof(List<Attachment>), BuildInJsonContext.Default);
                            if (parsedAtchs != null) message.Attachments = parsedAtchs;
                        } else {
                            needToGetFullMsgFromAPI = true;
                        }
                    } else {
                        needToGetFullMsgFromAPI = true;
                    }
                }
                if (additional.ContainsKey("source_act")) {
                    Action act = new Action {
                        Type = additional["source_act"].ToString()
                    };
                    act.FromId = message.FromId;
                    if (additional.ContainsKey("source_text")) act.Text = additional["source_text"].ToString();
                    if (additional.ContainsKey("source_old_text")) act.OldText = additional["source_old_text"].ToString();
                    if (additional.ContainsKey("source_mid")) act.MemberId = Int64.Parse(additional["source_mid"].GetValue<string>());
                    if (additional.ContainsKey("source_chat_local_id")) Int64.Parse(additional["source_chat_local_id"].GetValue<string>());
                    if (additional.ContainsKey("source_style")) act.Style = additional["source_style"].ToString();
                    if (act.Type == "chat_photo_update") needToGetFullMsgFromAPI = true;
                    message.Action = act;

                    bool memberInfoCached = (bool)(infoCached?.Invoke(act.MemberId));
                    if (!memberInfoCached) needToGetFullMsgFromAPI = true;
                }

                if (additional.ContainsKey("has_template")) needToGetFullMsgFromAPI = true;
                if (additional.ContainsKey("marked_users")) needToGetFullMsgFromAPI = true;

                message.IsPartial = needToGetFullMsgFromAPI;
                return message;
            } catch (Exception ex) {
                exception = ex;
                needToGetFullMsgFromAPI = true;
                return null;
            }
        }
    }
}

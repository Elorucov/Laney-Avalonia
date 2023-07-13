using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ELOR.VKAPILib.Objects {
    public class MessagesHistoryResponse {
        [JsonProperty("items")]
        public List<Message> Items { get; internal set; }

        [JsonProperty("count")]
        public int Count { get; internal set; }

        [JsonProperty("profiles")]
        public List<User> Profiles { get; internal set; }

        [JsonProperty("groups")]
        public List<Group> Groups { get; internal set; }

        [JsonProperty("conversations")]
        public List<Conversation> Conversations { get; internal set; }
    }

    //

    public class GeoCoordinates {
        [JsonProperty("latitude")]
        public double Latitude { get; internal set; }

        [JsonProperty("longitude")]
        public double Longitude { get; internal set; }
    }

    public class GeoPlace {
        [JsonProperty("id")]
        public int Id { get; internal set; }

        [JsonProperty("title")]
        public string Title { get; internal set; }

        [JsonProperty("latitude")]
        public double Latitude { get; internal set; }

        [JsonProperty("longitude")]
        public double Longitude { get; internal set; }

        [JsonProperty("created")]
        public int Created { get; internal set; }

        [JsonProperty("icon")]
        public string Icon { get; internal set; }

        [JsonProperty("country")]
        public string Country { get; internal set; }

        [JsonProperty("city")]
        public string City { get; internal set; }
    }

    public class Geo {
        [JsonProperty("type")]
        public string Type { get; internal set; }

        [JsonProperty("coordinates")]
        public GeoCoordinates Coordinates { get; internal set; }

        [JsonProperty("place")]
        public GeoPlace Place { get; internal set; }
    }

    public class Action {
        [JsonProperty("type")]
        public string Type { get; internal set; }

        [JsonProperty("member_id")]
        public long MemberId { get; internal set; }

        [JsonIgnore] // TODO: remove it.
        public long FromId { get; internal set; }

        [JsonProperty("text")]
        public string Text { get; internal set; }

        [JsonProperty("old_text")]
        public string OldText { get; internal set; }

        [JsonProperty("conversation_message_id")]
        public int ConversationMessageId { get; internal set; }

        [JsonProperty("message")]
        public string Message { get; internal set; }

        [JsonProperty("style")]
        public string Style { get; internal set; }
    }

    public class Message {
        [JsonIgnore]
        public DateTime DateTime { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }

        [JsonIgnore]
        public DateTime UpdateTime { get { return DateTimeOffset.FromUnixTimeSeconds(UpdateTimeUnix).DateTime.ToLocalTime(); } }

        //

        [JsonProperty("id")]
        public int Id { get; internal set; }

        [JsonProperty("conversation_message_id")]
        public int ConversationMessageId { get; internal set; }

        [JsonProperty("date")]
        public long DateUnix { get; internal set; }

        [JsonProperty("update_time")]
        public long UpdateTimeUnix { get; internal set; }

        [JsonProperty("peer_id")]
        public long PeerId { get; internal set; }

        [JsonProperty("from_id")]
        public long FromId { get; internal set; }

        [JsonProperty("admin_author_id")]
        public long AdminAuthorId { get; internal set; }

        [JsonProperty("text")]
        public string Text { get; internal set; }

        [JsonProperty("random_id")]
        public int RandomId { get; internal set; }

        [JsonProperty("attachments")]
        public List<Attachment> Attachments { get; internal set; }

        [JsonProperty("important")]
        public bool Important { get; internal set; }

        [JsonProperty("geo")]
        public Geo Geo { get; internal set; }

        [JsonProperty("payload")]
        public string PayLoad { get; internal set; }

        [JsonProperty("keyboard")]
        public BotKeyboard Keyboard { get; internal set; }

        [JsonProperty("fwd_messages")]
        public List<Message> ForwardedMessages { get; internal set; }

        [JsonProperty("reply_message")]
        public Message ReplyMessage { get; internal set; }

        [JsonProperty("action")]
        public Action Action { get; internal set; }

        [JsonProperty("template")]
        public BotTemplate Template { get; internal set; }

        [JsonProperty("expire_ttl")]
        public int ExpireTTL { get; internal set; }

        [JsonProperty("ttl")]
        public int TTL { get; internal set; }

        [JsonProperty("is_expired")]
        public bool IsExpired { get; internal set; }

        [JsonIgnore]
        public bool IsPartial { get; private set; }

        public static Message BuildFromLP(JArray msg, long currentUserId, Func<long, bool> infoCached, out bool needToGetFullMsgFromAPI, out Exception exception) {
            exception = null;
            try {
                int id = Convert.ToInt32(msg[1]);
                int flags = Convert.ToInt32(msg[2]);
                long peer = Convert.ToInt64(msg[3]);
                int timestamp = Convert.ToInt32(msg[4]);
                string text = (string)msg[5];
                JObject additional = msg[6].Value<JObject>();
                JObject attachments = msg[7].Value<JObject>();
                int randomId = Convert.ToInt32(msg[8]);
                int cmId = Convert.ToInt32(msg[9]);
                int updateTimestamp = Convert.ToInt32(msg[10]);
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
                    message.FromId = Convert.ToInt64(additional["from"].Value<string>());
                } else {
                    message.FromId = outbound ? currentUserId : peer;
                }

                bool senderInfoCached = (bool)(infoCached?.Invoke(message.FromId));
                if (!senderInfoCached) needToGetFullMsgFromAPI = true;

                if (!String.IsNullOrEmpty(text))
                    message.Text = text.Replace("<br>", "\n").Replace("&quot;", "\"").Replace("&amp;", "&")
                                       .Replace("&lt;", "<").Replace("&gt;", ">");

                if (additional.ContainsKey("payload")) message.PayLoad = additional["payload"].Value<string>();
                if (additional.ContainsKey("expire_ttl")) message.ExpireTTL = additional["expire_ttl"].Value<int>();
                if (additional.ContainsKey("ttl")) message.ExpireTTL = additional["ttl"].Value<int>();
                if (additional.ContainsKey("is_expired")) message.IsExpired = true;
                if (additional.ContainsKey("keyboard")) {
                    message.Keyboard = additional["keyboard"].ToObject<BotKeyboard>();
                    message.Keyboard.AuthorId = message.FromId;
                }

                if (attachments.Count > 0) {
                    if (attachments.ContainsKey("attach1_type") && attachments["attach1_type"].Value<string>() == "sticker") {
                        if (attachments.ContainsKey("attachments_count") && attachments["attachments_count"].Value<int>() == 1) {
                            var parsedAtchs = JsonConvert.DeserializeObject<List<Attachment>>(attachments["attachments"].Value<string>());
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
                        Type = additional["source_act"].Value<string>()
                    };
                    act.FromId = message.FromId;
                    if (additional.ContainsKey("source_text")) act.Text = additional["source_text"].Value<string>();
                    if (additional.ContainsKey("source_old_text")) act.OldText = additional["source_old_text"].Value<string>();
                    if (additional.ContainsKey("source_mid")) act.MemberId = additional["source_mid"].Value<int>();
                    if (additional.ContainsKey("source_chat_local_id")) act.ConversationMessageId = additional["source_chat_local_id"].Value<int>();
                    if (additional.ContainsKey("source_style")) act.Style = additional["source_style"].Value<string>();
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
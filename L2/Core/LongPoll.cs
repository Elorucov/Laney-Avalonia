using ELOR.Laney.Core.Network;
using ELOR.Laney.DataModels;
using ELOR.Laney.Execute.Objects;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib;
using ELOR.VKAPILib.Objects;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public enum LongPollReactionEventType {
        Unknown, IAdded, SomeoneAdded, IRemoved, SomeoneRemoved
    }

    public sealed class LongPoll {
        public const int WAIT_AFTER_FAIL = 3;
        public const int VERSION = 19;
        public const int WAIT_TIME = 25;
        public const int MODE = 234;

        private string _server;
        private string _key;
        private int _timeStamp;
        private int _pts;
        private bool _isRunning = false;
        private CancellationTokenSource _cts;

        private readonly bool _isOfficialClient = false;
        private readonly long _sessionId = 0;
        private readonly long _groupId = 0;
        private readonly VKAPI _api;
        private readonly Logger _log;

        private LongPollState _state;
        public LongPollState State { get { return _state; } private set { _state = value; StateChanged?.Invoke(this, value); } }

        public LongPoll(VKAPI api, long sessionId, long groupId, bool isOfficialClient = false) {
            _api = api;
            _sessionId = sessionId;
            _groupId = groupId;
            _isOfficialClient = isOfficialClient;

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Information();

            if (Settings.EnableLongPollLogs) {
                long lid = groupId == 0 ? sessionId : -groupId;
                loggerConfig = loggerConfig.WriteTo.File(Path.Combine(App.LocalDataPath, "logs", $"L2_LP_{lid}_.log"),
                    buffered: true, retainedFileCountLimit: 20, flushToDiskInterval: TimeSpan.FromSeconds(30)).MinimumLevel.Information();
            }

            _log = loggerConfig.CreateLogger();
            State = LongPollState.Connecting;
        }

        public void SetUp(LongPollServerInfo info) {
            _server = info.Server;
            _key = info.Key;
            _timeStamp = info.TS;
            _pts = info.PTS;
        }

        #region Events

        public delegate void MessageFlagsDelegate(LongPoll longPoll, int messageId, int flags, long peerId);
        public delegate void MessageReceivedDelegate(LongPoll longPoll, Message message, int flags, bool incrementUnreadCounter);
        public delegate void ConversationFlagsDelegate(LongPoll longPoll, long peerId, int flags);
        public delegate void MentionDelegate(LongPoll longPoll, long peerId, int messageId, bool isSelfDestruct);
        public delegate void ReadInfoDelegate(LongPoll longPoll, long peerId, int messageId, int count);
        public delegate void ConversationDataDelegate(LongPoll longPoll, int updateType, long peerId, long extra, Conversation? convo);
        public delegate void ActivityStatusDelegate(LongPoll longPoll, long peerId, List<LongPollActivityInfo> infos);
        public delegate void CanWriteChangedDelegate(LongPoll longPoll, long peerId, long memberId, bool isRestrictedToWrite, long until);
        public delegate void ReactionsChangedDelegate(LongPoll longPoll, long peerId, int cmId, LongPollReactionEventType type, int myReactionId, List<MessageReaction> reactions);
        public delegate void UnreadReactionsChangedDelegate(LongPoll longPoll, long peerId, List<int> cmIds);

        public event EventHandler NeedFullResync; // if getLongPollHistory failed.
        public event MessageFlagsDelegate MessageFlagSet; // 10002
        public event MessageFlagsDelegate MessageFlagRemove; // 10003
        public event MessageReceivedDelegate MessageReceived; // 10004
        public event MessageReceivedDelegate MessageEdited; // 10005, 10018
        public event MentionDelegate MentionReceived; // 10005, 10018
        public event ReadInfoDelegate IncomingMessagesRead; // 10006
        public event ReadInfoDelegate OutgoingMessagesRead; // 10007
        public event ConversationFlagsDelegate ConversationFlagReset; // 10
        public event ConversationFlagsDelegate ConversationFlagSet; // 12
        public event EventHandler<long> ConversationRemoved; // 10013 (решил упростить)
        public event ConversationFlagsDelegate MajorIdChanged; // 20
        public event ConversationFlagsDelegate MinorIdChanged; // 21
        public event ConversationDataDelegate ConversationDataChanged; // 52
        public event ActivityStatusDelegate ActivityStatusChanged; // 63-68
        public event EventHandler<int> UnreadCounterUpdated; // 80
        public event CanWriteChangedDelegate CanWriteChanged; // 91
        public event EventHandler<LongPollPushNotificationData> NotificationsSettingsChanged; // 114
        public event EventHandler<LongPollCallbackResponse> CallbackReceived; // 119
        public event ReactionsChangedDelegate ReactionsChanged; // 601
        public event UnreadReactionsChangedDelegate UnreadReactionsChanged; // 602

        public event EventHandler<LongPollState> StateChanged;

        #endregion

        public void Run() {
            _log.Information("Starting LongPoll...");
            _cts = new CancellationTokenSource();
            _isRunning = true;

            new System.Action(async () => await Task.Factory.StartNew(async () => {
                while (_isRunning) {
                    try {
                        _log.Information($"Waiting... TS: {_timeStamp}; PTS: {_pts}");
                        if (State == LongPollState.Updating || State == LongPollState.Connecting) State = LongPollState.Working;

                        Dictionary<string, string> parameters = new Dictionary<string, string> {
                            { "act", "a_check" },
                            { "key", _key },
                            { "ts", _timeStamp.ToString() },
                            { "wait", WAIT_TIME.ToString() },
                            { "mode", MODE.ToString() },
                            { "version", VERSION.ToString() }
                        };

                        HttpResponseMessage httpResponse = await LNet.PostAsync(new Uri($"https://{_server}"), parameters, cts: _cts).ConfigureAwait(false);
                        httpResponse.EnsureSuccessStatusCode();
                        var respstr = await httpResponse.Content.ReadAsStreamAsync();
                        JsonNode jr = await JsonNode.ParseAsync(respstr);
                        httpResponse.Dispose();
                        if (jr["updates"] != null) {
                            await ParseUpdatesAsync(jr["updates"].AsArray());
                            _timeStamp = (int)jr["ts"];
                            _pts = (int)jr["pts"];
                            State = LongPollState.Working;
                            await Task.Delay(1000).ConfigureAwait(false);
                        } else if (jr["failed"] != null) {
                            int errCode = (int)jr["failed"];
                            string errText = jr["error"].ToString();
                            _log.Error($"LongPoll error! Code {errCode}, message: {errText}. Restarting after {WAIT_AFTER_FAIL} sec...");

                            State = LongPollState.Failed;
                            await Task.Delay(WAIT_AFTER_FAIL * 1000).ConfigureAwait(false);
                            Restart();
                        } else {
                            throw new ArgumentException($"A non-standart response was received!\n{respstr}");
                        }
                    } catch (Exception ex) {
                        bool isConnectionLost = ExceptionHelper.IsExceptionAboutNetworkIssue(ex);
                        _log.Error(ex, $"Exception when parsing LongPoll events! Trying after {WAIT_AFTER_FAIL} sec.");

                        if (isConnectionLost) {
                            State = isConnectionLost ? LongPollState.NoInternet : LongPollState.Failed;
                            await Task.Delay(WAIT_AFTER_FAIL * 1000).ConfigureAwait(false);
                        } else {
                            Restart();
                        }
                    }
                }
            }))();
        }

        public void Stop() {
            _isRunning = false;
            _cts?.Cancel();
        }

        public void Restart() {
            Stop();

            new System.Action(async () => await Task.Factory.StartNew(async () => {
                bool trying = true;
                while (trying) {
                    try {
                        State = LongPollState.Updating;
                        _log.Information($"Getting LongPoll history... PTS: {_pts}, isOfficialClient={_isOfficialClient}");
                        var response = await _api.Messages.GetLongPollHistoryAsync(_groupId, VERSION, _timeStamp, _pts, 0, false, 1000, 1000, 0, VKAPIHelper.Fields, Constants.NestedMessagesLimit).ConfigureAwait(false);
                        CacheManager.Add(response.Profiles);
                        CacheManager.Add(response.Groups);

                        SetUp(response.Credentials);

                        // TODO: кешировать беседы.
                        await ParseUpdatesAsync(response.History, response.Messages.Items, response.Conversations);

                        if (response.More) _pts = response.NewPTS;
                        trying = response.More;
                    } catch (Exception ex) {
                        bool isConnectionLost = ExceptionHelper.IsExceptionAboutNetworkIssue(ex);
                        State = isConnectionLost ? LongPollState.NoInternet : LongPollState.Failed;
                        if (isConnectionLost) {
                            _log.Error(ex, $"Exception while getting LongPoll history! Trying after {WAIT_AFTER_FAIL} sec.");
                            await Task.Delay(WAIT_AFTER_FAIL * 1000).ConfigureAwait(false);
                        } else {
                            _log.Error(ex, $"Exception while getting LongPoll history! Required full resync. Has credentials: {_server != null}");
                            trying = false;
                            NeedFullResync?.Invoke(this, null);
                        }
                    }
                }
                if (_server != null) Run();
            }))();
        }

        // messages и convos — признак того, что метод вызывается после метода getLongPollHistory (если не null)
        private async Task ParseUpdatesAsync(JsonArray updates, List<Message> messages = null, List<Conversation> convos = null) {
            // peer id, CMID, is edited.
            List<Tuple<long, int, bool>> MessagesFromAPI = new List<Tuple<long, int, bool>>();
            Dictionary<int, int> MessagesFromAPIFlags = new Dictionary<int, int>();

            // TODO: для каждого события (кроме 10004, 10005 и 10018) создать отдельный метод.
            foreach (JsonArray u in updates) {
                _log.Information(string.Join(',', u));
                int eventId = (int)u[0];
                switch (eventId) {
                    case 10002:
                    case 10003:
                        int msgId = (int)u[1];
                        int flags = (int)u[2];
                        long peerId = (long)u[3];
                        bool hasMessage = u.Count > 4; // удалённое у текущего юзера сообщение восстановилось
                        _log.Information($"EVENT {eventId}: msg={msgId}, flags={flags}, peer={peerId}, hasMessage={hasMessage}");
                        if (eventId == 10003) MessageFlagRemove?.Invoke(this, msgId, flags, peerId);
                        else MessageFlagSet?.Invoke(this, msgId, flags, peerId);
                        if (hasMessage) {
                            Message msgFromHistory3 = messages?.SingleOrDefault(m => m.ConversationMessageId == msgId);
                            if (msgFromHistory3 != null) {
                                MessageReceived?.Invoke(this, msgFromHistory3, flags, false);
                                if (u.Count > 6) CheckMentions(u[6], msgFromHistory3.ConversationMessageId, peerId);
                            } else {
                                MessagesFromAPI.Add(new Tuple<long, int, bool>(peerId, msgId, false));
                                MessagesFromAPIFlags.Add(msgId, flags);
                            }
                        }
                        break;
                    case 10004:
                        int fieldsCountForDeleted = _isOfficialClient ? 5 : 4;
                        bool isDeletedBeforeEvent = u.Count == fieldsCountForDeleted && messages == null;
                        int receivedMsgId = (int)u[1];
                        int minor = 0;
                        long peerId4 = 0;
                        bool gLPH = messages != null;
                        if (gLPH) {
                            peerId4 = _isOfficialClient ? (long)u[4] : (long)u[3];
                        } else {
                            peerId4 = isDeletedBeforeEvent ? 0 : (long)u[4];
                            minor = (int)u[3];
                        }
                        _log.Information($"EVENT {eventId}: peer={peerId4}, msg={receivedMsgId}, isDeletedBeforeEvent={isDeletedBeforeEvent}, from getLongPollHistory={gLPH}");
                        if (isDeletedBeforeEvent) break;
                        Message msgFromHistory = messages?.SingleOrDefault(m => m.ConversationMessageId == receivedMsgId && m.PeerId == peerId4);
                        if (msgFromHistory != null) {
                            MessageReceived?.Invoke(this, msgFromHistory, (int)u[2], false);
                            minor = msgFromHistory.Id;
                            if (u.Count > 7) CheckMentions(u[7], receivedMsgId, peerId4);
                        } else {
                            bool isPartial = false;
                            Exception ex = null;
                            Message rmsg = Message.BuildFromLP(u, _sessionId, CheckIsCached, out isPartial, out ex);
                            if (ex == null && rmsg != null) {
                                MessageReceived?.Invoke(this, rmsg, (int)u[2], true);
                                minor = rmsg.Id;
                                if (u.Count > 7) CheckMentions(u[7], receivedMsgId, peerId4);
                                if (isPartial) {
                                    _log.Information($"Received message ({peerId4}_{receivedMsgId}) is partial. Added to queue for getting these from API.");
                                    MessagesFromAPI.Add(new Tuple<long, int, bool>(peerId4, receivedMsgId, true));
                                    MessagesFromAPIFlags.Add(receivedMsgId, (int)u[2]);
                                }
                            } else {
                                _log.Error(ex, $"An error occured while building message from LP! Message ID: {receivedMsgId}");
                                MessagesFromAPI.Add(new Tuple<long, int, bool>(peerId4, receivedMsgId, false));
                                MessagesFromAPIFlags.Add(receivedMsgId, (int)u[2]);
                            }
                        }
                        if (minor != 0) MinorIdChanged?.Invoke(this, peerId4, minor);
                        break;
                    case 10005:
                    case 10018:
                        bool isDeletedBeforeEvent2 = u.Count == 4 && messages == null;
                        int editedMsgId = (int)u[1];
                        long peerId5 = (long)u[3];
                        if (isDeletedBeforeEvent2) break;
                        Message editMsgFromHistory = messages?.SingleOrDefault(m => m.ConversationMessageId == editedMsgId && m.PeerId == peerId5);
                        _log.Information($"EVENT {eventId}: peer={peerId5}, msg={editedMsgId}, isDeletedBeforeEvent={isDeletedBeforeEvent2}");
                        if (editMsgFromHistory != null) {
                            MessageEdited?.Invoke(this, editMsgFromHistory, (int)u[2], false);
                            if (u.Count > 6) CheckMentions(u[6], editedMsgId, peerId5);
                        } else {
                            bool contains = MessagesFromAPI.Where(m => m.Item2 == editedMsgId).FirstOrDefault() != null;
                            if (!contains) {
                                MessagesFromAPI.Add(new Tuple<long, int, bool>(peerId5, editedMsgId, true));
                                MessagesFromAPIFlags.Add(editedMsgId, (int)u[2]);
                            }
                        }
                        break;
                    case 10006:
                    case 10007:
                        long peerId6 = (long)u[1];
                        int msgId6 = (int)u[2];
                        int count6 = 0;
                        if (convos != null) {
                            var convo = convos.SingleOrDefault(c => c.Peer.Id == peerId6);
                            if (convo != null) count6 = convo.UnreadCount;
                        } else {
                            count6 = u.Count > 3 ? (int)u[3] : 0;
                        }
                        _log.Information($"EVENT {eventId}: peer={peerId6}, msg={msgId6}, count={count6}");
                        if (eventId == 10006) {
                            IncomingMessagesRead?.Invoke(this, peerId6, msgId6, count6);
                        } else {
                            OutgoingMessagesRead?.Invoke(this, peerId6, msgId6, count6);
                        }
                        break;
                    case 10:
                    case 12:
                        long peerId10 = (long)u[1];
                        int flags10 = (int)u[2];
                        _log.Information($"EVENT {eventId}: peer={peerId10}, flags={flags10}");
                        if (eventId == 10) ConversationFlagReset?.Invoke(this, peerId10, flags10);
                        else ConversationFlagSet?.Invoke(this, peerId10, flags10);
                        break;
                    case 10013:
                        long peerId13 = (long)u[1];
                        int msgId13 = (int)u[2];
                        _log.Information($"EVENT {eventId}: peer={peerId13}, msg={msgId13}");
                        ConversationRemoved?.Invoke(this, peerId13);
                        break;
                    case 20:
                    case 21:
                        long peerId20 = (long)u[1];
                        int sortId = (int)u[2];
                        _log.Information($"EVENT {eventId}: peer={peerId20}, major/minor={sortId}");
                        if (eventId == 20) MajorIdChanged?.Invoke(this, peerId20, sortId);
                        else MinorIdChanged?.Invoke(this, peerId20, sortId);
                        break;
                    case 52:
                        int updateType = (int)u[1];
                        long peerId52 = (long)u[2];
                        long extra = (long)u[3];
                        _log.Information($"EVENT {eventId}: updateType={updateType}, peer={peerId52}, extra={extra}");
                        Conversation convo52 = convos?.SingleOrDefault(c => c.Peer.Id == peerId52);
                        ConversationDataChanged?.Invoke(this, updateType, peerId52, extra, convo52);
                        break;
                    case 63:
                    case 64:
                    case 65:
                    case 66:
                    case 67:
                    case 68:
                    case 69:
                    case 70:
                        LongPollActivityType type = GetLPActivityType(eventId);
                        long peerId63 = (long)u[1];
                        long[] userIds = (long[])u[2].Deserialize(typeof(long[]), L2JsonSerializerContext.Default);
                        int totalCount = (int)u[3];
                        _log.Information($"EVENT {eventId}: peer={peerId63}, users={String.Join(", ", userIds)}, count={totalCount}");

                        List<LongPollActivityInfo> acts = new List<LongPollActivityInfo>();
                        foreach (var userId in userIds) {
                            acts.Add(new LongPollActivityInfo(userId, type));
                        }

                        ActivityStatusChanged?.Invoke(this, peerId63, acts);
                        break;
                    case 80:
                        int unreadCount = (int)u[1];
                        _log.Information($"EVENT {eventId}: count={unreadCount}");
                        UnreadCounterUpdated?.Invoke(this, unreadCount);
                        break;
                    case 91:
                        int restrictionType = (int)u[1];
                        long peerId91 = (long)u[2];
                        long memberId = (long)u[3];
                        bool isDeny = restrictionType == 1 || restrictionType == 2;
                        long until = restrictionType == 1 ? (long)u[5] : 0;
                        _log.Information($"EVENT {eventId}: peer={peerId91}, memberId={memberId}, isDeny={isDeny}, until={until}");
                        CanWriteChanged?.Invoke(this, peerId91, memberId, isDeny, until);
                        break;
                    case 114:
                        var data = (LongPollPushNotificationData)u[1].Deserialize(typeof(LongPollPushNotificationData), L2JsonSerializerContext.Default);
                        _log.Information($"EVENT {eventId}: peer={data.PeerId}, sound={data.Sound}, disabledUntil={data.DisabledUntil}");
                        NotificationsSettingsChanged?.Invoke(this, data);
                        break;
                    case 119:
                        var cbData = (LongPollCallbackResponse)u[1].Deserialize(typeof(LongPollCallbackResponse), L2JsonSerializerContext.Default);
                        _log.Information($"EVENT {eventId}: peer={cbData.PeerId}, owner={cbData.OwnerId}, event={cbData.EventId} action={cbData.Action?.Type}");
                        CallbackReceived?.Invoke(this, cbData);
                        break;
                    case 601:
                        _log.Information($"EVENT {eventId}: length: {u.Count}");
                        ParseReactionsAndInvoke(u.Select(n => (long)n.Deserialize(typeof(long), L2JsonSerializerContext.Default)).ToArray());
                        break;
                    case 602:
                        _log.Information($"EVENT {eventId}: length: {u.Count}");
                        ParseUnreadReactionsAndInvoke(u.Select(n => (long)n.Deserialize(typeof(long), L2JsonSerializerContext.Default)).ToArray());
                        break;
                }
            }

            if (MessagesFromAPI.Count > 0) {
                await GetMessagesFromAPIAsync(MessagesFromAPI, MessagesFromAPIFlags);
            }

            await Task.Delay(16);
        }

        private void ParseReactionsAndInvoke(long[] u) {
            LongPollReactionEventType type = (LongPollReactionEventType)u[1];
            long myReaction = 0;
            long peerId = u[2];
            long cmId = u[3];

            int start = 4;
            if (type == LongPollReactionEventType.IAdded) {
                myReaction = u[4];
                start = 5;
            }

            long reactionsCount = u[start];
            long reactionStart = start + 1;
            List<MessageReaction> reactions = new List<MessageReaction>();

            for (long i = 0; i < reactionsCount; i++) {
                Tuple<long, MessageReaction> b = ParseReaction(u, reactionStart);
                reactionStart = b.Item1;
                reactions.Add(b.Item2);
            }
            ReactionsChanged?.Invoke(this, peerId, Convert.ToInt32(cmId), type, Convert.ToInt32(myReaction), reactions);
        }

        private Tuple<long, MessageReaction> ParseReaction(long[] u, long start) {
            long dataCount = u[start];
            long dataStart = start + 1;
            long reactionId = u[start + 1];
            long count = u[start + 2];
            long end = u[start + 3];

            List<long> members = new List<long>();
            for (int j = 0; j < end; j++) {
                members.Add(u[start + 4 + j]);
            }
            return new Tuple<long, MessageReaction>(dataStart + dataCount, new MessageReaction {
                ReactionId = Convert.ToInt32(reactionId),
                Count = Convert.ToInt32(count),
                UserIds = members
            });
        }

        private void ParseUnreadReactionsAndInvoke(long[] u) {
            long peerId = u[1];
            long cmidsCount = u[2];
            List<int> cmIds = new List<int>();
            if (cmidsCount > 0)
                foreach (var cmid in u.Skip(3)) {
                    cmIds.Add((int)cmid);
                }
            UnreadReactionsChanged?.Invoke(this, peerId, cmIds);
        }

        private void CheckMentions(JsonNode additional, int messageId, long peerId) {
            try {
                if (additional["marked_users"] == null) return;
                JsonArray t = additional["marked_users"].AsArray();
                if (t != null) {
                    foreach (JsonArray o in (JsonArray)t) {
                        int flag = Int32.Parse(o[0].ToString());
                        bool isBomb = flag == 2;

                        if (o[1].ToString() == "all") {
                            MentionReceived?.Invoke(this, peerId, messageId, isBomb);
                        } else {
                            JsonArray u1 = o[1].AsArray();
                            if (Int64.Parse(u1.FirstOrDefault().ToString()) == _sessionId) MentionReceived?.Invoke(this, peerId, messageId, isBomb);
                        }
                    }
                }
            } catch (Exception ex) {
                _log.Error(ex, $"Cannot check mentions, \"marked_users\" parsing error!");
            }
        }

        private LongPollActivityType GetLPActivityType(int eventId) {
            switch (eventId) {
                default: return LongPollActivityType.Typing;
                case 64: return LongPollActivityType.RecordingAudioMessage;
                case 65: return LongPollActivityType.UploadingPhoto;
                case 66: return LongPollActivityType.UploadingVideo;
                case 67: return LongPollActivityType.UploadingFile;
                case 68: return LongPollActivityType.UploadingVideoMessage;
                case 69: return LongPollActivityType.ChoosingFile;
                case 70: return LongPollActivityType.ChoosingTemplate;
            }
        }

        private async Task GetMessagesFromAPIAsync(List<Tuple<long, int, bool>> messagesFromAPI, Dictionary<int, int> flags) {
            string debugPairs = "N/A";
            try {
                List<KeyValuePair<long, int>> peerMessagePair = new List<KeyValuePair<long, int>>();
                Dictionary<string, bool> isEditedCMIDs = new Dictionary<string, bool>();

                foreach (var pair in messagesFromAPI) {
                    peerMessagePair.Add(new KeyValuePair<long, int>(pair.Item1, pair.Item2));
                    isEditedCMIDs.Add($"{pair.Item1}_{pair.Item2}", pair.Item3);
                }
                debugPairs = String.Join(", ", isEditedCMIDs.Keys.ToList());
                _log.Information($"Need to get this messages from API: {debugPairs}.");

                MessagesList response = await _api.Messages.GetByIdAsync(_groupId, peerMessagePair, 0, true, VKAPIHelper.Fields);
                if (response.Items.Count > 0) {
                    CacheManager.Add(response.Profiles);
                    CacheManager.Add(response.Groups);
                    foreach (Message msg in response.Items) {
                        int flag = flags[msg.ConversationMessageId];
                        bool isEdited = isEditedCMIDs[$"{msg.PeerId}_{msg.ConversationMessageId}"];
                        _log.Information($"Successfully received message ({msg.PeerId}_{msg.ConversationMessageId}) from API. Is edited: {isEdited}");
                        if (isEdited) {
                            MessageEdited?.Invoke(this, msg, flag, false);
                        } else {
                            MessageReceived?.Invoke(this, msg, flag, true);
                        }
                    }
                }
            } catch (Exception ex) {
                _log.Error(ex, $"An error occured while getting messages from API! Messages: {debugPairs}: {ex.Message}");
            }
        }

        private bool CheckIsCached(long id) {
            bool isCached = CacheManager.GetUser(id) != null || CacheManager.GetGroup(id) != null;
            _log.Information($"Is sender name for {id} exist in cache: {isCached}");
            return isCached;
        }

        #region Debug

        public void DebugFireDeleteConvoEvent(long peerId) {
            ConversationRemoved?.Invoke(this, peerId);
        }

        public void DebugInvalidateLPKey() {
            _key = string.Empty;
        }

        #endregion
    }
}
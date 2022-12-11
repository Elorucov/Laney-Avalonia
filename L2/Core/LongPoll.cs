using ELOR.Laney.Core.Network;
using ELOR.Laney.DataModels;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib;
using ELOR.VKAPILib.Objects;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public sealed class LongPoll {
        public const int WAIT_AFTER_FAIL = 3;
        public const int VERSION = 11;
        public const int WAIT_TIME = 25;
        public const int MODE = 234;

        private string Server;
        private string Key;
        private int TimeStamp;
        private int PTS;

        private int sessionId = 0;
        private int groupId = 0;
        private VKAPI API;
        private CancellationTokenSource cts;
        private bool isRunning = false;
        private Logger Log;

        public LongPoll(LongPollServerInfo info, VKAPI api, int sessionId, int groupId) {
            Server = info.Server;
            Key = info.Key;
            TimeStamp = info.TS;
            PTS = info.PTS;

            API = api;
            this.sessionId = sessionId;
            this.groupId = groupId;

            // TODO: настройка в UI для включения/отключения логирования LP.
            Log = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(Path.Combine(App.LocalDataPath, "logs", "L2_LP_.log"), rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        #region Events

        public delegate void MessageFlagsDelegate(LongPoll longPoll, int messageId, int flags, int peerId);
        public delegate void MessageReceivedDelegate(LongPoll longPoll, Message message, int flags);
        public delegate void ConversationFlagsDelegate(LongPoll longPoll, int peerId, int flags);
        public delegate void MentionDelegate(LongPoll longPoll, int peerId, int messageId, bool isSelfDestruct);
        public delegate void ReadInfoDelegate(LongPoll longPoll, int peerId, int messageId, int count);
        public delegate void ConversationDataDelegate(LongPoll longPoll, int updateType, int peerId, int extra);
        public delegate void ActivityStatusDelegate(LongPoll longPoll, int peerId, List<LongPollActivityInfo> infos);

        public event MessageFlagsDelegate MessageFlagSet; // 2
        public event MessageFlagsDelegate MessageFlagRemove; // 3
        public event MessageReceivedDelegate MessageReceived; // 4
        public event MessageReceivedDelegate MessageEdited; // 5, 18
        public event MentionDelegate MentionReceived; // 5, 18
        public event ReadInfoDelegate IncomingMessagesRead; // 6
        public event ReadInfoDelegate OutgoingMessagesRead; // 7
        public event ConversationFlagsDelegate ConversationFlagReset; // 10
        public event ConversationFlagsDelegate ConversationFlagSet; // 12
        public event EventHandler<int> ConversationRemoved; // 13 (решил упростить)
        public event ConversationFlagsDelegate MajorIdChanged; // 20
        public event ConversationFlagsDelegate MinorIdChanged; // 21
        public event ConversationDataDelegate ConversationDataChanged; // 52
        public event ActivityStatusDelegate ActivityStatusChanged; // 63-67
        public event EventHandler<int> UnreadCounterUpdated; // 80
        public event EventHandler<LongPollPushNotificationData> NotificationsSettingsChanged; // 114
        public event EventHandler<LongPollCallbackResponse> CallbackReceived; // 119

        public event EventHandler<LongPollState> StateChanged;

        #endregion

        public async void Run() {
            Log.Information("Starting LongPoll...");
            StateChanged?.Invoke(this, LongPollState.Connecting);
            cts = new CancellationTokenSource();
            isRunning = true;
            while (isRunning) {
                try {
                    Log.Information($"Waiting... TS: {TimeStamp}; PTS: {PTS}");
                    StateChanged?.Invoke(this, LongPollState.Working);

                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("act", "a_check");
                    parameters.Add("key", Key);
                    parameters.Add("ts", TimeStamp.ToString());
                    parameters.Add("wait", WAIT_TIME.ToString());
                    parameters.Add("mode", MODE.ToString());
                    parameters.Add("version", VERSION.ToString());

                    HttpResponseMessage httpResponse = await LNet.PostAsync(new Uri($"https://{Server}"), parameters, cts: cts).ConfigureAwait(false);
                    httpResponse.EnsureSuccessStatusCode();
                    string respstr = await httpResponse.Content.ReadAsStringAsync();
                    JObject jr = JObject.Parse(respstr);
                    httpResponse.Dispose();
                    if (jr["updates"] != null) {
                        TimeStamp = jr["ts"].Value<int>();
                        PTS = jr["pts"].Value<int>();
                        ParseUpdates(jr["updates"].Value<JArray>());
                        await Task.Delay(1000).ConfigureAwait(false);
                    } else if (jr["failed"] != null) {
                        int errCode = jr["failed"].Value<int>();
                        string errText = jr["error"].Value<string>();
                        Log.Error($"LongPoll error! Code {errCode}, message: {errText}. Restarting after {WAIT_AFTER_FAIL} sec...");
                        if (errCode != 1) {
                            StateChanged?.Invoke(this, LongPollState.Failed);
                            await Task.Delay(WAIT_AFTER_FAIL * 1000).ConfigureAwait(false);
                            Restart();
                        }
                    } else {
                        throw new ArgumentException($"A non-standart response was received!\n{respstr}");
                    }
                } catch (Exception ex) {
                    Log.Error(ex, $"Exception when parsing LongPoll events! Trying after {WAIT_AFTER_FAIL} sec.");
                    StateChanged?.Invoke(this, LongPollState.Failed);
                    await Task.Delay(WAIT_AFTER_FAIL * 1000).ConfigureAwait(false);
                }
            }
        }

        public void Stop() {
            isRunning = false;
            cts?.Cancel();
        }

        public async void Restart() {
            Stop();
            StateChanged?.Invoke(this, LongPollState.Updating);
            bool trying = true;
            while (trying) {
                try {
                    Log.Information("Getting LongPoll history...");
                    var response = await API.Messages.GetLongPollHistoryAsync(groupId, TimeStamp, PTS, 0, false, 1000, 500, 0, VKAPIHelper.Fields).ConfigureAwait(false);
                    CacheManager.Add(response.Profiles);
                    CacheManager.Add(response.Groups);
                    // TODO: кешировать беседы.
                    ParseUpdates(response.History, response.Messages.Items);

                    Server = response.Credentials.Server;
                    TimeStamp = response.Credentials.TS;
                    PTS = response.Credentials.PTS;

                    if (response.More) PTS = response.NewPTS;
                    trying = response.More;
                } catch (Exception ex) {
                    Log.Error(ex, $"Exception while getting LongPoll history! Trying after {WAIT_AFTER_FAIL} sec.");
                    await Task.Delay(WAIT_AFTER_FAIL * 1000).ConfigureAwait(false);
                }
            }
            Run();
        }

        private void ParseUpdates(JArray updates, List<Message> messages = null) {
            // Message id, is edited.
            Dictionary<int, bool> MessagesFromAPI = new Dictionary<int, bool>();
            Dictionary<int, int> MessagesFromAPIFlags = new Dictionary<int, int>();

            foreach (JArray u in updates) {
                int eventId = u[0].Value<int>();
                switch (eventId) {
                    case 2:
                    case 3:
                        int msgId = u[1].Value<int>();
                        int flags = u[2].Value<int>();
                        int peerId = u[3].Value<int>();
                        bool hasMessage = u.Count > 4; // удалённое у текущего юзера сообщение восстановилось
                        Log.Information($"EVENT {eventId}: msg={msgId}, flags={flags}, peer={peerId}, hasMessage={hasMessage}");
                        if (eventId == 3) MessageFlagRemove?.Invoke(this, msgId, flags, peerId);
                        else MessageFlagSet?.Invoke(this, msgId, flags, peerId);
                        if (hasMessage) {
                            Message msgFromHistory3 = messages?.Where(m => m.Id == msgId).FirstOrDefault();
                            if (msgFromHistory3 != null) {
                                MessageReceived?.Invoke(this, msgFromHistory3, flags);
                                if (u.Count > 6) CheckMentions(u[6], msgId, u[3].Value<int>());
                            } else {
                                bool isPartial = false;
                                Exception ex = null;
                                Message rmsg = Message.BuildFromLP(u, sessionId, CheckIsCached, out isPartial, out ex);
                                if (ex == null && rmsg != null) {
                                    MessageReceived?.Invoke(this, rmsg, u[2].Value<int>());
                                    if (u.Count > 6) CheckMentions(u[6], msgId, u[3].Value<int>());
                                    if (isPartial) {
                                        MessagesFromAPI.Add(msgId, true);
                                        MessagesFromAPIFlags.Add(msgId, u[2].Value<int>());
                                    }
                                } else {
                                    Log.Error(ex, $"An error occured while building message from LP! Message ID: {u[1].Value<int>()}");
                                    MessagesFromAPI.Add(msgId, false);
                                    MessagesFromAPIFlags.Add(msgId, u[2].Value<int>());
                                }
                            }
                        }
                        break;
                    case 4:
                        int receivedMsgId = u[1].Value<int>();
                        Log.Information($"EVENT {eventId}: msg={receivedMsgId}");
                        Message msgFromHistory = messages?.Where(m => m.Id == receivedMsgId).FirstOrDefault();
                        if (msgFromHistory != null) {
                            MessageReceived?.Invoke(this, msgFromHistory, u[2].Value<int>());
                            if (u.Count > 6) CheckMentions(u[6], receivedMsgId, u[3].Value<int>());
                        } else {
                            bool isPartial = false;
                            Exception ex = null;
                            Message rmsg = Message.BuildFromLP(u, sessionId, CheckIsCached, out isPartial, out ex);
                            if (ex == null && rmsg != null) {
                                MessageReceived?.Invoke(this, rmsg, u[2].Value<int>());
                                if (u.Count > 6) CheckMentions(u[6], receivedMsgId, u[3].Value<int>());
                                if (isPartial) {
                                    MessagesFromAPI.Add(receivedMsgId, true);
                                    MessagesFromAPIFlags.Add(receivedMsgId, u[2].Value<int>());
                                }
                            } else {
                                Log.Error(ex, $"An error occured while building message from LP! Message ID: {u[1].Value<int>()}");
                                MessagesFromAPI.Add(receivedMsgId, false);
                                MessagesFromAPIFlags.Add(receivedMsgId, u[2].Value<int>());
                            }
                        }
                        break;
                    case 5:
                    case 18:
                        int editedMsgId = u[1].Value<int>();
                        Message editMsgFromHistory = messages?.Where(m => m.Id == editedMsgId).FirstOrDefault();
                        Log.Information($"EVENT {eventId}: msg={editedMsgId}");
                        if (editMsgFromHistory != null) {
                            MessageEdited?.Invoke(this, editMsgFromHistory, u[2].Value<int>());
                            if (u.Count > 6) CheckMentions(u[6], editedMsgId, u[3].Value<int>());
                        } else {
                            if (!MessagesFromAPI.ContainsKey(editedMsgId)) {
                                MessagesFromAPI.Add(editedMsgId, true);
                                MessagesFromAPIFlags.Add(editedMsgId, u[2].Value<int>());
                            }
                        }
                        break;
                    case 6:
                        int peerId6 = u[1].Value<int>();
                        int msgId6 = u[2].Value<int>();
                        int count6 = u[3].Value<int>();
                        Log.Information($"EVENT {eventId}: peer={peerId6}, msg={msgId6}, count={count6}");
                        IncomingMessagesRead?.Invoke(this, peerId6, msgId6, count6);
                        break;
                    case 7:
                        int peerId7 = u[1].Value<int>();
                        int msgId7 = u[2].Value<int>();
                        int count7 = u[3].Value<int>();
                        Log.Information($"EVENT {eventId}: peer={peerId7}, msg={msgId7}, count={count7}");
                        OutgoingMessagesRead?.Invoke(this, peerId7, msgId7, count7);
                        break;
                    case 10:
                    case 12:
                        int peerId10 = u[1].Value<int>();
                        int flags10 = u[2].Value<int>();
                        Log.Information($"EVENT {eventId}: peer={peerId10}, flags={flags10}");
                        if (eventId == 10) ConversationFlagReset?.Invoke(this, peerId10, flags10);
                        else ConversationFlagSet?.Invoke(this, peerId10, flags10);
                        break;
                    case 13:
                        int peerId13 = u[1].Value<int>();
                        int msgId13 = u[2].Value<int>();
                        Log.Information($"EVENT {eventId}: peer={peerId13}, msg={msgId13}");
                        ConversationRemoved?.Invoke(this, peerId13);
                        break;
                    case 20:
                        int peerId20 = u[1].Value<int>();
                        int sortId = u[2].Value<int>();
                        Log.Information($"EVENT {eventId}: peer={peerId20}, major/minor={sortId}");
                        if (eventId == 20) MajorIdChanged?.Invoke(this, peerId20, sortId);
                        else MinorIdChanged?.Invoke(this, peerId20, sortId);
                        break;
                    case 52:
                        int updateType = u[1].Value<int>();
                        int peerId52 = u[2].Value<int>();
                        int extra = u[3].Value<int>();
                        Log.Information($"EVENT {eventId}: updateType={updateType}, peer={peerId52}, extra={extra}");
                        ConversationDataChanged?.Invoke(this, updateType, peerId52, extra);
                        break;
                    case 63:
                    case 64:
                    case 65:
                    case 66:
                    case 67:
                        LongPollActivityType type = GetLPActivityType(eventId);
                        int peerId63 = u[1].Value<int>();
                        int[] userIds = u[2].Value<JArray>().ToObject<int[]>();
                        int totalCount = u[3].Value<int>();
                        Log.Information($"EVENT {eventId}: peer={peerId63}, users={String.Join(", ", userIds)}, count={totalCount}");

                        List<LongPollActivityInfo> acts = new List<LongPollActivityInfo>();
                        foreach (var userId in userIds) {
                            acts.Add(new LongPollActivityInfo(userId, type));
                        }

                        ActivityStatusChanged?.Invoke(this, peerId63, acts);
                        break;
                    case 80:
                        int unreadCount = u[1].Value<int>();
                        Log.Information($"EVENT {eventId}: count={unreadCount}");
                        UnreadCounterUpdated?.Invoke(this, unreadCount);
                        break;
                    case 114:
                        LongPollPushNotificationData data = u[1].ToObject<LongPollPushNotificationData>();
                        Log.Information($"EVENT {eventId}: peer={data.PeerId}, sound={data.Sound}, disabledUntil={data.DisabledUntil}");
                        NotificationsSettingsChanged?.Invoke(this, data);
                        break;
                    case 119:
                        LongPollCallbackResponse cbData = u[1].ToObject<LongPollCallbackResponse>();
                        Log.Information($"EVENT {eventId}: peer={cbData.PeerId}, owner={cbData.OwnerId}, event={cbData.EventId} action={cbData.Action?.Type}");
                        CallbackReceived?.Invoke(this, cbData);
                        break;
                }
            }

            if (MessagesFromAPI.Count > 0) {
                GetMessagesFromAPI(MessagesFromAPI, MessagesFromAPIFlags);
            }
        }

        private void CheckMentions(JToken additional, int messageId, int peerId) {
            try { 
                JToken t = additional["marked_users"];
                if (t != null) {
                    foreach (JArray o in (JArray)t) {
                        int flag = Int32.Parse(o[0].ToString());
                        bool isBomb = flag == 2;

                        if (o[1].ToString() == "all") {
                            MentionReceived?.Invoke(this, peerId, messageId, isBomb);
                        } else {
                            JArray u1 = (JArray)o[1];
                            if (Int32.Parse(u1.First.ToString()) == sessionId) MentionReceived?.Invoke(this, peerId, messageId, isBomb);
                        }
                    }
                }
            } catch (Exception ex) {
                Log.Error(ex, $"Cannot check mentions, \"marked_users\" parsing error!");
            }
        }

        private LongPollActivityType GetLPActivityType(int eventId) {
            switch (eventId) {
                default: return LongPollActivityType.Typing;
                case 64: return LongPollActivityType.RecordingAudioMessage;
                case 65: return LongPollActivityType.UploadingPhoto;
                case 66: return LongPollActivityType.UploadingVideo;
                case 67: return LongPollActivityType.UploadingFile;
            }
        }

        private async void GetMessagesFromAPI(Dictionary<int, bool> messagesFromAPI, Dictionary<int, int> flags) {
            try {
                var msgIds = messagesFromAPI.Keys.ToList();
                Log.Information($"Need to get this messages from API: {String.Join(", ", msgIds)}.");
                var response = await API.Messages.GetByIdAsync(groupId, msgIds, 0, true, VKAPIHelper.Fields);
                if (response.Count > 0) {
                    CacheManager.Add(response.Profiles);
                    CacheManager.Add(response.Groups);
                    foreach (Message msg in response.Items) {
                        int flag = flags[msg.Id];
                        if (messagesFromAPI[msg.Id]) {
                            MessageEdited?.Invoke(this, msg, flag);
                        } else {
                            MessageReceived?.Invoke(this, msg, flag);
                        }
                    }
                }
            } catch (Exception ex) {
                Log.Error(ex, $"An error occured while getting messages from API! Messages: {String.Join(", ", messagesFromAPI.Keys.ToList())}.");
            }
        }

        private bool CheckIsCached(int id) {
            return CacheManager.GetUser(id) != null || CacheManager.GetGroup(id) != null;
        }
    }
}
﻿using ELOR.Laney.Core.Network;
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
    public sealed class LongPoll {
        public const int WAIT_AFTER_FAIL = 3;
        public const int VERSION = 19;
        public const int WAIT_TIME = 25;
        public const int MODE = 234;

        private string Server;
        private string Key;
        private int TimeStamp;
        private int PTS;

        private long sessionId = 0;
        private long groupId = 0;
        private VKAPI API;
        private CancellationTokenSource cts;
        private bool isRunning = false;
        private Logger Log;

        public LongPoll(VKAPI api, long sessionId, long groupId) {
            API = api;
            this.sessionId = sessionId;
            this.groupId = groupId;

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Information();

            if (Settings.EnableLongPollLogs)
                loggerConfig = loggerConfig.WriteTo.File(Path.Combine(App.LocalDataPath, "logs", "L2_LP_.log"), rollingInterval: RollingInterval.Hour, retainedFileCountLimit: 10);

            Log = loggerConfig.CreateLogger();
        }

        public void SetUp(LongPollServerInfo info) {
            // Server = info.Server;
            Server = info.Server.Replace("im.vk.com/nim", "api.vk.com/ruim"); // TODO: параметр в настройках как в старом Laney
            Key = info.Key;
            TimeStamp = info.TS;
            PTS = info.PTS;
        }

        #region Events

        public delegate void MessageFlagsDelegate(LongPoll longPoll, int messageId, int flags, long peerId);
        public delegate void MessageReceivedDelegate(LongPoll longPoll, Message message, int flags);
        public delegate void ConversationFlagsDelegate(LongPoll longPoll, long peerId, int flags);
        public delegate void MentionDelegate(LongPoll longPoll, long peerId, int messageId, bool isSelfDestruct);
        public delegate void ReadInfoDelegate(LongPoll longPoll, long peerId, int messageId, int count);
        public delegate void ConversationDataDelegate(LongPoll longPoll, int updateType, long peerId, long extra);
        public delegate void ActivityStatusDelegate(LongPoll longPoll, long peerId, List<LongPollActivityInfo> infos);

        public event MessageFlagsDelegate MessageFlagSet; // 2
        public event MessageFlagsDelegate MessageFlagRemove; // 3
        public event MessageReceivedDelegate MessageReceived; // 4
        public event MessageReceivedDelegate MessageEdited; // 5, 18
        public event MentionDelegate MentionReceived; // 5, 18
        public event ReadInfoDelegate IncomingMessagesRead; // 6
        public event ReadInfoDelegate OutgoingMessagesRead; // 7
        public event ConversationFlagsDelegate ConversationFlagReset; // 10
        public event ConversationFlagsDelegate ConversationFlagSet; // 12
        public event EventHandler<long> ConversationRemoved; // 13 (решил упростить)
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

                    Dictionary<string, string> parameters = new Dictionary<string, string> {
                        { "act", "a_check" },
                        { "key", Key },
                        { "ts", TimeStamp.ToString() },
                        { "wait", WAIT_TIME.ToString() },
                        { "mode", MODE.ToString() },
                        { "version", VERSION.ToString() }
                    };

                    HttpResponseMessage httpResponse = await LNet.PostAsync(new Uri($"https://{Server}"), parameters, cts: cts).ConfigureAwait(false);
                    httpResponse.EnsureSuccessStatusCode();
                    string respstr = await httpResponse.Content.ReadAsStringAsync();
                    JsonNode jr = JsonNode.Parse(respstr);
                    httpResponse.Dispose();
                    if (jr["updates"] != null) {
                        TimeStamp = (int)jr["ts"];
                        PTS = (int)jr["pts"];
                        ParseUpdates(jr["updates"].AsArray());
                        await Task.Delay(1000).ConfigureAwait(false);
                    } else if (jr["failed"] != null) {
                        int errCode = (int)jr["failed"];
                        string errText = jr["error"].ToString();
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

                    SetUp(response.Credentials);

                    if (response.More) PTS = response.NewPTS;
                    trying = response.More;
                } catch (Exception ex) {
                    Log.Error(ex, $"Exception while getting LongPoll history! Trying after {WAIT_AFTER_FAIL} sec.");
                    await Task.Delay(WAIT_AFTER_FAIL * 1000).ConfigureAwait(false);
                }
            }
            Run();
        }

        private void ParseUpdates(JsonArray updates, List<Message> messages = null) {
            // peer id, CMID, is edited.
            List<Tuple<long, int, bool>> MessagesFromAPI = new List<Tuple<long, int, bool>>();
            Dictionary<int, int> MessagesFromAPIFlags = new Dictionary<int, int>();

            // TODO: для каждого события (кроме 10004, 10005 и 10018) создать отдельный метод.
            foreach (JsonArray u in updates) {
                int eventId = (int)u[0];
                switch (eventId) {
                    case 10002:
                    case 10003:
                        int msgId = (int)u[1];
                        int flags = (int)u[2];
                        long peerId = (long)u[3];
                        bool hasMessage = u.Count > 4; // удалённое у текущего юзера сообщение восстановилось
                        Log.Information($"EVENT {eventId}: msg={msgId}, flags={flags}, peer={peerId}, hasMessage={hasMessage}");
                        if (eventId == 3) MessageFlagRemove?.Invoke(this, msgId, flags, peerId);
                        else MessageFlagSet?.Invoke(this, msgId, flags, peerId);
                        if (hasMessage) {
                            Message msgFromHistory3 = messages?.Where(m => m.Id == msgId).FirstOrDefault();
                            if (msgFromHistory3 != null) {
                                MessageReceived?.Invoke(this, msgFromHistory3, flags);
                                if (u.Count > 6) CheckMentions(u[6], msgId, (long)u[3]);
                            } else {
                                MessagesFromAPI.Add(new Tuple<long, int, bool>(peerId, msgId, false));
                                MessagesFromAPIFlags.Add(msgId, (int)u[2]);
                            }
                        }
                        break;
                    case 10004:
                        int receivedMsgId = (int)u[1];
                        int minor = (int)u[3];
                        long peerId4 = (long)u[4];
                        Log.Information($"EVENT {eventId}: peer={peerId4}, msg={receivedMsgId}");
                        Message msgFromHistory = messages?.Where(m => m.ConversationMessageId == receivedMsgId).FirstOrDefault();
                        if (msgFromHistory != null) {
                            MessageReceived?.Invoke(this, msgFromHistory, (int)u[2]);
                            if (u.Count > 7) CheckMentions(u[7], receivedMsgId, peerId4);
                        } else {
                            bool isPartial = false;
                            Exception ex = null;
                            Message rmsg = Message.BuildFromLP(u, sessionId, CheckIsCached, out isPartial, out ex);
                            if (ex == null && rmsg != null) {
                                MessageReceived?.Invoke(this, rmsg, (int)u[2]);
                                if (u.Count > 6) CheckMentions(u[7], receivedMsgId, peerId4);
                                if (isPartial) {
                                    MessagesFromAPI.Add(new Tuple<long, int, bool>(peerId4, receivedMsgId, true));
                                    MessagesFromAPIFlags.Add(receivedMsgId, (int)u[2]);
                                }
                            } else {
                                Log.Error(ex, $"An error occured while building message from LP! Message ID: {(int)u[1]}");
                                MessagesFromAPI.Add(new Tuple<long, int, bool>(peerId4, receivedMsgId, false));
                                MessagesFromAPIFlags.Add(receivedMsgId, (int)u[2]);
                            }
                        }
                        MinorIdChanged?.Invoke(this, peerId4, minor);
                        break;
                    case 10005:
                    case 10018:
                        int editedMsgId = (int)u[1];
                        long peerId5 = (long)u[3];
                        Message editMsgFromHistory = messages?.Where(m => m.ConversationMessageId == editedMsgId).FirstOrDefault();
                        Log.Information($"EVENT {eventId}: peer={peerId5}, msg={editedMsgId}");
                        if (editMsgFromHistory != null) {
                            MessageEdited?.Invoke(this, editMsgFromHistory, (int)u[2]);
                            if (u.Count > 6) CheckMentions(u[6], editedMsgId, peerId5);
                        } else {
                            bool contains = MessagesFromAPI.Where(m => m.Item2 == editedMsgId).FirstOrDefault() != null;
                            if (!contains) {
                                MessagesFromAPI.Add(new Tuple<long, int, bool>(peerId5, editedMsgId, false));
                                MessagesFromAPIFlags.Add(editedMsgId, (int)u[2]);
                            }
                        }
                        break;
                    case 10006:
                        long peerId6 = (long)u[1];
                        int msgId6 = (int)u[2];
                        int count6 = u.Count > 3 ? (int)u[3] : 0;
                        Log.Information($"EVENT {eventId}: peer={peerId6}, msg={msgId6}, count={count6}");
                        IncomingMessagesRead?.Invoke(this, peerId6, msgId6, count6);
                        break;
                    case 10007:
                        long peerId7 = (long)u[1];
                        int msgId7 = (int)u[2];
                        int count7 = u.Count > 3 ? (int)u[3] : 0;
                        Log.Information($"EVENT {eventId}: peer={peerId7}, msg={msgId7}, count={count7}");
                        OutgoingMessagesRead?.Invoke(this, peerId7, msgId7, count7);
                        break;
                    case 10:
                    case 12:
                        long peerId10 = (long)u[1];
                        int flags10 = (int)u[2];
                        Log.Information($"EVENT {eventId}: peer={peerId10}, flags={flags10}");
                        if (eventId == 10) ConversationFlagReset?.Invoke(this, peerId10, flags10);
                        else ConversationFlagSet?.Invoke(this, peerId10, flags10);
                        break;
                    case 10013:
                        long peerId13 = (long)u[1];
                        int msgId13 = (int)u[2];
                        Log.Information($"EVENT {eventId}: peer={peerId13}, msg={msgId13}");
                        ConversationRemoved?.Invoke(this, peerId13);
                        break;
                    case 20:
					case 21:
                        long peerId20 = (long)u[1];
                        int sortId = (int)u[2];
                        Log.Information($"EVENT {eventId}: peer={peerId20}, major/minor={sortId}");
                        if (eventId == 20) MajorIdChanged?.Invoke(this, peerId20, sortId);
                        else MinorIdChanged?.Invoke(this, peerId20, sortId);
                        break;
                    case 52:
                        int updateType = (int)u[1];
                        long peerId52 = (long)u[2];
                        int extra = (int)u[3];
                        Log.Information($"EVENT {eventId}: updateType={updateType}, peer={peerId52}, extra={extra}");
                        ConversationDataChanged?.Invoke(this, updateType, peerId52, extra);
                        break;
                    case 63:
                    case 64:
                    case 65:
                    case 66:
                    case 67:
                        LongPollActivityType type = GetLPActivityType(eventId);
                        long peerId63 = (long)u[1];
                        long[] userIds = (long[])u[2].Deserialize(typeof(long[]), L2JsonSerializerContext.Default);
                        int totalCount = (int)u[3];
                        Log.Information($"EVENT {eventId}: peer={peerId63}, users={String.Join(", ", userIds)}, count={totalCount}");

                        List<LongPollActivityInfo> acts = new List<LongPollActivityInfo>();
                        foreach (var userId in userIds) {
                            acts.Add(new LongPollActivityInfo(userId, type));
                        }

                        ActivityStatusChanged?.Invoke(this, peerId63, acts);
                        break;
                    case 80:
                        int unreadCount = (int)u[1];
                        Log.Information($"EVENT {eventId}: count={unreadCount}");
                        UnreadCounterUpdated?.Invoke(this, unreadCount);
                        break;
                    case 114:
                        var data = (LongPollPushNotificationData)u[1].Deserialize(typeof(LongPollPushNotificationData), L2JsonSerializerContext.Default);
                        Log.Information($"EVENT {eventId}: peer={data.PeerId}, sound={data.Sound}, disabledUntil={data.DisabledUntil}");
                        NotificationsSettingsChanged?.Invoke(this, data);
                        break;
                    case 119:
                        var cbData = (LongPollCallbackResponse)u[1].Deserialize(typeof(LongPollCallbackResponse), L2JsonSerializerContext.Default);
                        Log.Information($"EVENT {eventId}: peer={cbData.PeerId}, owner={cbData.OwnerId}, event={cbData.EventId} action={cbData.Action?.Type}");
                        CallbackReceived?.Invoke(this, cbData);
                        break;
                }
            }

            if (MessagesFromAPI.Count > 0) {
                GetMessagesFromAPI(MessagesFromAPI, MessagesFromAPIFlags);
            }
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
                            if (Int64.Parse(u1.FirstOrDefault().ToString()) == sessionId) MentionReceived?.Invoke(this, peerId, messageId, isBomb);
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

        private async void GetMessagesFromAPI(List<Tuple<long, int, bool>> messagesFromAPI, Dictionary<int, int> flags) {
            string debugPairs = "N/A";
            try {
                List<KeyValuePair<long, int>> peerMessagePair = new List<KeyValuePair<long, int>>();
                Dictionary<string, bool> isEditedCMIDs = new Dictionary<string, bool>();

                foreach (var pair in messagesFromAPI) {
                    peerMessagePair.Add(new KeyValuePair<long, int>(pair.Item1, pair.Item2));
                    isEditedCMIDs.Add($"{pair.Item1}_{pair.Item2}", pair.Item3);
                }
                debugPairs = String.Join(", ", isEditedCMIDs.Keys.ToList());
                Log.Information($"Need to get this messages from API: {debugPairs}.");

                string code = VKAPIHelper.BuildCodeForGetMessagesByCMID(groupId, peerMessagePair, VKAPIHelper.Fields);

                MessagesList response = await API.CallMethodAsync<MessagesList>("execute", new Dictionary<string, string> {
                    { "code", code }
                });
                if (response.Items.Count > 0) {
                    CacheManager.Add(response.Profiles);
                    CacheManager.Add(response.Groups);
                    foreach (Message msg in response.Items) {
                        int flag = flags[msg.ConversationMessageId];
                        if (isEditedCMIDs[$"{msg.PeerId}_{msg.ConversationMessageId}"]) {
                            MessageEdited?.Invoke(this, msg, flag);
                        } else {
                            MessageReceived?.Invoke(this, msg, flag);
                        }
                    }
                }
            } catch (Exception ex) {
                Log.Error(ex, $"An error occured while getting messages from API! Messages: {debugPairs}.");
            }
        }

        private bool CheckIsCached(long id) {
            return CacheManager.GetUser(id) != null || CacheManager.GetGroup(id) != null;
        }
    }
}
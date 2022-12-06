using ELOR.Laney.Core.Network;
using ELOR.Laney.DataModels;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib;
using ELOR.VKAPILib.Objects;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public sealed class LongPoll {
        public const int VERSION = 11;
        public const int WAIT_TIME = 25;
        public const int MODE = 234;

        private string Server;
        private string Key;
        private int TimeStamp;
        private int PTS;

        private int groupId = 0;
        private VKAPI API;
        private CancellationTokenSource cts;
        private bool isRunning = false;
        private Logger Log;

        public LongPoll(LongPollServerInfo info, VKAPI api, int groupId) {
            Server = info.Server;
            Key = info.Key;
            TimeStamp = info.TS;
            PTS = info.PTS;

            API = api;
            this.groupId = groupId;

            Log = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(Path.Combine(App.LocalDataPath, "logs", "L2_LP_.log"), rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        #region Events

        public delegate void MessageFlagsDelegate(LongPoll longPoll, int messageId, int flags, int peerId);
        public delegate void ConversationFlagsDelegate(LongPoll longPoll, int peerId, int flags);
        public delegate void ReadInfoDelegate(LongPoll longPoll, int peerId, int messageId, int count);
        public delegate void ConversationDataDelegate(LongPoll longPoll, int updateType, int peerId, int extra);
        public delegate void ActivityStatusDelegate(LongPoll longPoll, LongPollActivityType type, int peerId, int[] userIds, int totalCount);

        public event MessageFlagsDelegate MessageFlagSet; // 2
        public event MessageFlagsDelegate MessageFlagRemove; // 3
        public event EventHandler<Message> MessageReceived; // 4
        public event EventHandler<Message> MessageEdited; // 5, 18
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

        #endregion

        public async void Run() {
            Log.Information("Starting LongPoll...");
            cts = new CancellationTokenSource();
            isRunning = true;
            while (isRunning) {
                try {
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("act", "a_check");
                    parameters.Add("key", Key);
                    parameters.Add("ts", TimeStamp.ToString());
                    parameters.Add("wait", WAIT_TIME.ToString());
                    parameters.Add("mode", MODE.ToString());
                    parameters.Add("version", VERSION.ToString());

                    HttpResponseMessage httpResponse = await LNet.PostAsync(new Uri($"https://{Server}"), parameters, cts: cts).ConfigureAwait(false);
                    httpResponse.EnsureSuccessStatusCode();

                    Log.Information($"Waiting... TS: {TimeStamp}");
                    string respstr = await httpResponse.Content.ReadAsStringAsync();
                    JObject jr = JObject.Parse(respstr);
                    httpResponse.Dispose();
                    if (jr["updates"] != null) {
                        TimeStamp = jr["ts"].Value<int>();
                        PTS = jr["pts"].Value<int>();
                        ParseUpdates(jr["updates"].Value<JArray>());
                    } else if (jr["failed"] != null) {
                        int errCode = jr["failed"].Value<int>();
                        string errText = jr["error"].Value<string>();
                        Log.Error($"LongPoll error! Code {errCode}, message: {errText}. Restarting...");
                        await Task.Delay(2000).ConfigureAwait(false);
                        Restart();
                    } else {
                        throw new ArgumentException($"A non-standart response was received!\n{respstr}");
                    }
                } catch (Exception ex) {
                    Log.Error(ex, "Exception when parsing LongPoll events! Trying after 3 sec.");
                    await Task.Delay(3000).ConfigureAwait(false);
                }
            }
        }

        public void Stop() {
            isRunning = false;
            cts?.Cancel();
        }

        public async void Restart() {
            Stop();
            bool trying = true;
            while (trying) {
                try {
                    Log.Information("Getting LongPoll history...");
                    var response = await API.Messages.GetLongPollHistoryAsync(groupId, TimeStamp, PTS, 0, false, 1000, 500, 0, VKAPIHelper.Fields).ConfigureAwait(false);
                    PTS = response.NewPTS;
                    CacheManager.Add(response.Profiles);
                    CacheManager.Add(response.Groups);
                    ParseUpdates(response.History);

                    Server = response.Credentials.Server;
                    TimeStamp = response.Credentials.TS;
                    PTS = response.Credentials.PTS;

                    trying = response.More;
                } catch (Exception ex) {
                    Log.Error(ex, "Exception while getting LongPoll history! Trying after 3 sec.");
                    await Task.Delay(3000).ConfigureAwait(false);
                }
            }
            Run();
        }

        private void ParseUpdates(JArray updates) {
            foreach (JArray u in updates) {
                int eventId = u[0].Value<int>();
                switch (eventId) {
                    case 2:
                    case 3:
                        int msgId = u[1].Value<int>();
                        int flags = u[2].Value<int>();
                        int peerId = u[3].Value<int>();
                        if (eventId == 3) MessageFlagRemove?.Invoke(this, msgId, flags, peerId);
                        else MessageFlagSet?.Invoke(this, msgId, flags, peerId);
                        break;
                }
            }
        }
    }
}
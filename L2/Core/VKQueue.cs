using ELOR.Laney.Core.Network;
using ELOR.Laney.DataModels.VKQueue;
using ELOR.Laney.Execute.Objects;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public class VKQueue {
        #region Variables

        static bool IsCycleEnabled = false;
        const int WaitTime = 25;
        const int WAIT_AFTER_FAIL = 3;

        static string Server;
        static string Key;
        static string TimeStamp;

        public static bool IsInitialized { get; private set; }
        static CancellationTokenSource cts;

        #endregion

        #region Events

        public static event EventHandler<OnlineEvent> Online;

        #endregion

        public static async void Init(QueueSubscribeResponse config) {
            if (config == null) return;

            Server = config.BaseUrl;
            Key = config.Queues[0].Key;
            TimeStamp = config.Queues[0].Timestamp.ToString();
            Log.Information($"Initializing queue. BaseUrl: {config.BaseUrl}, TS: {config.Queues[0].Timestamp}");

            cts = new CancellationTokenSource();
            IsCycleEnabled = true;
            await Task.Factory.StartNew(Run);
        }

        public static void Stop() {
            // Log(LogImportance.Info, "Stopping...");
            Log.Information($"Stopping queue.");
            IsCycleEnabled = false;
            cts?.Cancel();
        }

        private static async void Run() {
            while (IsCycleEnabled) {
                Log.Verbose($"Queue: waiting {TimeStamp}...");
                try {
                    Dictionary<string, string> parameters = new Dictionary<string, string> {
                        { "act", "a_check" },
                        { "key", Key },
                        { "ts", TimeStamp },
                        { "id", VKSession.Main.Id.ToString() },
                        { "wait", WaitTime.ToString() }
                    };

                    HttpResponseMessage httpResponse = await LNet.PostAsync(new Uri(Server), parameters, cts: cts).ConfigureAwait(false);
                    string respstr = await httpResponse.Content.ReadAsStringAsync();
                    httpResponse.Dispose();

                    var response = JsonNode.Parse(respstr).AsObject();
                    if (response.ContainsKey("failed")) {
                        int failed = response!["failed"].GetValue<int>();
                        int err = response!["err"].GetValue<int>();
                        throw new Exception($"Got error from queue, code {failed}, err: {err}.");
                    } else if (response.ContainsKey("events")) {
                        TimeStamp = response!["ts"].GetValue<string>();
                        var events = response!["events"].AsArray();

                        foreach (var qevent in events) {
                            string etype = qevent!["entity_type"].GetValue<string>();
                            if (etype == "online") ParseOnlineEvent(qevent!["data"].AsObject());
                        }
                    } else {
                        throw new Exception($"A non-standart response was received!\n{respstr}");
                    }
                } catch (Exception ex) {
                    Log.Error(ex, $"Exception when parsing Queue events! TS: {TimeStamp}...");
                    await Task.Delay(WAIT_AFTER_FAIL * 1000).ConfigureAwait(false);
                }
            }
        }

        private static void ParseOnlineEvent(JsonObject data) {
            OnlineEvent oe = (OnlineEvent)data.Deserialize(typeof(OnlineEvent), L2JsonSerializerContext.Default);
            Online?.Invoke(null, oe);
        }
    }
}
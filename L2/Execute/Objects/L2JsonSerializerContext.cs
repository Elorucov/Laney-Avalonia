using ELOR.Laney.DataModels;
using ELOR.Laney.DataModels.VKQueue;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ELOR.Laney.Execute.Objects {

    [JsonSerializable(typeof(AlbumLite))]
    [JsonSerializable(typeof(List<AlbumLite>))]
    [JsonSerializable(typeof(StartSessionResponse))]
    [JsonSerializable(typeof(MessagesHistoryEx))]
    [JsonSerializable(typeof(StickerPickerData))]
    [JsonSerializable(typeof(UserEx))]
    [JsonSerializable(typeof(GroupEx))]
    [JsonSerializable(typeof(ChatInfoEx))]
    [JsonSerializable(typeof(DemoModeData))]
    [JsonSerializable(typeof(long[]))]
    [JsonSerializable(typeof(LongPollPushNotificationData))]
    [JsonSerializable(typeof(LongPollCallbackResponse))]
    [JsonSerializable(typeof(OnlineEvent))]
    public partial class L2JsonSerializerContext : JsonSerializerContext {
    }
}
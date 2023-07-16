using ELOR.VKAPILib.Objects;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib {
    [JsonSerializable(typeof(APIException))]
    [JsonSerializable(typeof(App))]
    [JsonSerializable(typeof(AppsList))]
    [JsonSerializable(typeof(ConversationsResponse))]
    internal partial class BuildInJsonContext : JsonSerializerContext { }
}
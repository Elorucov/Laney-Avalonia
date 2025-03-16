using ELOR.VKAPILib.Attributes;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public enum ScreenNameType {
        [EnumMember(Value = "user")]
        User,

        [EnumMember(Value = "group")]
        Group,

        [EnumMember(Value = "application")]
        Application,

        [EnumMember(Value = "vk_app")]
        VkApp,

        [EnumMember(Value = "community_application")]
        CommunityApplication,

        [EnumMember(Value = "internal_vkui")]
        InternalVKUI
    }

    public class ResolveScreenNameResult {
        public ResolveScreenNameResult() { }

        [JsonPropertyName("object_id")]
        public long ObjectId { get; set; }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<ScreenNameType>))]
        public ScreenNameType Type { get; set; }
    }
}
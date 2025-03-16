using ELOR.VKAPILib.Attributes;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public enum BotButtonColor {
        [EnumMember(Value = "secondary")]
        Default,

        [EnumMember(Value = "primary")]
        Primary,

        [EnumMember(Value = "positive")]
        Positive,

        [EnumMember(Value = "negative")]
        Negative
    }

    public enum BotButtonType {
        Unknown,

        [EnumMember(Value = "text")]
        Text,

        [EnumMember(Value = "vkpay")]
        VKPay,

        [EnumMember(Value = "location")]
        Location,

        [EnumMember(Value = "callback")]
        Callback,

        [EnumMember(Value = "open_app")]
        OpenApp,

        [EnumMember(Value = "open_link")]
        OpenLink,

        [EnumMember(Value = "open_photo")]
        OpenPhoto
    }

    public class BotButtonAction {
        public BotButtonAction() { }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<BotButtonType>))]
        public BotButtonType Type { get; set; }

        [JsonPropertyName("payload")]
        public string Payload { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("app_id")]
        public int AppId { get; set; }

        [JsonPropertyName("owner_id")]
        public long OwnerId { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonIgnore]
        public Uri LinkUri { get { return new Uri(Link); } }
    }

    public class BotButton {
        public BotButton() { }

        [JsonPropertyName("color")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<BotButtonColor>))]
        public BotButtonColor Color { get; set; }

        [JsonPropertyName("action")]
        public BotButtonAction Action { get; set; }
    }

    public class BotKeyboard {
        public BotKeyboard() { }

        [JsonPropertyName("one_time")]
        public bool OneTime { get; set; }

        [JsonPropertyName("author_id")]
        public long AuthorId { get; set; }

        [JsonPropertyName("inline")]
        public bool Inline { get; set; }

        [JsonPropertyName("buttons")]
        public List<List<BotButton>> Buttons { get; set; }
    }
}

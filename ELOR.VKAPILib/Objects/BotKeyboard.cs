using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects {
    [DataContract]
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

    [DataContract]
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
        [JsonProperty("type")]
        public BotButtonType Type { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("app_id")]
        public int AppId { get; set; }

        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonIgnore]
        public Uri LinkUri { get { return new Uri(Link); } }
    }

    public class BotButton {
        [JsonProperty("color")]
        public BotButtonColor Color { get; set; }

        [JsonProperty("action")]
        public BotButtonAction Action { get; set; }
    }

    public class BotKeyboard {
        [JsonProperty("one_time")]
        public bool OneTime { get; set; }

        [JsonProperty("author_id")]
        public int AuthorId { get; set; }

        [JsonProperty("inline")]
        public bool Inline { get; set; }

        [JsonProperty("buttons")]
        public List<List<BotButton>> Buttons { get; set; }
    }
}

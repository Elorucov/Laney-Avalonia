using ELOR.VKAPILib.Attributes;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public enum BotTemplateType {
        Unknown,

        [EnumMember(Value = "carousel")]
        Carousel,
    }

    public class BotTemplate {
        public BotTemplate() { }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<BotTemplateType>))]
        public BotTemplateType Type { get; set; }

        [JsonPropertyName("elements")]
        public List<CarouselElement> Elements { get; set; }
    }
}
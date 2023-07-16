using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using ELOR.VKAPILib.Attributes;

namespace ELOR.VKAPILib.Objects {
    public enum BotTemplateType {
        Unknown,

        [EnumMember(Value = "carousel")]
        Carousel,
    }

    public class BotTemplate {
        public BotTemplate() {}
            
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<BotTemplateType>))]
        public BotTemplateType Type { get; set; }

        [JsonPropertyName("elements")]
        public List<CarouselElement> Elements { get; set; }
    }
}
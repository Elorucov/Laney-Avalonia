using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace ELOR.VKAPILib.Objects {
    [DataContract]
    public enum BotTemplateType {
        Unknown,

        [EnumMember(Value = "carousel")]
        Carousel,
    }

    public class BotTemplate {
        [JsonProperty("type")]
        public BotTemplateType Type { get; set; }

        [JsonProperty("elements")]
        public List<CarouselElement> Elements { get; set; }
    }
}
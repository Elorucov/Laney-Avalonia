using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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

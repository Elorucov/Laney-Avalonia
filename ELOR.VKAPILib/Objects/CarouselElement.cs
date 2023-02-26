using Newtonsoft.Json;

namespace ELOR.VKAPILib.Objects {
    public class CarouselElement {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("action")]
        public BotButtonAction Action { get; set; }

        [JsonProperty("photo")]
        public Photo Photo { get; set; }

        [JsonProperty("buttons")]
        public List<BotButton> Buttons { get; set; }
    }
}
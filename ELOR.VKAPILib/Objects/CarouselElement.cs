using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class CarouselElement {
        public CarouselElement() { }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("action")]
        public BotButtonAction Action { get; set; }

        [JsonPropertyName("photo")]
        public Photo Photo { get; set; }

        [JsonPropertyName("buttons")]
        public List<BotButton> Buttons { get; set; }
    }
}
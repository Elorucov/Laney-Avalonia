using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class MarketPrice {
        public MarketPrice() {}

        [JsonPropertyName("amount")]
        public string Amount { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class MarketCategory {
        public MarketCategory() {}

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class Market : AttachmentBase {
        public Market() {}

        [JsonIgnore]
        public override string ObjectType { get { return "market"; } }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("price")]
        public MarketPrice Price { get; set; }

        [JsonPropertyName("thumb_photo")]
        public string ThumbPhoto { get; set; }

        [JsonPropertyName("date")]
        public int DateUnix { get; set; }

        [JsonPropertyName("availability")]
        public int Availability { get; set; }
    }
}
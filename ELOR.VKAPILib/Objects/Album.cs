using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {

    public class Album {
        public Album() { }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("owner_id")]
        public long OwnerId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }
}
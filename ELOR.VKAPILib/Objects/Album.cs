using Newtonsoft.Json;

namespace ELOR.VKAPILib.Objects {
    public class Album {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("owner_id")]
        public long OwnerId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
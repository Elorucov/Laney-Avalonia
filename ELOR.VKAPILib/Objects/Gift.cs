using Newtonsoft.Json;

namespace ELOR.VKAPILib.Objects
{
    public class Gift
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("thumb_256")]
        public string Thumb { get; set; }

        [JsonIgnore]
        public Uri ThumbUri { get { return new Uri(Thumb); } }
    }
}
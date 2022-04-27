using Newtonsoft.Json;

namespace ELOR.VKAPILib.Objects {
    public class App {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

}
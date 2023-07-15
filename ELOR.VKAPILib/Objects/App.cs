using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class App {
        public App() {}
        
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }

}
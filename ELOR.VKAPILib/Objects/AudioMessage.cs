using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects
{
    public class AudioMessage : AttachmentBase {
        public AudioMessage() {}
            
        [JsonIgnore]
        public override string ObjectType { get { return "doc"; } }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("waveform")]
        public int[] WaveForm { get; set; }

        [JsonPropertyName("link_mp3")]
        public string Link { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return new Uri(Link); } }

        [JsonPropertyName("transcript")]
        public string Transcript { get; set; }
    }
}
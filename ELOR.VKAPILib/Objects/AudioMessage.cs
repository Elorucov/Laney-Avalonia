using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects
{
    public class AudioMessage : AttachmentBase {
        [JsonIgnore]
        public override string ObjectType { get { return "doc"; } }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonIgnore]
        public TimeSpan DurationTime { get { return TimeSpan.FromSeconds(Duration); } }

        [JsonProperty("waveform")]
        public int[] WaveForm { get; set; }

        [JsonProperty("link_mp3")]
        public string Link { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return new Uri(Link); } }

        [JsonProperty("transcript")]
        public string Transcript { get; set; }
    }
}

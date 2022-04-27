using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects
{
    public class Audio : AttachmentBase {
        [JsonIgnore]
        public override string ObjectType { get { return "audio"; } }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonIgnore]
        public TimeSpan DurationTime { get { return TimeSpan.FromSeconds(Duration); } }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }

        [JsonIgnore]
        public string FullSongName { get { return String.IsNullOrEmpty(Subtitle) ? Title : $"{Title} ({Subtitle})"; } }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return new Uri(Url); } }

        [JsonProperty("date")]
        public int DateUnix { get; set; }

        [JsonIgnore]
        public DateTime Date { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }
    }
}

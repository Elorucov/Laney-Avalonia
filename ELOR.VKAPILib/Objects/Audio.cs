using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class Audio : AttachmentBase {
        public Audio() { }

        [JsonIgnore]
        public override string ObjectType { get { return "audio"; } }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("subtitle")]
        public string Subtitle { get; set; }

        [JsonIgnore]
        public string FullSongName { get { return String.IsNullOrEmpty(Subtitle) ? Title : $"{Title} ({Subtitle})"; } }

        [JsonPropertyName("artist")]
        public string Artist { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get { return Uri.IsWellFormedUriString(Url, UriKind.Absolute) ? new Uri(Url) : null; } }

        [JsonPropertyName("date")]
        public int DateUnix { get; set; }

        [JsonIgnore]
        public DateTime Date { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }
    }
}
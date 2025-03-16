using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class VideoFiles {
        public VideoFiles() { }

        [JsonPropertyName("external")]
        public string External { get; set; }

        [JsonPropertyName("mp4_240")]
        public string MP4p240 { get; set; }

        [JsonPropertyName("mp4_360")]
        public string MP4p360 { get; set; }

        [JsonPropertyName("mp4_480")]
        public string MP4p480 { get; set; }

        [JsonPropertyName("mp4_720")]
        public string MP4p720 { get; set; }

        [JsonPropertyName("mp4_1080")]
        public string MP4p1080 { get; set; }
    }

    public class Video : AttachmentBase, IPreview {
        public Video() { }

        [JsonIgnore]
        public override string ObjectType { get { return "video"; } }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonIgnore]
        public TimeSpan DurationTime { get { return TimeSpan.FromSeconds(Duration); } }

        [JsonPropertyName("image")]
        public List<PhotoSizes> Image { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("date")]
        public int DateUnix { get; set; }

        [JsonIgnore]
        public DateTime Date { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }

        [JsonPropertyName("adding_date")]
        public int AddingDateUnix { get; set; }

        [JsonIgnore]
        public DateTime AddingDate { get { return DateTimeOffset.FromUnixTimeSeconds(AddingDateUnix).DateTime.ToLocalTime(); } }

        [JsonPropertyName("files")]
        public VideoFiles Files { get; set; }

        [JsonPropertyName("player")]
        public string Player { get; set; }

        [JsonIgnore]
        public Uri PlayerUri { get { return new Uri(Player); } }

        [JsonPropertyName("first_frame")]
        public List<PhotoSizes> FirstFrame { get; set; }

        [JsonIgnore]
        public PhotoSizes FirstFrameForStory { get { return GetFirstFrame(248); } }

        [JsonPropertyName("platform")]
        public string Platform { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("processing")]
        public int Processing { get; set; }

        [JsonPropertyName("live")]
        public int Live { get; set; }

        [JsonPropertyName("upcoming")]
        public int Upcoming { get; set; }

        [JsonPropertyName("views")]
        public int Views { get; set; }

        private PhotoSizes GetFirstFrame(double maxWidth) {
            if (FirstFrame == null && FirstFrame.Count == 0) return null;
            PhotoSizes cps = null;
            foreach (PhotoSizes ps in FirstFrame) {
                if (cps == null) cps = ps;
                if (cps.Width > maxWidth) return cps;
                if (cps.Width < ps.Width) cps = ps;
            }
            return cps;
        }
    }
}
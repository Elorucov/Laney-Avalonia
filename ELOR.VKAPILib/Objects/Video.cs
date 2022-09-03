using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ELOR.VKAPILib.Objects
{
    public class VideoFiles {
        [JsonProperty("external")]
        public string External { get; set; }

        [JsonProperty("mp4_240")]
        public string MP4p240 { get; set; }

        [JsonProperty("mp4_360")]
        public string MP4p360 { get; set; }

        [JsonProperty("mp4_480")]
        public string MP4p480 { get; set; }

        [JsonProperty("mp4_720")]
        public string MP4p720 { get; set; }

        [JsonProperty("mp4_1080")]
        public string MP4p1080 { get; set; }
    }

    public class Video : AttachmentBase, IPreview {
        [JsonIgnore]
        public override string ObjectType { get { return "video"; } }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonIgnore]
        public TimeSpan DurationTime { get { return TimeSpan.FromSeconds(Duration); } }

        [JsonProperty("image")]
        public List<PhotoSizes> Image { get; set; }

        [JsonIgnore]
        public Uri PreviewImageUri { get { return FirstFrame[0].Uri; } }

        [JsonIgnore]
        public Size PreviewImageSize { get { return FirstFrame[0].Size; } }

        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("date")]
        public int DateUnix { get; set; }

        [JsonIgnore]
        public DateTime Date { get { return DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime.ToLocalTime(); } }

        [JsonProperty("adding_date")]
        public int AddingDateUnix { get; set; }

        [JsonIgnore]
        public DateTime AddingDate { get { return DateTimeOffset.FromUnixTimeSeconds(AddingDateUnix).DateTime.ToLocalTime(); } }

        [JsonProperty("files")]
        public VideoFiles Files { get; set; }

        [JsonProperty("player")]
        public string Player { get; set; }

        [JsonIgnore]
        public Uri PlayerUri { get { return new Uri(Player); } }

        [JsonProperty("first_frame")]
        public List<PhotoSizes> FirstFrame { get; set; }

        [JsonIgnore]
        public PhotoSizes FirstFrameForStory { get { return GetFirstFrame(248); } }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("processing")]
        public int Processing { get; set; }

        [JsonProperty("live")]
        public int Live { get; set; }

        [JsonProperty("upcoming")]
        public int Upcoming { get; set; }

        [JsonProperty("views")]
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

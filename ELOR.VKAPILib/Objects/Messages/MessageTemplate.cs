using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Messages {
    public class MessageTemplate : ICloneable {
        public MessageTemplate() { }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("creator_id")]
        public long CreatorId { get; set; }

        [JsonPropertyName("creation_time")]
        public int CreationTimeUnix { get; set; }

        [JsonIgnore]
        public DateTime CreationTime { get { return DateTimeOffset.FromUnixTimeSeconds(CreationTimeUnix).DateTime.ToLocalTime(); } }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("usages_all_time")]
        public int UsagesAllTime { get; set; }

        [JsonPropertyName("usages_week")]
        public int UsagesWeek { get; set; }

        [JsonPropertyName("editor_id")]
        public long EditorId { get; set; }

        [JsonPropertyName("update_time")]
        public int UpdateTimeUnix { get; set; }

        [JsonIgnore]
        public DateTime UpdateTime { get { return DateTimeOffset.FromUnixTimeSeconds(UpdateTimeUnix).DateTime.ToLocalTime(); } }

        public object Clone() {
            return (MessageTemplate)MemberwiseClone();
        }
    }

    public class AddTemplateResponse {
        public AddTemplateResponse() { }

        [JsonPropertyName("template_id")]
        public int TemplateId { get; set; }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELOR.VKAPILib.Objects.Messages {
    public class MessageTemplate {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("creator_id")]
        public int CreatorId { get; set; }

        [JsonProperty("creation_time")]
        public int CreationTimeUnix { get; set; }

        [JsonIgnore]
        public DateTime CreationTime { get { return DateTimeOffset.FromUnixTimeSeconds(CreationTimeUnix).DateTime.ToLocalTime(); } }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("usages_all_time")]
        public int UsagesAllTime { get; set; }

        [JsonProperty("usages_week")]
        public int UsagesWeek { get; set; }

        [JsonProperty("editor_id")]
        public int EditorId { get; set; }

        [JsonProperty("update_time")]
        public int UpdateTimeUnix { get; set; }

        [JsonIgnore]
        public DateTime UpdateTime { get { return DateTimeOffset.FromUnixTimeSeconds(UpdateTimeUnix).DateTime.ToLocalTime(); } }
    }

    public class AddTemplateResponse {
        [JsonProperty("template_id")]
        public int TemplateId { get; set; }
    }
}

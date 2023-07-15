using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Messages {
    public class IsAllowedResponse {
        public IsAllowedResponse() {}
        
        [JsonPropertyName("is_allowed")]
        public bool IsAllowed { get; set; }
    }
}
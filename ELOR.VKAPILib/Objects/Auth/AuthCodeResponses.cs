using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Auth {
    public class GetAuthCodeResponse {
        [JsonPropertyName("auth_code")]
        public string AuthCode { get; set; }

        [JsonPropertyName("auth_hash")]
        public string AuthHash { get; set; }

        [JsonPropertyName("auth_id")]
        public string AuthId { get; set; }

        [JsonPropertyName("auth_url")]
        public string AuthUrl { get; set; }

        [JsonPropertyName("expires_in")]
        public long ExpiresIn { get; set; }
    }

    public class CheckAuthCodeResponse {
        [JsonPropertyName("status")]
        public int Status { get; set; } // 0 — not scanned, 1 — scanned and waiting to confirm, 2 — success, 3 — canceled.

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("super_app_token")]
        public string SuperAppToken { get; set; }

        [JsonPropertyName("is_partial")]
        public bool IsPartial { get; set; }
    }
}

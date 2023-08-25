using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Auth {
    public class OauthResponse {
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }

    public class BanInfo {
        [JsonPropertyName("member_name")]
        public string MemberName { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class DirectAuthResponse : OauthResponse {
        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("error_type")]
        public string ErrorType { get; set; }

        [JsonPropertyName("error_description")]
        public string ErrorDescription { get; set; }

        [JsonPropertyName("phone_mask")]
        public string PhoneMask { get; set; }

        [JsonPropertyName("validation_type")]
        public string ValidationType { get; set; }

        [JsonPropertyName("captcha_sid")]
        public string CaptchaSid { get; set; }

        [JsonPropertyName("captcha_img")]
        public string CaptchaImg { get; set; }

        [JsonPropertyName("ban_info")]
        public BanInfo BanInfo { get; set; }
    }
}
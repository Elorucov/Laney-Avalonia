using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects.Auth {
    public class OauthResponse {
        [JsonPropertyName("user_id")]
        public long UserId { get; private set; }

        [JsonPropertyName("expires_in")]
        public long ExpiresIn { get; private set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; private set; }
    }

    public class BanInfo {
        [JsonPropertyName("member_name")]
        public string MemberName { get; private set; }

        [JsonPropertyName("message")]
        public string Message { get; private set; }
    }

    public class DirectAuthResponse : OauthResponse {
        [JsonPropertyName("error")]
        public string Error { get; private set; }

        [JsonPropertyName("error_type")]
        public string ErrorType { get; private set; }

        [JsonPropertyName("error_description")]
        public string ErrorDescription { get; private set; }

        [JsonPropertyName("phone_mask")]
        public string PhoneMask { get; private set; }

        [JsonPropertyName("validation_type")]
        public string ValidationType { get; private set; }

        [JsonPropertyName("captcha_sid")]
        public string CaptchaSid { get; private set; }

        [JsonPropertyName("captcha_img")]
        public string CaptchaImg { get; private set; }

        [JsonPropertyName("ban_info")]
        public BanInfo BanInfo { get; private set; }
    }
}
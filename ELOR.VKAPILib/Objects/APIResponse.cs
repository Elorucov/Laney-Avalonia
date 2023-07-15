using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    public class APIResponse<T> {

        public APIResponse() {}

        [JsonPropertyName("response")]
        public T Response { get; set; }

        [JsonPropertyName("error")]
        public APIException Error { get; set; }
    }

    public class APIException : Exception {
        public APIException() {}

        [JsonPropertyName("error_code")]
        public int Code { get; set; }

        [JsonPropertyName("error_msg")]
        public string Message { get; set; }

        [JsonPropertyName("captcha_sid")]
        public string CaptchaSID { get; set; }

        [JsonPropertyName("captcha_img")]
        public string CaptchaImage { get; set; }

        [JsonPropertyName("redirect_url")]
        public string RedirectUrl { get; set; }

        [JsonIgnore]
        public Uri RedirectUri { get { return new Uri(RedirectUrl); }  }

        [JsonPropertyName("confirmation_text")]
        public string ConfirmationText { get; set; }
    }
}
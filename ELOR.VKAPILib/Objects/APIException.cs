using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects {
    public class APIException : Exception {
        [JsonProperty("error_code")]
        public int Code { get; set; }

        [JsonProperty("error_msg")]
        public string Message { get; set; }

        [JsonProperty("captcha_sid")]
        public string CaptchaSID { get; set; }

        [JsonProperty("captcha_img")]
        public string CaptchaImage { get; set; }

        [JsonProperty("redirect_url")]
        public string RedirectUrl { get; set; }

        [JsonIgnore]
        public Uri RedirectUri { get { return new Uri(RedirectUrl); }  }

        [JsonProperty("confirmation_text")]
        public string ConfirmationText { get; set; }
    }
}

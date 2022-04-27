using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects.Upload {
    public class UploadError {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty("error_descr")]
        public string ErrorDescription { get; set; }
    }
}

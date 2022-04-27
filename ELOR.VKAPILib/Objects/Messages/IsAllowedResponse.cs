using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELOR.VKAPILib.Objects.Messages {
    public class IsAllowedResponse {
        [JsonProperty("is_allowed")]
        public bool IsAllowed { get; internal set; }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects.Upload {
    public class DocumentUploadResult {
        [JsonProperty("file")]
        public string File { get; set; }
    }
}

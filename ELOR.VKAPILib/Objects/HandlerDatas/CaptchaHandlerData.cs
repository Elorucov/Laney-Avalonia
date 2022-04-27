using System;
using System.Collections.Generic;
using System.Text;

namespace ELOR.VKAPILib.Objects.HandlerDatas {
    public class CaptchaHandlerData {
        public string SID { get; internal set; }
        public Uri Image { get; internal set; }
    }
}

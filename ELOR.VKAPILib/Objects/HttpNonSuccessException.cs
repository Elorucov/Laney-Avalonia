using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ELOR.VKAPILib.Objects {
    public class HttpNonSuccessException : Exception {
        public HttpStatusCode StatusCode { get; private set; }

        internal HttpNonSuccessException(HttpStatusCode statusCode, string content) : base(content) {
            StatusCode = statusCode;
        }
    }
}

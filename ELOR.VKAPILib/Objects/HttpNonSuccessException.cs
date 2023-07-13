using System.Net;

namespace ELOR.VKAPILib.Objects {
    public class HttpNonSuccessException : Exception {
        public HttpStatusCode StatusCode { get; private set; }

        internal HttpNonSuccessException(HttpStatusCode statusCode, string content) : base(content) {
            StatusCode = statusCode;
        }
    }
}
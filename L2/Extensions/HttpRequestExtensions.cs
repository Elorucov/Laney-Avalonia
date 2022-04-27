using System;
using System.Net.Http;

namespace ELOR.Laney.Extensions {
    public static class HttpRequestExtensions {
        private const string TimeoutPropertyKey = "RequestTimeout";

        public static void SetTimeout(this HttpRequestMessage request, TimeSpan? timeout) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Options.Set(new HttpRequestOptionsKey<TimeSpan?>(TimeoutPropertyKey), timeout);
        }

        public static TimeSpan? GetTimeout(this HttpRequestMessage request) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Options.TryGetValue(new HttpRequestOptionsKey<TimeSpan?>(TimeoutPropertyKey), out var value) && value is TimeSpan timeout)
                return timeout;
            return null;
        }
    }
}
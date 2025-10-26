using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ELOR.Laney.Core.Network {
    public class LNet {
        static HttpClient defaultClient;
        static HttpClient zstdClient;

        public static event EventHandler<string> DebugLog;
        private static void Log(string text) {
            Debug.WriteLine($"LNet: {text}");
            DebugLog?.Invoke(null, text);
        }

        private static HttpClient GetConfiguredHttpClient() {
            return new HttpClient(new HttpClientHandler() {
                AllowAutoRedirect = true,
                MaxConnectionsPerServer = 50,
                AutomaticDecompression = DecompressionMethods.All
            }, false) {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public static async Task<HttpResponseMessage> GetAsync(Uri uri,
            Dictionary<string, string> parameters = null,
            Dictionary<string, string> headers = null,
            CancellationTokenSource cts = null) {
            return await InternalSendRequestAsync(uri, parameters, cts, headers, HttpMethod.Get);
        }

        public static async Task<HttpResponseMessage> PostAsync(Uri uri,
            Dictionary<string, string> parameters = null,
            Dictionary<string, string> headers = null,
            CancellationTokenSource cts = null) {
            return await InternalSendRequestAsync(uri, parameters, cts, headers, HttpMethod.Post);
        }

        private static async Task<HttpResponseMessage> InternalSendRequestAsync(Uri uri, Dictionary<string, string> parameters, CancellationTokenSource cts, Dictionary<string, string> headers = null, HttpMethod httpMethod = null) {
            if (httpMethod == null) httpMethod = HttpMethod.Get;

            bool isZstdRequest = headers?.ContainsKey("Accept-Encoding") == true && headers["Accept-Encoding"].Contains("zstd");

            Uri fixedUri = uri;
            string host = string.Empty;

            HttpRequestMessage hrm = new HttpRequestMessage(httpMethod, fixedUri) {
                Version = new Version(2, 0)
            };
            if (!String.IsNullOrEmpty(host)) hrm.Headers.Host = host;
            if (headers != null) foreach (var header in headers) {
                    hrm.Headers.Add(header.Key, header.Value);
                }
            if (parameters != null) hrm.Content = new FormUrlEncodedContent(parameters);

            HttpClient client = null;
            if (isZstdRequest) {
                if (zstdClient == null) zstdClient = GetConfiguredHttpClient();
                zstdClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("zstd"));
                client = zstdClient;
            } else {
                if (defaultClient == null) defaultClient = GetConfiguredHttpClient();
                defaultClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                defaultClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                client = defaultClient;
            }

            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            if (!client.DefaultRequestHeaders.Contains("User-Agent")) client.DefaultRequestHeaders.Add("User-Agent", App.UserAgent);

#if RELEASE
            string paramstr = "";
#else
            StringBuilder paramstr = new StringBuilder();
            if (parameters != null) {
                paramstr.Append(" | ");
                foreach (var p in parameters) {
                    string value = p.Value.Replace("\n", "").Replace("\r\n", "");
                    if (p.Key == "access_token" || p.Key == "code") continue;
                    paramstr.Append($"{p.Key}={value}; ");
                }
            }
#endif
#if !RELEASE
            if (Settings.LNetLogs) Log($"=> {fixedUri.AbsoluteUri} | {httpMethod.Method}{paramstr}");
#endif
            var stopwatch = Stopwatch.StartNew();
            var result = cts == null ? await client.SendAsync(hrm, HttpCompletionOption.ResponseHeadersRead)
                : await client.SendAsync(hrm, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            stopwatch.Stop();

#if !RELEASE
            if (Settings.LNetLogs) Log($"<= {fixedUri.AbsoluteUri} | Code: {result.StatusCode} | {stopwatch.ElapsedMilliseconds} ms.");
#endif

            return result;
        }
    }
}
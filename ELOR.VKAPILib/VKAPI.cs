using ELOR.VKAPILib.Methods;
using ELOR.VKAPILib.Objects;
using ELOR.VKAPILib.Objects.HandlerDatas;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib {
    public class VKAPI {

        #region Methods

        public AccountMethods Account { get; private set; }
        public AppsMethods Apps { get; private set; }
        public AuthMethods Auth { get; private set; }
        public DocsMethods Docs { get; private set; }
        public FriendsMethods Friends { get; private set; }
        public GroupsMethods Groups { get; private set; }
        public MessagesMethods Messages { get; private set; }
        public PhotosMethods Photos { get; private set; }
        public PollsMethods Polls { get; private set; }
        public StoreMethods Store { get; private set; }
        public UsersMethods Users { get; private set; }
        public UtilsMethods Utils { get; private set; }
        public VideoMethods Video { get; private set; }
        public MethodsSectionBase Execute { get; private set; }

        #endregion

        #region Funcs

        public Func<Uri, Dictionary<string, string>, Dictionary<string, string>, Task<HttpResponseMessage>> WebRequestCallback { get; set; }
        public Func<CaptchaHandlerData, Task<string>> CaptchaHandler { get; set; }
        public Func<string, Task<bool>> ActionConfirmationHandler { get; set; }

        #endregion

        #region Fields & Properties

        private string _accessToken;
        private string _language;
        private string _domain;
        private static string _version = "5.238";

        public string AccessToken { get { return _accessToken; } internal set { _accessToken = value; } }
        public string Language { get { return _language; } set { _language = value; } }
        public string Domain { get { return _domain; } }
        public int LongPollVersion { get; set; } = 19;
        public static string UserAgent { get; private set; }
        public static string Version { get { return _version; } }

        private HttpClient HttpClient;

        #endregion

        #region Events

        public event EventHandler UserAuthorizationFailed;
        public event EventHandler<Uri> ValidationRequired;
        public event EventHandler UserDeletedOrBanned;

        #endregion

        public VKAPI(string accessToken, string language, string userAgent, string domain = "api.vk.com") {
            _accessToken = accessToken;
            _language = language;
            UserAgent = userAgent;
            _domain = domain;

            HttpClientHandler handler = new HttpClientHandler() {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            HttpClient = new HttpClient(handler, false);
            HttpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate");
            if (!String.IsNullOrEmpty(userAgent)) HttpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);

            Account = new AccountMethods(this);
            Apps = new AppsMethods(this);
            Auth = new AuthMethods(this);
            Docs = new DocsMethods(this);
            Friends = new FriendsMethods(this);
            Groups = new GroupsMethods(this);
            Messages = new MessagesMethods(this);
            Photos = new PhotosMethods(this);
            Polls = new PollsMethods(this);
            Store = new StoreMethods(this);
            Users = new UsersMethods(this);
            Utils = new UtilsMethods(this);
            Video = new VideoMethods(this);
        }

        internal Dictionary<string, string> GetNormalizedParameters(Dictionary<string, string> parameters) {
            Dictionary<string, string> prmkv = new Dictionary<string, string>();

            foreach (var a in parameters) {
                // prmkv.Add(a.Key, WebUtility.UrlDecode(a.Value));
                prmkv.Add(a.Key, a.Value);
            }

            prmkv.Add("access_token", AccessToken);
            if (!prmkv.ContainsKey("lang")) prmkv.Add("lang", Language);
            if (!prmkv.ContainsKey("v")) prmkv.Add("v", Version);

            return prmkv;
        }

        public async Task<HttpContent> SendRequestAsync(string method, Dictionary<string, string> parameters = null) {
            string requestUri = $@"https://{Domain}/method/{method}";
            return await SendRequestAsync(new Uri(requestUri), parameters);
        }

        internal async Task<HttpContent> SendRequestAsync(Uri uri, Dictionary<string, string> parameters = null) {
            if (WebRequestCallback != null) {
                Dictionary<string, string> headers = new Dictionary<string, string> {
                    { "Accept-Encoding", "gzip,deflate" }
                };
                if (!String.IsNullOrEmpty(UserAgent)) headers.Add("User-Agent", UserAgent);
                if (uri.AbsoluteUri.Contains("auth.getAuthCode") || uri.AbsoluteUri.Contains("auth.checkAuthCode")) {
                    headers.Add("Origin", $"https://id.vk.com");
                }

                var resp = await WebRequestCallback.Invoke(uri, parameters, headers);
                return resp.Content;
            } else {
                using (HttpRequestMessage hmsg = new HttpRequestMessage(HttpMethod.Post, uri) {
                    Version = new Version(2, 0)
                }) {
                    if (uri.AbsoluteUri.Contains("auth.getAuthCode") || uri.AbsoluteUri.Contains("auth.checkAuthCode")) {
                        hmsg.Headers.Add("Origin", $"https://id.vk.com");
                    }
                    hmsg.Content = new FormUrlEncodedContent(parameters);

                    var resp = await HttpClient.SendAsync(hmsg);
                    return resp.Content;
                }
            }
        }

        public async Task<JsonDocument> CallMethodAsync(string method, Dictionary<string, string> parameters = null) {
            if (parameters == null) parameters = new Dictionary<string, string>();

            using var response = await SendRequestAsync(method, GetNormalizedParameters(parameters));
            using var stream = await response.ReadAsStreamAsync();
            return await JsonDocument.ParseAsync(stream);
        }

        public async Task<T> CallMethodAsync<T>(string method, Dictionary<string, string> parameters = null, JsonSerializerContext serializerContext = null) {
            if (parameters == null) parameters = new Dictionary<string, string>();

            using var response = await SendRequestAsync(method, GetNormalizedParameters(parameters));
            using var respStream = await response.ReadAsStreamAsync();
            JsonNode resp = await JsonNode.ParseAsync(respStream);
            if (resp["error"] != null) {
                APIException apiex = (APIException)resp["error"].Deserialize(typeof(APIException), BuildInJsonContext.Default);
                switch (apiex.Code) {
                    case 5: UserAuthorizationFailed?.Invoke(this, null); throw apiex;
                    case 14: return await HandleCaptchaRequest<T>(apiex, method, parameters).ConfigureAwait(false);
                    case 17: ValidationRequired?.Invoke(this, apiex.RedirectUri); throw apiex;
                    case 18: UserDeletedOrBanned?.Invoke(this, null); throw apiex;
                    case 24: return await HandleActionConfirmationRequest<T>(apiex, method, parameters).ConfigureAwait(false);
                    default: throw apiex;
                }
            } else if (resp["response"] != null) {
                if (serializerContext == null) serializerContext = BuildInJsonContext.Default;
                T apiresp = (T)resp["response"].Deserialize(typeof(T), serializerContext);
                return apiresp;
            } else {
                throw new Exception("Invalid response from VK API backend!");
            }
        }

        private async Task<T> HandleCaptchaRequest<T>(APIException apiex, string method, Dictionary<string, string> parameters) {
            if (CaptchaHandler != null) {
                CaptchaHandlerData chd = new CaptchaHandlerData {
                    SID = apiex.CaptchaSID,
                    Image = new Uri(apiex.CaptchaImage)
                };
                string key = String.Empty;
                key = await CaptchaHandler.Invoke(chd);
                if (String.IsNullOrEmpty(key)) throw apiex;
                if (parameters.ContainsKey("captcha_sid")) {
                    parameters["captcha_sid"] = apiex.CaptchaSID;
                } else {
                    parameters.Add("captcha_sid", apiex.CaptchaSID);
                }

                if (parameters.ContainsKey("captcha_key")) {
                    parameters["captcha_key"] = key;
                } else {
                    parameters.Add("captcha_key", key);
                }

                return await CallMethodAsync<T>(method, parameters).ConfigureAwait(false);
            } else {
                throw apiex;
            }
        }

        private async Task<T> HandleActionConfirmationRequest<T>(APIException apiex, string method, Dictionary<string, string> parameters) {
            if (ActionConfirmationHandler != null) {
                bool result = await ActionConfirmationHandler.Invoke(apiex.ConfirmationText);
                if (!result) throw apiex;
                parameters.Add("confirm", "1");
                return await CallMethodAsync<T>(method, parameters).ConfigureAwait(false);
            } else {
                throw apiex;
            }
        }
    }
}
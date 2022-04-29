using ELOR.VKAPILib.Attributes;
using ELOR.VKAPILib.Methods;
using ELOR.VKAPILib.Objects;
using ELOR.VKAPILib.Objects.HandlerDatas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ELOR.VKAPILib {
    public class VKAPI {

        #region Methods

        public AccountMethods Account { get; private set; }
        public AppsMethods Apps { get; private set; }
        public DocsMethods Docs { get; private set; }
        public FriendsMethods Friends { get; private set; }
        public GroupsMethods Groups { get; private set; }
        public MessagesMethods Messages { get; private set; }
        public PhotosMethods Photos { get; private set; }
        public PollsMethods Polls { get; private set; }
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

        private int _userId;
        private string _accessToken;
        private string _language;
        private string _domain;
        private static string _version = "5.144";

        public int UserId { get { return _userId; } }
        public string AccessToken { get { return _accessToken; } }
        public string Language { get { return _language; } }
        public string Domain { get { return _domain; } }
        public int LongPollVersion { get; set; } = 11;
        public static string UserAgent { get; private set; }
        public static string Version { get { return _version; } }

        private HttpClient HttpClient;

        #endregion

        #region Events

        public event EventHandler UserAuthorizationFailed;
        public event EventHandler<Uri> ValidationRequired;
        public event EventHandler UserDeletedOrBanned;

        #endregion

        public VKAPI(int userId, string accessToken, string language, string userAgent, string domain = "api.vk.com") {
            _userId = userId;
            _accessToken = accessToken;
            _language = language;
            UserAgent = userAgent;
            _domain = domain;

            HttpClientHandler handler = new HttpClientHandler() {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            HttpClient = new HttpClient(handler);
            HttpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate");
            if (!String.IsNullOrEmpty(userAgent)) HttpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);

            Account = new AccountMethods(this);
            Apps = new AppsMethods(this);
            Docs = new DocsMethods(this);
            Friends = new FriendsMethods(this);
            Groups = new GroupsMethods(this);
            Messages = new MessagesMethods(this);
            Photos = new PhotosMethods(this);
            Polls = new PollsMethods(this);
            Users = new UsersMethods(this);
            Utils = new UtilsMethods(this);
            Video = new VideoMethods(this);
        }

        internal Dictionary<string, string> GetNormalizedParameters(Dictionary<string, string> parameters) {
            Dictionary<string, string> prmkv = new Dictionary<string, string>();

            foreach (var a in parameters) {
                prmkv.Add(a.Key, WebUtility.UrlDecode(a.Value));
            }

            prmkv.Add("access_token", AccessToken);
            if (!prmkv.ContainsKey("lang")) prmkv.Add("lang", Language);
            if (!prmkv.ContainsKey("v")) prmkv.Add("v", Version);

            return prmkv;
        }

        public async Task<string> SendRequestAsync(string method, Dictionary<string, string> parameters = null) {
            string requestUri = $@"https://{Domain}/method/{method}";

            if (WebRequestCallback != null) {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Accept-Encoding", "gzip,deflate");
                if (!String.IsNullOrEmpty(UserAgent)) headers.Add("User-Agent", UserAgent);

                var resp = await WebRequestCallback.Invoke(new Uri(requestUri), parameters, headers);
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadAsStringAsync();
            } else {
                using (HttpRequestMessage hmsg = new HttpRequestMessage(HttpMethod.Post, new Uri(requestUri))) {
                    hmsg.Content = new FormUrlEncodedContent(parameters);
                    using (var resp = await HttpClient.SendAsync(hmsg)) {
                        resp.EnsureSuccessStatusCode();
                        return await resp.Content.ReadAsStringAsync();
                    }
                }
            }
        }

        public async Task<T> CallMethodAsync<T>(MethodsSectionBase methodClass, Dictionary<string, string> parameters = null, [CallerMemberName] string callerName = "") {
            var s = methodClass.GetType().GetTypeInfo();
            SectionAttribute si = s.GetCustomAttribute<SectionAttribute>();
            MethodInfo m = null;
            foreach (var mm in s.DeclaredMethods) {
                if (mm.Name == callerName) {
                    m = mm; break;
                }
            }
            if (m == null) throw new Exception("Failed to get member info!");
            MethodAttribute mi = m.GetCustomAttribute<MethodAttribute>();
            string method = $"{si.Name}.{mi.Name}";
            return await CallMethodAsync<T>(method, parameters);
        }

        public async Task<T> CallMethodAsync<T>(string method, Dictionary<string, string> parameters = null) {
            if(parameters == null) parameters = new Dictionary<string, string>();

            string response = await SendRequestAsync(method, GetNormalizedParameters(parameters));
            JObject jr = JObject.Parse(response);
            if(jr["error"] != null) {
                APIException apiex = JsonConvert.DeserializeObject<APIException>(jr["error"].ToString(Formatting.None));
                switch(apiex.Code) {
                    case 5: UserAuthorizationFailed?.Invoke(this, null); throw apiex;
                    case 14: return await HandleCaptchaRequest<T>(apiex, method, parameters).ConfigureAwait(false);
                    case 17: ValidationRequired?.Invoke(this, apiex.RedirectUri); throw apiex;
                    case 18: UserDeletedOrBanned?.Invoke(this, null); throw apiex;
                    case 24: return await HandleActionConfirmationRequest<T>(apiex, method, parameters).ConfigureAwait(false);
                    default: throw apiex;
                }
            } else if(jr["response"] != null) {
                return jr["response"].ToObject<T>();
            } else {
                throw new Exception("Invalid response.");
            }
        }

        private async Task<T> HandleCaptchaRequest<T>(APIException apiex, string method, Dictionary<string, string> parameters) {
            if(CaptchaHandler != null) {
                CaptchaHandlerData chd = new CaptchaHandlerData {
                    SID = apiex.CaptchaSID,
                    Image = new Uri(apiex.CaptchaImage)
                };
                string key = String.Empty;
                key = await CaptchaHandler.Invoke(chd);
                if (String.IsNullOrEmpty(key)) throw apiex;
                parameters.Add("captcha_sid", apiex.CaptchaSID);
                parameters.Add("captcha_key", key);
                return await CallMethodAsync<T>(method, parameters).ConfigureAwait(false);
            } else {
                throw apiex;
            }
        }

        private async Task<T> HandleActionConfirmationRequest<T>(APIException apiex, string method, Dictionary<string, string> parameters) {
            if(ActionConfirmationHandler != null) {
                bool result = await ActionConfirmationHandler.Invoke(apiex.ConfirmationText);
                if(!result) throw apiex;
                parameters.Add("confirm", "1");
                return await CallMethodAsync<T>(method, parameters).ConfigureAwait(false);
            } else {
                throw apiex;
            }
        }
    }
}
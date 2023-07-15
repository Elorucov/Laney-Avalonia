using ELOR.VKAPILib.Attributes;
using ELOR.VKAPILib.Objects;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ELOR.VKAPILib.Methods {
    [Section("utils")]
    public class UtilsMethods : MethodsSectionBase {
        internal UtilsMethods(VKAPI api) : base(api) { }

        public async Task<ResolveScreenNameResult> ResolveScreenNameAsync(string screenName) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("screen_name", screenName);
            //return await API.CallMethodAsync<ResolveScreenNameResult>(this, parameters);

            // Знали бы разработчики VK API, какую боль в заднице испытывают программисты на строго-типизированных языках,
            // разрабатывая библиотеки для VK API...
            string response = await API.SendRequestAsync("utils.resolveScreenName", API.GetNormalizedParameters(parameters));
            var jr = JsonNode.Parse(response);
            if (jr["response"] is JsonArray) {
                return null;
            } else {
                return jr["response"].Deserialize<ResolveScreenNameResult>();
            }
        }
    }
}
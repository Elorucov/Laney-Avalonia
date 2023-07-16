using ELOR.VKAPILib.Objects;

namespace ELOR.VKAPILib.Methods {

    public class AppsMethods : MethodsSectionBase {
        internal AppsMethods(VKAPI api) : base(api) { }

        public async Task<AppsList> GetAsync(int appId = 0) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (appId > 0) parameters.Add("app_id", appId.ToString());
            return await API.CallMethodAsync<AppsList>("apps.get", parameters);
        }
    }
}
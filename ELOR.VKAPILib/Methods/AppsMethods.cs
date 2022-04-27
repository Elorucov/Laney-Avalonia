using ELOR.VKAPILib.Attributes;
using ELOR.VKAPILib.Objects;

namespace ELOR.VKAPILib.Methods {

    [Section("apps")]
    public class AppsMethods : MethodsSectionBase {
        internal AppsMethods(VKAPI api) : base(api) { }

        [Method("get")]
        public async Task<VKList<App>> GetAsync(int appId = 0) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (appId > 0) parameters.Add("app_id", appId.ToString());
            return await API.CallMethodAsync<VKList<App>>(this, parameters);
        }
    }
}
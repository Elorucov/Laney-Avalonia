using ELOR.Laney.Execute.Objects;
using ELOR.VKAPILib;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ELOR.Laney.Execute {
    public static class ExecuteMethods {
        public static async Task<StartSessionResponse> StartSessionAsync(this VKAPI API) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("func_v", "2");
            parameters.Add("lp_version", Core.LongPoll.VERSION.ToString());
            return await API.CallMethodAsync<StartSessionResponse>("execute.l2StartSession", parameters);
        }
    }
}
using ELOR.VKAPILib.Attributes;
using ELOR.VKAPILib.Objects;

namespace ELOR.VKAPILib.Methods {

    [Section("account")]
    public class AccountMethods : MethodsSectionBase {
        internal AccountMethods(VKAPI api) : base(api) { }

        [Method("ban")]
        public async Task<bool> BanAsync(int ownerId) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("owner_id", ownerId.ToString());
            return await API.CallMethodAsync<bool>(this, parameters);
        }

        [Method("getBanned")]
        public async Task<VKList<int>> GetBannedAsync(List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<VKList<int>>(this, parameters);
        }

        [Method("getPrivacySettings")]
        public async Task<PrivacyResponse> GetPrivacySettingsAsync() {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            return await API.CallMethodAsync<PrivacyResponse>(this, parameters);
        }

        [Method("setPrivacy")]
        public async Task<PrivacySettingValue> SetPrivacyAsync(string key, string value) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("key", key);
            parameters.Add("value", value);
            return await API.CallMethodAsync<PrivacySettingValue>(this, parameters);
        }


        [Method("setSilenceMode")]
        public async Task<bool> SetSilenceModeAsync(int time, int peerId, bool sound) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("time", time.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("sound", sound ? "1" : "0");
            return await API.CallMethodAsync<bool>(this, parameters);
        }

        [Method("unban")]
        public async Task<bool> UnbanAsync(int ownerId) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("owner_id", ownerId.ToString());
            return await API.CallMethodAsync<bool>(this, parameters);
        }
    }
}
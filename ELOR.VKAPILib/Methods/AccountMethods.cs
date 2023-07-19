using ELOR.VKAPILib.Objects;

namespace ELOR.VKAPILib.Methods {

    public class AccountMethods : MethodsSectionBase {
        internal AccountMethods(VKAPI api) : base(api) { }

        public async Task<bool> BanAsync(long ownerId) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "owner_id", ownerId.ToString() }
            };
            return await API.CallMethodAsync<bool>("account.ban", parameters);
        }

        public async Task<LongList> GetBannedAsync(List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<LongList>("account.getBanned", parameters);
        }

        public async Task<PrivacyResponse> GetPrivacySettingsAsync() {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            return await API.CallMethodAsync<PrivacyResponse>("account.getPrivacySettings", parameters);
        }

        public async Task<PrivacySettingValue> SetPrivacyAsync(string key, string value) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "key", key },
                { "value", value }
            };
            return await API.CallMethodAsync<PrivacySettingValue>("account.setPrivacy", parameters);
        }

        public async Task<int> SetSilenceModeAsync(int time, long peerId, bool sound) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "time", time.ToString() },
                { "peer_id", peerId.ToString() },
                { "sound", sound ? "1" : "0" }
            };
            return await API.CallMethodAsync<int>("account.setSilenceMode", parameters);
        }

        public async Task<bool> UnbanAsync(long ownerId) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "owner_id", ownerId.ToString() }
            };
            return await API.CallMethodAsync<bool>("account.unban", parameters);
        }
    }
}
using ELOR.Laney.Core;
using ELOR.Laney.Execute.Objects;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ELOR.Laney.Execute {
    public static class ExecuteMethods {
        private static async Task<string> GetCodeAsync(string fileName) {
            return await AssetsManager.GetStringFromUriAsync(new System.Uri(Path.Combine(Constants.ExecuteCodePath, $"{fileName}.txt")));
        }

        public static async Task<StartSessionResponse> StartSessionAsync(this VKAPI API, List<long> groupIds) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "code", await GetCodeAsync("startSession") },
                { "group_ids", string.Join(',', groupIds) },
                { "lp_version", Core.LongPoll.VERSION.ToString() },
                { "fields", string.Join(',', VKAPIHelper.Fields) }
            };
            return await API.CallMethodAsync<StartSessionResponse>("execute", parameters, L2JsonSerializerContext.Default);
        }

        public static async Task<StartSessionResponse> GetGroupsWithLongPollAsync(this VKAPI API, List<long> groupIds) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "code", await GetCodeAsync("getGroupsAndLP") },
                { "group_ids", string.Join(',', groupIds) },
                { "lp_version", Core.LongPoll.VERSION.ToString() }
            };
            return await API.CallMethodAsync<StartSessionResponse>("execute", parameters, L2JsonSerializerContext.Default);
        }

        public static async Task<List<AlbumLite>> GetPhotoAlbumsAsync(this VKAPI API, long ownerId) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "code", await GetCodeAsync("getPhotoAlbums") },
                { "owner_id", ownerId.ToString() }
            };
            return await API.CallMethodAsync<List<AlbumLite>>("execute", parameters, L2JsonSerializerContext.Default);
        }

        public static async Task<List<AlbumLite>> GetVideoAlbumsAsync(this VKAPI API, long ownerId) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "code", await GetCodeAsync("getVideoAlbums") },
                { "owner_id", ownerId.ToString() }
            };
            return await API.CallMethodAsync<List<AlbumLite>>("execute", parameters, L2JsonSerializerContext.Default);
        }

        public static async Task<StickerPickerData> GetRecentStickersAndGraffitiesAsync(this VKAPI API) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "code", await GetCodeAsync("getRecentStickersAndGraffities") },
            };
            return await API.CallMethodAsync<StickerPickerData>("execute", parameters, L2JsonSerializerContext.Default);
        }

        public static async Task<UserEx> GetUserCardAsync(this VKAPI API, long userId, List<string> fields) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "code", await GetCodeAsync("getUserCard") },
                { "user_id", userId.ToString() },
                { "fields", string.Join(",", fields) }
            };
            return await API.CallMethodAsync<UserEx>("execute", parameters, L2JsonSerializerContext.Default);
        }

        public static async Task<GroupEx> GetGroupCardAsync(this VKAPI API, long groupId, List<string> fields) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "code", await GetCodeAsync("getGroupCard") },
                { "fields", string.Join(",", fields) },
                { "group_id", groupId.ToString() }
            };
            return await API.CallMethodAsync<GroupEx>("execute", parameters, L2JsonSerializerContext.Default);
        }

        public static async Task<ChatInfoEx> GetChatAsync(this VKAPI API, long chatId, List<string> fields) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "code", await GetCodeAsync("getChatInfo") },
                { "chat_id", chatId.ToString() }
            };
            return await API.CallMethodAsync<ChatInfoEx>("execute", parameters, L2JsonSerializerContext.Default);
        }
    }
}
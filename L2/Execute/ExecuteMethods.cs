﻿using ELOR.Laney.Execute.Objects;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ELOR.Laney.Execute {
    public static class ExecuteMethods {
        public static async Task<StartSessionResponse> StartSessionAsync(this VKAPI API, List<long> groupIds) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "func_v", "2" },
                { "group_ids", string.Join(',', groupIds) },
                { "lp_version", Core.LongPoll.VERSION.ToString() },
                { "fields", string.Join(',', VKAPIHelper.Fields) }
            };
            return await API.CallMethodAsync<StartSessionResponse>("execute.l2StartSession", parameters, L2JsonSerializerContext.Default);
        }

        public static async Task<StartSessionResponse> GetGroupsWithLongPollAsync(this VKAPI API, List<long> groupIds) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "group_ids", string.Join(',', groupIds) },
                { "lp_version", Core.LongPoll.VERSION.ToString() }
            };
            return await API.CallMethodAsync<StartSessionResponse>("execute.l2GetGroupsAndLP", parameters, L2JsonSerializerContext.Default);
        }

        public static async Task<List<AlbumLite>> GetPhotoAlbumsAsync(this VKAPI API, long ownerId) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "owner_id", ownerId.ToString() }
            };
            return await API.CallMethodAsync<List<AlbumLite>>("execute.getPhotoAlbums", parameters, L2JsonSerializerContext.Default);
        }

        public static async Task<List<AlbumLite>> GetVideoAlbumsAsync(this VKAPI API, long ownerId) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "owner_id", ownerId.ToString() }
            };
            return await API.CallMethodAsync<List<AlbumLite>>("execute.getVideoAlbums", parameters, L2JsonSerializerContext.Default);
        }

        public static async Task<StickerPickerData> GetRecentStickersAndGraffitiesAsync(this VKAPI API) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "func_v", "2" }
            };
            return await API.CallMethodAsync<StickerPickerData>("execute.getRecentStickersAndGraffities", parameters, L2JsonSerializerContext.Default);
        }

        public static async Task<UserEx> GetUserCardAsync(this VKAPI API, long userId) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "user_id", userId.ToString() },
                { "func_v", "3" }
            };
            return await API.CallMethodAsync<UserEx>("execute.getUserCard", parameters, L2JsonSerializerContext.Default);
        }

        public static async Task<GroupEx> GetGroupCardAsync(this VKAPI API, long groupId) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "group_id", groupId.ToString() }
            };
            return await API.CallMethodAsync<GroupEx>("execute.getGroupCard", parameters, L2JsonSerializerContext.Default);
        }

        public static async Task<ChatInfoEx> GetChatAsync(this VKAPI API, long chatId, List<string> fields) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "chat_id", chatId.ToString() },
                { "fields", string.Join(",", fields) },
                { "func_v", "3" }
            };
            return await API.CallMethodAsync<ChatInfoEx>("execute.getChatInfoWithMembers", parameters, L2JsonSerializerContext.Default);
        }
    }
}
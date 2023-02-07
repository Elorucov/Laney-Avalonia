﻿using ELOR.Laney.Execute.Objects;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib;
using ELOR.VKAPILib.Objects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ELOR.Laney.Execute {
    public static class ExecuteMethods {
        public static async Task<StartSessionResponse> StartSessionAsync(this VKAPI API) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("func_v", "2");
            parameters.Add("lp_version", Core.LongPoll.VERSION.ToString());
            parameters.Add("fields", string.Join(",", VKAPIHelper.Fields));
            return await API.CallMethodAsync<StartSessionResponse>("execute.l2StartSession", parameters);
        }

        public static async Task<MessagesHistoryEx> GetHistoryWithMembersAsync(this VKAPI API, int groupId, int peerId, int offset, int count, int startMessageId, bool rev, List<string> fields, bool dontReturnMembers = false) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("peer_id", peerId.ToString());
            parameters.Add("offset", offset.ToString());
            parameters.Add("count", count.ToString());
            parameters.Add("start_message_id", startMessageId.ToString());
            if (rev) parameters.Add("rev", "1");
            if (dontReturnMembers) parameters.Add("do_not_return_members", "1");
            parameters.Add("fields", string.Join(",", fields));
            parameters.Add("func_v", "5");
            return await API.CallMethodAsync<MessagesHistoryEx>("execute.getHistoryWithMembers", parameters);
        }

        public static async Task<List<AlbumLite>> GetPhotoAlbumsAsync(this VKAPI API, int ownerId) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("owner_id", ownerId.ToString());
            return await API.CallMethodAsync<List<AlbumLite>>("execute.getPhotoAlbums", parameters);
        }

        public static async Task<List<AlbumLite>> GetVideoAlbumsAsync(this VKAPI API, int ownerId) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("owner_id", ownerId.ToString());
            return await API.CallMethodAsync<List<AlbumLite>>("execute.getVideoAlbums", parameters);
        }

        public static async Task<StickerPickerData> GetRecentStickersAndGraffitiesAsync(this VKAPI API) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("func_v", "2");
            return await API.CallMethodAsync<StickerPickerData>("execute.getRecentStickersAndGraffities", parameters);
        }

        public static async Task<UserEx> GetUserCardAsync(this VKAPI API, int userId) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("user_id", userId.ToString());
            parameters.Add("func_v", "3");
            return await API.CallMethodAsync<UserEx>("execute.getUserCard", parameters);
        }

        public static async Task<GroupEx> GetGroupCardAsync(this VKAPI API, int groupId) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("group_id", groupId.ToString());
            return await API.CallMethodAsync<GroupEx>("execute.getGroupCard", parameters);
        }

        public static async Task<ChatInfoEx> GetChatAsync(this VKAPI API, int chatId, List<string> fields) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("chat_id", chatId.ToString());
            parameters.Add("fields", string.Join(",", fields));
            parameters.Add("func_v", "2");
            return await API.CallMethodAsync<ChatInfoEx>("execute.getChatInfoWithMembers", parameters);
        }
    }
}
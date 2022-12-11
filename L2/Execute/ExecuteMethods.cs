using ELOR.Laney.Execute.Objects;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib;
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
    }
}
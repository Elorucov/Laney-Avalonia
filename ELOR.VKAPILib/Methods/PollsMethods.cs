using ELOR.VKAPILib.Attributes;
using ELOR.VKAPILib.Objects;
using Newtonsoft.Json;

namespace ELOR.VKAPILib.Methods {
    [Section("polls")]
    public class PollsMethods : MethodsSectionBase {
        internal PollsMethods(VKAPI api) : base(api) { }

        /// <summary>Creates polls that can be attached to the users' or communities' posts or messages.</summary>
        /// <param name="question">question text.</param>
        /// <param name="answers">available answers list.</param>
        /// <param name="isAnonymous">true — anonymous poll, participants list is hidden.</param>
        /// <param name="isMultiple">true — to create a poll with a multiple choice.</param>
        /// <param name="disableUnvote">true — to disable unvote.</param>
        /// <param name="endDate">date when the poll should be closed in Unixtime.</param>
        /// <param name="backgroundId">background ID for the snippet.</param>
        /// <param name="ownerId">If a poll will be added to a communty it is required to send a negative group identifier. Current user by default.</param>
        [Method("create")]
        public async Task<Poll> CreateAsync(string question, List<string> answers, bool isAnonymous = false, bool isMultiple = false, bool disableUnvote = false, long endDate = 0, int backgroundId = 0, long ownerId = 0) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "question", question },
                { "add_answers", JsonConvert.SerializeObject(answers) }
            };
            if (isAnonymous) parameters.Add("is_anonymous", "1");
            if (isMultiple) parameters.Add("is_multiple", "1");
            if (disableUnvote) parameters.Add("disable_unvote", "1");
            if (endDate > 0) parameters.Add("end_date", endDate.ToString());
            if (backgroundId > 0) parameters.Add("background_id", backgroundId.ToString());
            if (ownerId != 0) parameters.Add("owner_id", ownerId.ToString());
            return await API.CallMethodAsync<Poll>(this, parameters);
        }

        /// <summary>Return default backgrounds for polls.</summary>
        [Method("getBackgrounds")]
        public async Task<List<PollBackground>> GetBackgroundsAsync() {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            return await API.CallMethodAsync<List<PollBackground>>(this, parameters);
        }

        /// <summary>Returns detailed information about a poll by its ID.</summary>
        /// <param name="ownerId">ID of the user or community that owns the poll.</param>
        /// <param name="pollId">Poll ID.</param>
        /// <param name="extended">true —  to return additional fields for users.</param>
        /// <param name="fields">Profile fields to return.</param>
        [Method("getById")]
        public async Task<Poll> GetByIdAsync(long ownerId, int pollId, bool extended = false, List<string> fields = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "owner_id", ownerId.ToString() },
                { "poll_id", pollId.ToString() }
            };
            if (extended) parameters.Add("extended", "1");
            if (!fields.IsNullOrEmpty()) parameters.Add("fields", fields.Combine());
            return await API.CallMethodAsync<Poll>(this, parameters);
        }
    }
}
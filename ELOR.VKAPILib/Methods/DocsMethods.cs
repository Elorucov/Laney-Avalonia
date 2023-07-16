using ELOR.VKAPILib.Attributes;
using ELOR.VKAPILib.Objects;
using ELOR.VKAPILib.Objects.Upload;

namespace ELOR.VKAPILib.Methods {
    public class DocsMethods : MethodsSectionBase {
        internal DocsMethods(VKAPI api) : base(api) { }

        /// <summary>Copies a document to a user's or community's document list.</summary>
        /// <param name="ownerId">ID of the user or community that owns the document.</param>
        /// <param name="docId">Document ID.</param>
        /// <param name="accessKey">Access key. This parameter is required if access_key was returned with the document's data.</param>
        public async Task<int> AddAsync(long ownerId, int docId, string accessKey = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "owner_id", ownerId.ToString() },
                { "doc_id", docId.ToString() }
            };
            if (!String.IsNullOrEmpty(accessKey)) parameters.Add("access_key", accessKey);
            return await API.CallMethodAsync<int>("docs.add", parameters);
        }

        /// <summary>Returns detailed information about user or community documents.</summary>
        /// <param name="ownerId">ID of the user or community that owns the documents.</param>
        /// <param name="type">Document type. See possible values at vk.com/dev/docs.get</param>
        /// <param name="offset">Offset needed to return a specific subset of photos.</param>
        /// <param name="count">Number of photos to return. </param>
        /// <param name="returnTags">true — to return tags.</param>
        public async Task<DocumentsList> GetAsync(long ownerId, int type = 0, int offset = 0, int count = 50, bool returnTags = false) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "owner_id", ownerId.ToString() },
                { "type", type.ToString() }
            };
            if (offset > 0) parameters.Add("offset", offset.ToString());
            if (count > 0) parameters.Add("count", count.ToString());
            if (returnTags) parameters.Add("return_tags", "1");
            return await API.CallMethodAsync<DocumentsList>("docs.get", parameters);
        }

        /// <summary>Returns the server address for document upload.</summary>
        /// <param name="peerId">Destination ID.</param>
        public async Task<VkUploadServer> GetMessagesUploadServerAsync(long groupId, long peerId, bool isAudioMessage = false) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            if (peerId > 0) parameters.Add("peer_id", peerId.ToString());
            if (isAudioMessage) parameters.Add("type", "audio_message");
            return await API.CallMethodAsync<VkUploadServer>("docs.getMessagesUploadServer", parameters);
        }

        /// <summary>Saves a document after uploading it to a server.</summary>
        public async Task<DocumentSaveResult> SaveAsync(long groupId, string file, string title = null, string tags = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("file", file);
            if (!String.IsNullOrEmpty(title)) parameters.Add("title", title);
            if (!String.IsNullOrEmpty(tags)) parameters.Add("tags", tags);
            return await API.CallMethodAsync<DocumentSaveResult>("docs.save", parameters);
        }
    }
}
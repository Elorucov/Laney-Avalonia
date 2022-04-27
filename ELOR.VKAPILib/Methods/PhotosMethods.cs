using ELOR.VKAPILib.Attributes;
using ELOR.VKAPILib.Objects;
using ELOR.VKAPILib.Objects.Upload;

namespace ELOR.VKAPILib.Methods {
    [Section("photos")]
    public class PhotosMethods : MethodsSectionBase {
        internal PhotosMethods(VKAPI api) : base(api) { }

        /// <summary>Allows to copy a photo to the "Saved photos" album.</summary>
        /// <param name="ownerId">photo's owner ID.</param>
        /// <param name="photoId">photo ID.</param>
        /// <param name="accessKey">special access key for private photos.</param>
        [Method("copy")]
        public async Task<int> CopyAsync(int ownerId, int photoId, string accessKey = null) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("owner_id", ownerId.ToString());
            parameters.Add("photo_id", photoId.ToString());
            if (!String.IsNullOrEmpty(accessKey)) parameters.Add("access_key", accessKey);
            return await API.CallMethodAsync<int>(this, parameters);
        }

        /// <summary>Returns a list of a user's or community's photo albums.</summary>
        /// <param name="ownerId">ID of the user or community that owns the albums.</param>
        /// <param name="albumIds">Album IDs.</param>
        /// <param name="offset">Offset needed to return a specific subset of albums.</param>
        /// <param name="count">Number of albums to return. </param>
        /// <param name="needSystem">true — to return system albums with negative IDs.</param>
        /// <param name="needCovers">true — to return an additional ThumbSrc and Thumb fields.</param>
        /// <param name="photoSizes">true — to return PhotoSizes (instead ThumbSrc and Thumb).</param>
        [Method("getAlbums")]
        public async Task<VKList<PhotoAlbum>> GetAlbumsAsync(int ownerId, List<int> albumIds = null, int offset = 0, int count = 0, bool needSystem = false, bool needCovers = false, bool photoSizes = false) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("owner_id", ownerId.ToString());
            if (!albumIds.IsNullOrEmpty()) parameters.Add("album_ids", albumIds.Combine());
            if (offset > 0) parameters.Add("offset", offset.ToString());
            if (count > 0) parameters.Add("count", count.ToString());
            if (needSystem) parameters.Add("need_system", "1");
            if (needCovers) parameters.Add("need_covers", "1");
            if (photoSizes) parameters.Add("photo_sizes", "1");
            return await API.CallMethodAsync<VKList<PhotoAlbum>>(this, parameters);
        }

        /// <summary>Returns a list of photos belonging to a user or community, in reverse chronological order.</summary>
        /// <param name="ownerId">ID of the user or community that owns the albums.</param>
        /// <param name="extended">true — to return detailed information about photos.</param>
        /// <param name="offset">Offset needed to return a specific subset of photos.</param>
        /// <param name="count">Number of photos to return. </param>
        /// <param name="photoSizes">true — to return PhotoSizes.</param>
        /// <param name="noServiceAlbums">true — to return photos only from standard albums.</param>
        /// <param name="needHidden">true — to show information about photos being hidden from the block above the wall.</param>
        /// <param name="skipHidden">true — not to return photos being hidden from the block above the wall. Works only with ownerId > 0, noServiceAlbums is ignored.</param>
        [Method("getAll")]
        public async Task<VKList<Photo>> GetAllAsync(int ownerId, bool extended, int offset = 0, int count = 0, bool photoSizes = false, bool noServiceAlbums = false, bool needHidden = false, bool skipHidden = false) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("owner_id", ownerId.ToString());
            if (extended) parameters.Add("extended", "1");
            if (offset > 0) parameters.Add("offset", offset.ToString());
            if (count > 0) parameters.Add("count", count.ToString());
            if (photoSizes) parameters.Add("photo_sizes", "1");
            if (noServiceAlbums) parameters.Add("no_service_albums", "1");
            if (needHidden) parameters.Add("need_hidden", "1");
            if (skipHidden) parameters.Add("skip_hidden", "1");
            return await API.CallMethodAsync<VKList<Photo>>(this, parameters);
        }

        /// <summary>Returns a list of a user's or community's photos.</summary>
        /// <param name="ownerId">ID of the user or community that owns the photos.</param>
        /// <param name="albumId">Photo album ID.</param>
        /// <param name="photoIds">Photo IDs.</param>
        /// <param name="rev">Sort order. true — reverse chronological.</param>
        /// <param name="extended">true — to return additional likes, comments, and tags fields.</param>
        /// <param name="offset">Offset needed to return a specific subset of photos.</param>
        /// <param name="count">Number of photos to return. </param>
        /// <param name="photoSizes">true — to return PhotoSizes.</param>
        [Method("get")]
        public async Task<VKList<Photo>> GetAsync(int ownerId, int albumId, List<int> photoIds, bool rev, bool extended = false, int offset = 0, int count = 50, bool photoSizes = false) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("owner_id", ownerId.ToString());
            parameters.Add("album_id", albumId.ToString());
            if (!photoIds.IsNullOrEmpty()) parameters.Add("photo_ids", photoIds.Combine());
            if (rev) parameters.Add("rev", "1");
            if (extended) parameters.Add("extended", "1");
            if (offset > 0) parameters.Add("offset", offset.ToString());
            if (count > 0) parameters.Add("count", count.ToString());
            if (photoSizes) parameters.Add("photo_sizes", "1");
            return await API.CallMethodAsync<VKList<Photo>>(this, parameters);
        }

        /// <summary>Returns the server address for photo upload in a private message for a user.</summary>
        /// <param name="peerId">Peer ID (for community messages).</param>
        [Method("getMessagesUploadServer")]
        public async Task<PhotoUploadServer> GetMessagesUploadServerAsync(int groupId, int peerId = 0) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            if (peerId > 0) parameters.Add("peer_id", peerId.ToString());
            return await API.CallMethodAsync<PhotoUploadServer>(this, parameters);
        }

        /// <summary>Saves a photo after being successfully uploaded.</summary>
        [Method("saveMessagesPhoto")]
        public async Task<List<PhotoSaveResult>> SaveMessagesPhotoAsync(int groupId, int server, string photo, string hash) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            parameters.Add("server", server.ToString());
            parameters.Add("photo", photo);
            parameters.Add("hash", hash);
            return await API.CallMethodAsync<List<PhotoSaveResult>>(this, parameters);
        }
    }
}
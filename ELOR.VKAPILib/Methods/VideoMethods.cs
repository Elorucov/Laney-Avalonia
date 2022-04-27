using ELOR.VKAPILib.Attributes;
using ELOR.VKAPILib.Objects;
using ELOR.VKAPILib.Objects.Upload;

namespace ELOR.VKAPILib.Methods {
    [Section("video")]
    public class VideoMethods : MethodsSectionBase {
        internal VideoMethods(VKAPI api) : base(api) { }

        /// <summary>Returns a list of video albums owned by a user or community.</summary>
        /// <param name="ownerId">ID of the user or community that owns the video albums.</param>
        /// <param name="offset">Offset needed to return a specific subset of video albums.</param>
        /// <param name="count">Number of video albums to return.</param>
        /// <param name="extended">true — to return additional fields Count and Photo properties for each album.</param>
        /// <param name="needSystem">true — to return system albums.</param>
        [Method("getAlbums")]
        public async Task<VKList<VideoAlbum>> GetAlbumsAsync(int ownerId, int offset = 0, int count = 0, bool extended = false, bool needSystem = false) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("owner_id", ownerId.ToString());
            if (offset > 0) parameters.Add("offset", offset.ToString());
            if (count > 0) parameters.Add("count", count.ToString());
            if (extended) parameters.Add("extended", "1");
            if (needSystem) parameters.Add("need_system", "1");
            return await API.CallMethodAsync<VKList<VideoAlbum>>(this, parameters);
        }

        /// <summary>Returns a list of a user's or community's videos.</summary>
        /// <param name="ownerId">ID of the user or community that owns the videos.</param>
        /// <param name="albumId">ID of the album containing the videos.</param>
        /// <param name="offset">Offset needed to return a specific subset of videos.</param>
        /// <param name="count">Number of videos to return.</param>
        /// <param name="extended">true — to return an extended response with additional fields.</param>
        [Method("get")]
        public async Task<VKList<Video>> GetAsync(int ownerId, string videos = null, int albumId = 0, int offset = 0, int count = 50, bool extended = false) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("owner_id", ownerId.ToString());
            if (!String.IsNullOrEmpty(videos)) parameters.Add("videos", videos);
            if (albumId > 0) parameters.Add("album_id", albumId.ToString());
            if (offset > 0) parameters.Add("offset", offset.ToString());
            if (count > 0) parameters.Add("count", count.ToString());
            if (extended) parameters.Add("extended", "1");
            return await API.CallMethodAsync<VKList<Video>>(this, parameters);
        }

        /// <summary>Returns a server address (required for upload) and video data.</summary>
        /// <param name="groupId">ID of the community in which the video will be saved. By default (0), the current user's page.</param>
        /// <param name="name">Name of the video.</param>
        /// <param name="description">Description of the video.</param>
        /// <param name="isPrivate">true — to designate the video as private (send it via a private message); the video will not appear on the user's video list and will not be available by ID for other users.</param>
        /// <param name="wallpost">true — to post the saved video on a user's wall.</param>
        /// <param name="link">URL for embedding the video from an external website.</param>
        /// <param name="albumId">ID of the album to which the saved video will be added.</param>
        [Method("save")]
        public async Task<VideoUploadServer> SaveAsync(int groupId = 0, string name = null, string description = null, bool isPrivate = false, bool wallpost = false, string link = null, int albumId = 0) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (groupId > 0) parameters.Add("group_id", groupId.ToString());
            if (!String.IsNullOrEmpty(name)) parameters.Add("name", name);
            if (!String.IsNullOrEmpty(description)) parameters.Add("description", description);
            if (isPrivate) parameters.Add("is_private", "1");
            if (wallpost) parameters.Add("wallpost", "1");
            if (!String.IsNullOrEmpty(link)) parameters.Add("link", link);
            if (albumId > 0) parameters.Add("album_id", albumId.ToString());
            return await API.CallMethodAsync<VideoUploadServer>(this, parameters);
        }
    }
}
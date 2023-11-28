using Avalonia.Platform.Storage;
using ELOR.Laney.Core.Network;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public static class CacheManager {
        private static List<User> CachedUsers = new List<User>();
        private static List<Group> CachedGroups = new List<Group>();
        private static Dictionary<long, List<ChatViewModel>> CachedChats = new Dictionary<long, List<ChatViewModel>>();
        public static void Add(IEnumerable<User> users) {
            if (users == null) return;
            foreach (User user in users) {
                Add(user);
            }
        }

        public static void Add(IEnumerable<Group> groups) {
            if (groups == null) return;
            foreach (Group group in groups) {
                Add(group);
            }
        }

        public static void Add(User user) {
            lock (CachedUsers) {
                int index = CachedUsers.FindIndex(i => i.Id == user.Id);
                if (index >= 0) {
                    CachedUsers[index] = user;
                } else {
                    CachedUsers.Add(user);
                }
            }
        }

        public static void Add(Group group) {
            lock (CachedGroups) {
                int index = CachedGroups.FindIndex(i => i.Id == group.Id);
                if (index >= 0) {
                    CachedGroups[index] = group;
                } else {
                    CachedGroups.Add(group);
                }
            }
        }

        public static void Add(long sessionId, ChatViewModel chat) {
            lock (CachedChats) {
                if (!CachedChats.ContainsKey(sessionId)) CachedChats.Add(sessionId, new List<ChatViewModel>());
                int index = CachedChats[sessionId].FindIndex(i => i.PeerId == chat.PeerId);
                if (index >= 0) {
                    // Мы не будем добавлять в кэш беседу.
                } else {
                    CachedChats[sessionId].Add(chat);
                }
            }
        }

        public static User GetUser(long id) {
            try {
                return CachedUsers.FirstOrDefault(i => i.Id == id);
            } catch (Exception ex) {
                Log.Error(ex, $"Error while getting user with id {id} from cache!");
                return null;
            }
        }

        public static Group GetGroup(long id) {
            try {
                if (id < 0) id = id * -1;
                return CachedGroups.FirstOrDefault(i => i.Id == id);
            } catch (Exception ex) {
                Log.Error(ex, $"Error while getting group with id {id} from cache!");
                return null;
            }
        }

        public static ChatViewModel GetChat(long sessionId, long peerId) {
            if (!CachedChats.ContainsKey(sessionId)) return null;
            try {
                return CachedChats[sessionId].FirstOrDefault(i => i.PeerId == peerId);
            } catch (Exception ex) {
                Log.Error(ex, $"Error while getting chat with id {peerId} from cache!");
                return null;
            }
        }

        // First name, last name, avatar
        public static Tuple<string, string, Uri> GetNameAndAvatar(long id, bool shortLastName = false) {
            if (id.IsUser()) {
                User u = GetUser(id);
                if (u == null) return null;
                string lastName = u.LastName;
                if (shortLastName && !String.IsNullOrEmpty(lastName) && lastName.Length > 1)
                    lastName = lastName[0].ToString();
                return new Tuple<string, string, Uri>(u.FirstName, lastName, u.Photo);
            } else if (id.IsGroup()) {
                Group g = GetGroup(id);
                if (g == null) return null;
                return new Tuple<string, string, Uri>(g.Name, null, g.Photo);
            }
            return null;
        }

        public static string GetNameOnly(long id, bool shortLastName = false) {
            var t = GetNameAndAvatar(id, shortLastName);
            if (t == null) return id.ToString();
            return id.IsUser() ? $"{t.Item1} {t.Item2}" : t.Item1;
        }

        public static void ClearUsersAndGroupsCache() {
            CachedUsers.Clear();
            CachedGroups.Clear();
        }

        #region Files

        // TODO: TaskCompletionSource
        public static async Task<bool> GetFileFromCacheAsync(Uri uri) {
            try {
                string cachePath = Path.Combine(App.LocalDataPath, "cache");
                Directory.CreateDirectory(cachePath);

                string filePath = Path.Combine(cachePath, uri.Segments.Last());
                if (File.Exists(filePath)) return true;

                HttpResponseMessage hmsg = await LNet.GetAsync(uri);
                using (var stream = await hmsg.Content.ReadAsStreamAsync()) {
                    using (var fileStream = File.Open(filePath, FileMode.Create)) {
                        await stream.CopyToAsync(fileStream);
                        await fileStream.FlushAsync();
                    }
                }
                return true;
            } catch (Exception ex) {
                Log.Error(ex, "Error while caching a file!");
                return false;
            }
        }

        #endregion
    }
}
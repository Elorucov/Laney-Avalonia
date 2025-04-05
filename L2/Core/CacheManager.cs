using Avalonia.Platform;
using Avalonia.Svg.Skia;
using ELOR.Laney.Core.Network;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public static class CacheManager {
        private static Dictionary<long, User> CachedUsers = new Dictionary<long, User>();
        private static Dictionary<long, Group> CachedGroups = new Dictionary<long, Group>();
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
                if (CachedUsers.ContainsKey(user.Id)) {
                    CachedUsers[user.Id] = user;
                } else {
                    CachedUsers.Add(user.Id, user);
                }
            }
        }

        public static void Add(Group group) {
            lock (CachedGroups) {
                if (CachedGroups.ContainsKey(group.Id)) {
                    CachedGroups[group.Id] = group;
                } else {
                    CachedGroups.Add(group.Id, group);
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
                if (CachedUsers.ContainsKey(id)) return CachedUsers[id];
            } catch (Exception ex) {
                Log.Error(ex, $"Error while getting user with id {id} from cache!");
            }
            return null;
        }

        public static Group GetGroup(long id) {
            try {
                if (id < 0) id = id * -1;
                if (CachedGroups.ContainsKey(id)) return CachedGroups[id];
            } catch (Exception ex) {
                Log.Error(ex, $"Error while getting group with id {id} from cache!");
            }
            return null;
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

        public static void RemoveChat(ChatViewModel chat) {
            if (!CachedChats.ContainsKey(chat.OwnedSessionId)) return;
            try {
                var list = CachedChats[chat.OwnedSessionId];
                if (list.Contains(chat)) {
                    chat.Unload();
                    list.Remove(chat);
                } else {
                    Log.Warning($"CacheManager.RemoveChat: chat VM itself {chat.Id} not found in cache. Try find via id.");
                    chat = list.FirstOrDefault(i => i.PeerId == chat.PeerId);
                    if (chat != null) {
                        chat.Unload();
                        list.Remove(chat);
                    } else {
                        Log.Warning($"CacheManager.RemoveChat: chat {chat.Id} not found in cache. Nothing to remove");
                    }
                }
            } catch (Exception ex) {
                Log.Error(ex, $"Error while removing chat with id {chat.Id} from cache!");
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
                return new Tuple<string, string, Uri>(String.Intern(u.FirstName), lastName, u.Photo);
            } else if (id.IsGroup()) {
                Group g = GetGroup(id);
                if (g == null) return null;
                return new Tuple<string, string, Uri>(String.Intern(g.Name), null, g.Photo);
            }
            return null;
        }

        public static string GetNameOnly(long id, bool shortLastName = false) {
            var t = GetNameAndAvatar(id, shortLastName);
            if (t == null) return id.ToString();
            return id.IsUser() ? String.Intern($"{t.Item1} {t.Item2}") : String.Intern(t.Item1);
        }

        public static void ClearUsersAndGroupsCache() {
            CachedUsers.Clear();
            CachedGroups.Clear();
        }

        #region Reactions assets

        public static ConcurrentDictionary<int, ReactionAssetLinks> ReactionsAssets { get; private set; }
        public static List<int> AvailableReactions { get; private set; }
        public static ConcurrentDictionary<string, SvgImage> ReactionsAssetData { get; private set; } = new ConcurrentDictionary<string, SvgImage>();
        static ConcurrentDictionary<string, ManualResetEventSlim> nowLoading = new ConcurrentDictionary<string, ManualResetEventSlim>();

        public static void SetReactionsInfo(List<int> available, List<ReactionAssets> assets) {
            AvailableReactions = available;
            ReactionsAssets = new ConcurrentDictionary<int, ReactionAssetLinks>();
            if (assets == null) return;
            foreach (var a in assets) {
                ReactionsAssets.TryAdd(a.ReactionId, a.Links);
            }
        }

        public static string GetStaticReactionUrl(int id) {
            if (!ReactionsAssets.ContainsKey(id)) return "avares://laney/Assets/placeholder.svg";
            return ReactionsAssets[id].Static;
        }

        public static async Task<SvgImage> GetStaticReactionImageAsync(Uri uri) {
            if (uri.Scheme == "avares") {
                using var stream = AssetLoader.Open(uri);
                using StreamReader reader = new StreamReader(stream);
                string data = reader.ReadToEnd();
                return new SvgImage {
                    Source = SvgSource.LoadFromSvg(data)
                };
            }
            string key = uri.AbsoluteUri;
            if (!ReactionsAssetData.ContainsKey(key)) {
                if (nowLoading.ContainsKey(key)) {
                    var lmres = nowLoading[key];
                    await Task.Factory.StartNew(lmres.Wait).ConfigureAwait(true);
                    // lmres.Dispose();
                    return ReactionsAssetData[key];
                }
                string data = null;
                ManualResetEventSlim mres = new ManualResetEventSlim();
                bool isAdded = nowLoading.TryAdd(key, mres);
                if (!isAdded) Log.Warning($"GetStaticReactionImage: cannot add MRES \"{uri}\"!");

                using var response = await LNet.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                data = await response.Content.ReadAsStringAsync();
                SvgImage image = new SvgImage {
                    Source = SvgSource.LoadFromSvg(data)
                };

                if (!ReactionsAssetData.ContainsKey(key)) {
                    bool isAdded2 = ReactionsAssetData.TryAdd(key, image);
                    if (!isAdded2) Log.Warning($"GetStaticReactionImage: cannot add svg asset data \"{uri}\"!");
                }

                ManualResetEventSlim outmres = null;
                bool isRemoved2 = nowLoading.TryRemove(key, out outmres);
                if (!isRemoved2) Log.Warning($"GetStaticReactionImage: cannot remove MRES \"{uri}\"!");
                mres.Set();
                // mres.Dispose();
                return image;
            } else {
                return ReactionsAssetData[key];
            }
        }

        #endregion

        #region Files

        // TODO: TaskCompletionSource
        public static async Task<bool> GetFileFromCacheAsync(Uri uri) {
            try {
                string cachePath = Path.Combine(App.LocalDataPath, "cache");
                Directory.CreateDirectory(cachePath);

                string filePath = Path.Combine(cachePath, uri.Segments.Last());
                if (File.Exists(filePath)) return true;

                using HttpResponseMessage hmsg = await LNet.GetAsync(uri);
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
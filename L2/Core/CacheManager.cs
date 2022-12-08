using ELOR.Laney.ViewModels;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ELOR.Laney.Core {
    public static class CacheManager {
        private static List<User> CachedUsers = new List<User>();
        private static List<Group> CachedGroups = new List<Group>();
        private static Dictionary<int, List<ChatViewModel>> CachedChats = new Dictionary<int, List<ChatViewModel>>();
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

        public static void Add(int sessionId, ChatViewModel chat) {
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

        public static User GetUser(int id) {
            try {
                return CachedUsers.FirstOrDefault(i => i.Id == id);
            } catch (Exception ex) {
                // Log.General.Error($"Error while getting user with id {id} from cache!", ex);
                return null;
            }
        }

        public static Group GetGroup(int id) {
            try {
                if (id < 0) id = id * -1;
                return CachedGroups.FirstOrDefault(i => i.Id == id);
            } catch (Exception ex) {
                // Log.General.Error($"Error while getting group with id {id} from cache!", ex);
                return null;
            }
        }

        public static ChatViewModel GetChat(int sessionId, int peerId) {
            if (!CachedChats.ContainsKey(sessionId)) return null;
            try {
                return CachedChats[sessionId].FirstOrDefault(i => i.PeerId == peerId);
            } catch (Exception ex) {
                // Log.General.Error($"Error while getting chat with id {id} from cache!", ex);
                return null;
            }
        }

        // First name, last name, avatar
        public static Tuple<string, string, Uri> GetNameAndAvatar(int id, bool shortLastName = false) {
            if (id > 0) {
                User u = GetUser(id);
                if (u == null) return null;
                string lastName = u.LastName;
                if (shortLastName && !String.IsNullOrEmpty(lastName) && lastName.Length > 1)
                    lastName = lastName[0].ToString();
                return new Tuple<string, string, Uri>(u.FirstName, lastName, u.Photo);
            } else if (id < 0) {
                Group g = GetGroup(id);
                if (g == null) return null;
                return new Tuple<string, string, Uri>(g.Name, null, g.Photo);
            }
            return null;
        }

        public static string GetNameOnly(int id, bool shortLastName = false) {
            var t = GetNameAndAvatar(id, shortLastName);
            if (t == null) return String.Empty;
            return id > 0 ? $"{t.Item1} {t.Item2}" : t.Item1;
        }
    }
}
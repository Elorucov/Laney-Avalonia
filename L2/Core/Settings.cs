using ELOR.Laney.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ELOR.Laney.Core {
    public static class Settings {
        private static Dictionary<string, object> _settings = new Dictionary<string, object>();
        private static FileStream _file;
        public static string FilePath { get; private set; }

        public delegate void SettingChangedDelegate(string key, object value);
        public static event SettingChangedDelegate SettingChanged;

        #region Initialization

        public static void Initialize() {
            FilePath = Path.Combine(App.LocalDataPath, "settings.xml");
            _file = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096);

#if MAC
#else
            _file.Lock(0, 0);
#endif

            byte[] fileBytes = new byte[_file.Length];
            _file.Read(fileBytes, 0, fileBytes.Length);

            UTF8Encoding enc = new UTF8Encoding(true);
            string content = enc.GetString(fileBytes);

            if (content.Length == 0) return;
            try {
                _settings = ElorPrefs.Deserialize(content);
            } catch (Exception ex) {
                Log.Error(ex, "An error occured while reading settings file!");
            }
        }

        private static async void UpdateFile() {
            string content = ElorPrefs.SerializeToXML(_settings);
            byte[] bytes = Encoding.UTF8.GetBytes(content);

            _file.Position = 0;
            _file.SetLength(bytes.Length);
            await _file.WriteAsync(bytes);
            await _file.FlushAsync();
        }

        public static void UnlockSettingsFile(bool doNotUpdateFile = false) {
            if (!doNotUpdateFile) UpdateFile();
            _file.Close();
            _file.Dispose();
        }

        #endregion

        #region Getter/setter

        public static T Get<T>(string key, T defaultValue = default) {
            if (!_settings.ContainsKey(key)) return defaultValue;
            try {
                object v = _settings[key];
                return v != null ? (T)_settings[key] : defaultValue;
            } catch {
                return defaultValue;
            }
        }

        public static void Set(string key, object value) {
            AddOrReplace(key, value);
            UpdateFile();
            SettingChanged?.Invoke(key, value);
        }

        public static void SetBatch(Dictionary<string, object> settings) {
            foreach (var setting in settings) {
                AddOrReplace(setting.Key, setting.Value);
                SettingChanged?.Invoke(setting.Key, setting.Value);
            }
            UpdateFile();
        }

        private static void AddOrReplace(string key, object value) {
            if (value == null) {
                if (_settings.ContainsKey(key)) {
                    _settings.Remove(key);
                    if (key == VK_TOKEN) {
                        if (_settings.ContainsKey($"{key}1")) _settings.Remove($"{key}1");
                        if (_settings.ContainsKey($"{key}2")) _settings.Remove($"{key}2");
                    }
                }
                return;
            }

            if (_settings.ContainsKey(key)) {
                if (key == VK_TOKEN) {
                    var result = Encryption.Encrypt(AssetsManager.BinaryPayload.Skip(576).Take(32).OrderDescending().ToArray(), (string)value);
                    _settings[key] = result.Item1;
                    if (_settings.ContainsKey($"{key}1")) _settings[$"{key}1"] = result.Item2;
                    if (_settings.ContainsKey($"{key}2")) _settings[$"{key}2"] = result.Item3;
                } else {
                    _settings[key] = value;
                }
            } else {
                if (key == VK_TOKEN) {
                    var result = Encryption.Encrypt(AssetsManager.BinaryPayload.Skip(576).Take(32).OrderDescending().ToArray(), (string)value);
                    _settings.Add(key, result.Item1);
                    if (!_settings.ContainsKey($"{key}1")) _settings.Add($"{key}1", result.Item2);
                    if (!_settings.ContainsKey($"{key}2")) _settings.Add($"{key}2", result.Item3);
                } else {
                    _settings.Add(key, value);
                }
            }
        }

        #endregion

        #region Constants

        public const string TEST_STRING = "test_string";

        public const string VK_USER_ID = "user_id";
        public const string VK_TOKEN = "access_token";
        public const string GROUPS = "groups";

        public const string WIN_SIZE_W = "winw";
        public const string WIN_SIZE_H = "winh";
        public const string WIN_POS_X = "winx";
        public const string WIN_POS_Y = "winy";
        public const string WIN_MAXIMIZED = "winm";

        public const string LANGUAGE = "lang";
        public const string SEND_VIA_ENTER = "sent_via_enter";
        public const string DONT_PARSE_LINKS = "dont_parse_liks";
        public const string DISABLE_MENTIONS = "disable_mentions";
        public const string STICKERS_SUGGEST = "suggest_stickers";
        public const string STICKERS_ANIMATE = "animate_stickers";

        public const string THEME = "theme";
        public const string CHAT_ITEM_MORE_ROWS = "chat_item_more_rows";

        public const string NOTIF_PRIVATE = "notifications_private";
        public const string NOTIF_PRIVATE_SOUND = "notifications_private_sound";
        public const string NOTIF_GCHAT = "notifications_group_chat";
        public const string NOTIF_GCHAT_SOUND = "notifications_group_chat_sound";

        public const string AUDIO_PLAYER_LOOP = "audio_player_loop";

        public const string DEBUG_LOGS_CORE = "log_to_file_core";
        public const string DEBUG_LOGS_LP = "log_to_file_lp";
        public const string DEBUG_LOGS_BITMAPMANAGER = "log_to_file_bm";
        public const string DEBUG_LOGS_LNET = "log_to_file_lnet";
        public const string DEBUG_LOGS_MESSAGERENDERING = "log_to_file_msgui";

        public const string DEBUG_MARK_AS_READ_OFF = "no_mark_as_read";

        public const string DEBUG_FPS = "dbg_fps";
        public const string DEBUG_COUNTERS_CHAT = "dbg_counters_chat";
        public const string DEBUG_COUNTERS_RAM = "dbg_counters_ram";
        public const string DEBUG_CONTEXT_MENU = "dbg_dev_context_menu";
        public const string DEBUG_MSGESSAGES_LIST_VIRTUALIZATION = "dbg_msgl_virtualization";
        public const string DEBUG_GALLERY = "dbg_gallery";

        #endregion

        #region Settings with defaults

        public static bool SentViaEnter {
            get => Get(SEND_VIA_ENTER, true);
            set => Set(SEND_VIA_ENTER, value);
        }

        public static bool DontParseLinks {
            get => Get(DONT_PARSE_LINKS, false);
            set => Set(DONT_PARSE_LINKS, value);
        }

        public static bool DisableMentions {
            get => Get(DISABLE_MENTIONS, false);
            set => Set(DISABLE_MENTIONS, value);
        }

        public static bool SuggestStickers {
            get => Get(STICKERS_SUGGEST, true);
            set => Set(STICKERS_SUGGEST, value);
        }

        public static bool AnimateStickers {
            get => Get(STICKERS_ANIMATE, true);
            set => Set(STICKERS_ANIMATE, value);
        }

        // Appearance

        public static int AppTheme {
            get => Get(THEME, Constants.DefaultTheme);
            set => Set(THEME, value);
        }

        public static bool ChatItemMoreRows {
            get => Get(CHAT_ITEM_MORE_ROWS, false);
            set => Set(CHAT_ITEM_MORE_ROWS, value);
        }

        // Notifications

        public static bool NotificationsPrivate {
            get => Get(NOTIF_PRIVATE, true);
            set => Set(NOTIF_PRIVATE, value);
        }

        public static bool NotificationsPrivateSound {
            get => Get(NOTIF_PRIVATE_SOUND, true);
            set => Set(NOTIF_PRIVATE_SOUND, value);
        }

        public static bool NotificationsGroupChat {
            get => Get(NOTIF_GCHAT, true);
            set => Set(NOTIF_GCHAT, value);
        }

        public static bool NotificationsGroupChatSound {
            get => Get(NOTIF_GCHAT_SOUND, true);
            set => Set(NOTIF_GCHAT_SOUND, value);
        }

        public static bool AudioPlayerLoop {
            get => Get(AUDIO_PLAYER_LOOP, false);
            set => Set(AUDIO_PLAYER_LOOP, value);
        }

        // Debug

        public static bool EnableLogs {
            get => Get(DEBUG_LOGS_CORE, true);
            set => Set(DEBUG_LOGS_CORE, value);
        }

        public static bool EnableLongPollLogs {
            get => Get(DEBUG_LOGS_CORE, false);
            set => Set(DEBUG_LOGS_CORE, value);
        }

        public static bool BitmapManagerLogs {
            get => Get(DEBUG_LOGS_BITMAPMANAGER, false);
            set => Set(DEBUG_LOGS_BITMAPMANAGER, value);
        }

        public static bool LNetLogs {
            get => Get(DEBUG_LOGS_LNET, false);
            set => Set(DEBUG_LOGS_LNET, value);
        }

        public static bool MessageRenderingLogs {
            get => Get(DEBUG_LOGS_MESSAGERENDERING, false);
            set => Set(DEBUG_LOGS_MESSAGERENDERING, value);
        }

        public static bool ShowFPS {
            get => Get(DEBUG_FPS, false);
            set => Set(DEBUG_FPS, value);
        }

        public static bool ShowDebugCounters {
            get => Get(DEBUG_COUNTERS_CHAT, false);
            set => Set(DEBUG_COUNTERS_CHAT, value);
        }

        public static bool ShowRAMUsage {
            get => Get(DEBUG_COUNTERS_RAM, false);
            set => Set(DEBUG_COUNTERS_RAM, value);
        }

        public static bool ShowDevItemsInContextMenus {
            get => Get(DEBUG_CONTEXT_MENU, false);
            set => Set(DEBUG_CONTEXT_MENU, value);
        }

        public static bool DisableMarkingMessagesAsRead {
            get => Get(DEBUG_MARK_AS_READ_OFF, false);
            set => Set(DEBUG_MARK_AS_READ_OFF, value);
        }

        public static bool MessagesListVirtualization {
            get => Get(DEBUG_MSGESSAGES_LIST_VIRTUALIZATION, false);
            set => Set(DEBUG_MSGESSAGES_LIST_VIRTUALIZATION, value);
        }

        public static bool ShowDebugInfoInGallery {
            get => Get(DEBUG_GALLERY, false);
            set => Set(DEBUG_GALLERY, value);
        }

        #endregion
    }
}
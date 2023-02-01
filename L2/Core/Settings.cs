﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
            FilePath = Path.Combine(App.LocalDataPath, "settings.json");
            _file = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096);
            
            if (App.Platform != OSPlatform.OSX) {
                _file.Lock(0, 0);
            }

            byte[] fileBytes = new byte[_file.Length];
            _file.Read(fileBytes, 0, fileBytes.Length);

            UTF8Encoding enc = new UTF8Encoding(true);
            string content = enc.GetString(fileBytes);

            if (content.Length == 0) return;
            var json = JObject.Parse(content);
            foreach (var setting in json) {
                switch (setting.Value.Type) {
                    case JTokenType.String:
                        _settings.Add(setting.Key, setting.Value.Value<string>());
                        break;
                    case JTokenType.Integer:
                        _settings.Add(setting.Key, setting.Value.Value<int>());
                        break;
                    case JTokenType.Float:
                        _settings.Add(setting.Key, setting.Value.Value<double>());
                        break;
                    case JTokenType.Boolean:
                        _settings.Add(setting.Key, setting.Value.Value<bool>());
                        break;
                    case JTokenType.Array:
                        CheckJsonArray(setting.Key, setting.Value.Value<JArray>());
                        break;
                    // TODO: Date, TimeSpan, GUID, Uri
                }
            }
        }

        private static void CheckJsonArray(string key, JArray? jArray) {
            if (jArray == null || jArray.Count == 0) return;
            JTokenType type = jArray[0].Type;
            switch (type) {
                case JTokenType.String:
                    _settings.Add(key, jArray.ToObject<List<string>>());
                    break;
                case JTokenType.Integer:
                    _settings.Add(key, jArray.ToObject<List<int>>());
                    break;
                case JTokenType.Float:
                    _settings.Add(key, jArray.ToObject<List<double>>());
                    break;
            }
        }

        private static async void UpdateFile() {
            string content = JsonConvert.SerializeObject(_settings);
            byte[] bytes = Encoding.UTF8.GetBytes(content);

            _file.Position = 0;
            _file.SetLength(bytes.Length);
            await _file.WriteAsync(bytes);
            await _file.FlushAsync();
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
            if (_settings.ContainsKey(key)) {
                _settings[key] = value;
            } else {
                _settings.Add(key, value);
            }
        }

        #endregion

        #region Constants

        public const string TEST_STRING = "test_string";

        public const string VK_USER_ID = "user_id";
        public const string VK_TOKEN = "access_token";

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

        public const string DEBUG_LOGS_CORE = "log_to_file_core";
        public const string DEBUG_LOGS_LP = "log_to_file_lp";

        public const string DEBUG_FPS = "dbg_fps";
        public const string DEBUG_COUNTERS_CHAT = "dbg_counters_chat";
        public const string DEBUG_COUNTERS_RAM = "dbg_counters_chat";
        public const string DEBUG_CONTEXT_MENU = "dbg_dev_context_menu";

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

        // Debug

        public static bool EnableLogs {
            get => Get(DEBUG_LOGS_CORE, true);
            set => Set(DEBUG_LOGS_CORE, value);
        }

        public static bool EnableLongPollLogs {
            get => Get(DEBUG_LOGS_CORE, false);
            set => Set(DEBUG_LOGS_CORE, value);
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

        #endregion
    }
}
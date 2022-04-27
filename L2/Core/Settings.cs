using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ELOR.Laney.Core {
    public static class Settings {
        private static Dictionary<string, object> _settings = new Dictionary<string, object>();
        private static FileStream _file;
        public static string FilePath { get; private set; }

        #region Initialization

        public static void Initialize() {
            FilePath = Path.Combine(App.LocalDataPath, "settings.json");
            _file = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096);
            _file.Lock(0, 0);
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
            // SettingChanged event;
        }

        public static void SetBatch(Dictionary<string, object> settings) {
            foreach (var setting in settings) {
                AddOrReplace(setting.Key, setting.Value);
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
        public const string TEST_INT = "test_int";
        public const string TEST_DOUBLE = "test_double";
        public const string TEST_BOOL = "test_bool";
        public const string TEST_ARRAY_STRING = "test_array_string";
        public const string TEST_ARRAY_INT = "test_array_int";
        public const string TEST_ARRAY_DOUBLE = "test_array_double";

        public const string VK_USER_ID = "user_id";
        public const string VK_TOKEN = "access_token";

        #endregion
    }
}
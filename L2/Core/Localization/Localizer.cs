using Avalonia;
using Avalonia.Platform;
using ELOR.VKAPILib.Objects;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using ELOR.Laney.DataModels;

namespace ELOR.Laney.Core.Localization {
    public class Localizer : INotifyPropertyChanged {
        private const string IndexerName = "Item";
        private const string IndexerArrayName = "Item[]";
        private Dictionary<string, string> m_Strings;

        public static readonly ObservableCollection<TwoStringTuple> SupportedLanguages = new ObservableCollection<TwoStringTuple> {
            new TwoStringTuple("en-US", "English"),
            new TwoStringTuple("ru-RU", "Русский"),
            new TwoStringTuple("uk-UA", "Українська"),
        };

        public Localizer() { }

        public bool LoadLanguage(string language) {
            Language = language;
            m_Strings = new Dictionary<string, string>();

            Uri uri = new Uri($"avares://laney/Assets/i18n/{language}.json");
            if (AssetLoader.Exists(uri)) {
                using (StreamReader sr = new StreamReader(AssetLoader.Open(uri), Encoding.UTF8)) {
                    string str = sr.ReadToEnd();
                    JsonDocument json = JsonDocument.Parse(str);
                    foreach (var node in json.RootElement.EnumerateObject()) {
                        m_Strings.Add(node.Name, node.Value.GetString());
                    }
                }
                Invalidate();

                return true;
            }
            return false;
        }

        public string Language { get; private set; }

        public bool ContainsKey(string key) {
            return m_Strings != null && m_Strings.ContainsKey(key);
        }

        public string this[string key] {
            get {
                string res;
                bool uppercase = false;
                if (key[0] == '!') {
                    uppercase = true;
                    key = key.Substring(1);
                }
                if (m_Strings != null && m_Strings.TryGetValue(key, out res)) {
                    if (uppercase) res = res.ToUpper();
                    return res.Replace("\\n", "\n");
                }

                return $"%{key}%";
            }
        }

        public string Get(string key, Sex sex) {
            string suffix = sex == Sex.Female ? "_f" : "_m";
            return this[$"{key}{suffix}"];
        }

        public string GetFormatted(string key, params object[] args) {
            return String.Format(this[key], args);
        }

        public string GetFormatted(Sex sex, string key, params object[] args) {
            return String.Format(Get(key, sex), args);
        }

        private string GetDeclensionSuffix(decimal num) {
            int number = (int)num % 100;
            if (number >= 11 && number <= 19) {
                return "_plu";
            }

            var i = number % 10;
            switch (i) {
                case 1:
                    return "_nom";
                case 2:
                case 3:
                case 4:
                    return "_gen";
                default:
                    return "_plu";
            }
        }

        public string GetDeclension(decimal num, string str) {
            return this[$"{str}{GetDeclensionSuffix(num)}"];
        }

        // Example: 1 message, 2 messages
        public string GetDeclensionFormatted(decimal num, string key, params object[] args) {
            var list = args.ToList();
            list.Insert(0, num);
            return String.Format(GetDeclension(num, key), list.ToArray());
        }

        // Example: Message, 2 messages
        public string GetDeclensionFormatted2(decimal num, string key, params object[] args) {
            if (num != 1) {
                var list = args.ToList();
                list.Insert(0, num);
                return String.Format(GetDeclension(num, key), list.ToArray());
            } else {
                return this[key];
            }
        }

        public static Localizer Instance { get; private set; } = new Localizer();
        public event PropertyChangedEventHandler PropertyChanged;

        public void Invalidate() {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
        }
    }
}
using Avalonia;
using Avalonia.Platform;
using ELOR.VKAPILib.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace ELOR.Laney.Core.Localization {
    public class Localizer : INotifyPropertyChanged {
        private const string IndexerName = "Item";
        private const string IndexerArrayName = "Item[]";
        private Dictionary<string, string> m_Strings;

        public static readonly ObservableCollection<Tuple<string, string>> SupportedLanguages = new ObservableCollection<Tuple<string, string>> {
            new Tuple<string, string>("en-US", "English"),
            new Tuple<string, string>("ru-RU", "Русский"),
            new Tuple<string, string>("uk-UA", "Українська"),
        };

        public Localizer() { }

        public bool LoadLanguage(string language) {
            Language = language;

            Uri uri = new Uri($"avares://laney/Assets/i18n/{language}.json");
            if (AssetLoader.Exists(uri)) {
                using (StreamReader sr = new StreamReader(AssetLoader.Open(uri), Encoding.UTF8)) {
                    m_Strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
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
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ELOR.Laney.DataModels;
using System.Globalization;

namespace ELOR.Laney.Core.Localization {
    public class Localizer {
        public static readonly ObservableCollection<TwoStringTuple> SupportedLanguages = new ObservableCollection<TwoStringTuple> {
            new TwoStringTuple("en-US", "English"),
            new TwoStringTuple("ru-RU", "Русский"),
            new TwoStringTuple("uk-UA", "Українська"),
        };

        public static void LoadLanguage(string language) {
            Assets.i18n.Resources.Culture = new CultureInfo(language);
        }

        public static string Get(string key) {
            string value = Assets.i18n.Resources.ResourceManager.GetString(key, Assets.i18n.Resources.Culture);
            return string.IsNullOrEmpty(value) ? $"%{key}%" : value;
        }

        public static string Get(string key, Sex sex) {
            string suffix = sex == Sex.Female ? "_f" : "_m";
            string fkey = $"{key}{suffix}";
            string value = Assets.i18n.Resources.ResourceManager.GetString(fkey, Assets.i18n.Resources.Culture);
            return string.IsNullOrEmpty(value) ? $"%{fkey}%" : value;
        }

        public static string GetFormatted(string key, params object[] args) {
            string value = Assets.i18n.Resources.ResourceManager.GetString(key, Assets.i18n.Resources.Culture);
            return string.IsNullOrEmpty(value) ? $"%{key}%" : String.Format(value, args);
        }

        public static string GetFormatted(Sex sex, string key, params object[] args) {
            return String.Format(Get(key, sex), args);
        }

        private static string GetDeclensionSuffix(decimal num) {
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

        public static string GetDeclension(decimal num, string str) {
            var fkey = $"{str}{GetDeclensionSuffix(num)}";
            string value = Assets.i18n.Resources.ResourceManager.GetString(fkey, Assets.i18n.Resources.Culture);
            return string.IsNullOrEmpty(value) ? $"%{fkey}%" : value;
        }

        // Example: 1 message, 2 messages
        public static string GetDeclensionFormatted(decimal num, string key, params object[] args) {
            var list = args.ToList();
            list.Insert(0, num);
            return String.Format(GetDeclension(num, key), list.ToArray());
        }

        // Example: Message, 2 messages
        public static string GetDeclensionFormatted2(decimal num, string key, params object[] args) {
            if (num != 1) {
                var list = args.ToList();
                list.Insert(0, num);
                return String.Format(GetDeclension(num, key), list.ToArray());
            } else {
                string value = Assets.i18n.Resources.ResourceManager.GetString(key);
                return string.IsNullOrEmpty(value) ? $"%{key}%" : value;
            }
        }
    }
}
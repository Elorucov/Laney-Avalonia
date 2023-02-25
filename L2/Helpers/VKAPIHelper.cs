using Avalonia.Media;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels.Controls;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using VKUI.Controls;

namespace ELOR.Laney.Helpers {
    public static class VKAPIHelper {
        public static readonly List<string> Fields = new List<string>() { "photo_200", "photo_100", "photo_50",
            "ban_info", "blacklisted", "blacklisted_by_me", "can_message", "can_write_private_message", "friend_status",
            "is_messages_blocked", "online_info", "domain", "verified", "sex", "activity",
            "first_name_gen", "first_name_dat", "first_name_acc", "first_name_ins", "first_name_abl",
            "last_name_gen", "last_name_dat", "last_name_acc", "last_name_ins", "last_name_abl", "photo_avg_color"
        };

        public static readonly List<string> UserFields = new List<string>() { "photo_200", "photo_100", "photo_50", "sex",
            "blacklisted", "blacklisted_by_me", "can_write_private_message", "friend_status", "online_info", "domain", "verified", "photo_avg_color"
        };

        public static readonly List<string> GroupFields = new List<string>() { "photo_200", "photo_100", "photo_50", "description",
            "ban_info", "can_message", "is_messages_blocked", "domain", "activity", "status", "verified", "activity", "photo_avg_color"
        };

        #region Errors

        public static string GetUnderstandableErrorMessage(int code) {
            string key = $"api_error_{code}";
            if (Localizer.Instance.ContainsKey(key)) {
                return Localizer.Instance[$"api_error_{code}"];
            } else {
                return string.Empty;
            }
        }

        public static string GetUnderstandableErrorMessage(APIException ex) {
            string uem = GetUnderstandableErrorMessage(ex.Code);
            return string.IsNullOrEmpty(uem) ? $"{ex.Message} ({ex.Code})" : uem;
        }

        public static string GetUnderstandableErrorMessage(int code, string message) {
            string uem = GetUnderstandableErrorMessage(code);
            return string.IsNullOrEmpty(uem) ? $"{message} ({code})" : uem;
        }

        #endregion

        public static string GetOnlineInfo(UserOnlineInfo info, Sex sex) {
            if (info != null) {
                if (info.Visible) {
                    if (info.IsOnline) {
                        return Localizer.Instance["online"];
                    } else {
                        return info.LastSeen.Year >= 2006 ?
                            Localizer.Instance.GetFormatted(sex, "offline_last_seen", info.LastSeen.ToHumanizedString()) :
                            Localizer.Instance["offline"]; // у забаненных/удалённых возвращается 0 в unixtime. 
                    }
                } else {
                    switch (info.Status) {
                        case UserOnlineStatus.Recently: return Localizer.Instance.Get("offline_recently", sex);
                        case UserOnlineStatus.LastWeek: return Localizer.Instance.Get("offline_last_week", sex);
                        case UserOnlineStatus.LastMonth: return Localizer.Instance.Get("offline_last_month", sex);
                        case UserOnlineStatus.LongAgo: return Localizer.Instance.Get("offline_long_ago", sex);
                    }
                }
            }
            return Localizer.Instance["offline"];
        }

        public static string GetSenderNameShort(MessageViewModel msg) {
            if (msg.Action != null) return string.Empty;
            string sender = string.Empty;

            if (msg.SenderId == VKSession.Main.UserId && msg.PeerId != VKSession.Main.UserId) {
                sender = Localizer.Instance["you"];
            } else if (msg.PeerId > 2000000000) {
                sender = CacheManager.GetNameOnly(msg.SenderId, true);
            }

            if (!string.IsNullOrEmpty(sender)) sender += ": ";
            return sender;
        }

        public static SolidColorBrush GetDocumentIconBackground(DocumentType type) {
            switch (type) {
                case DocumentType.Text: return new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                case DocumentType.Archive: return new SolidColorBrush(Color.FromArgb(255, 118, 185, 121));
                case DocumentType.GIF: return new SolidColorBrush(Color.FromArgb(255, 119, 165, 214));
                case DocumentType.Image: return new SolidColorBrush(Color.FromArgb(255, 119, 165, 214));
                case DocumentType.Audio: return new SolidColorBrush(Color.FromArgb(255, 186, 104, 200));
                case DocumentType.Video: return new SolidColorBrush(Color.FromArgb(255, 229, 115, 155));
                case DocumentType.EBook: return new SolidColorBrush(Color.FromArgb(255, 255, 174, 56));
                default: return new SolidColorBrush(Color.FromArgb(255, 119, 165, 214));
            }
        }

        public static string GetDocumentIcon(DocumentType type) {
            switch (type) {
                case DocumentType.Text: return VKIconNames.Icon28ArticleOutline;
                case DocumentType.Archive: return VKIconNames.Icon28ZipOutline;
                case DocumentType.GIF: return VKIconNames.Icon28PictureOutline;
                case DocumentType.Image: return VKIconNames.Icon28PictureOutline;
                case DocumentType.Audio: return VKIconNames.Icon28MusicOutline;
                case DocumentType.Video: return VKIconNames.Icon28VideoOutline;
                case DocumentType.EBook: return VKIconNames.Icon28ArticleOutline;
                default: return VKIconNames.Icon28DocumentOutline;
            }
        }

        public static string GetNormalizedBirthDate(string bdate) {
            string[] a = bdate.Split('.');
            DateTime dt = a.Length == 3 ? new DateTime(Int32.Parse(a[2]), Int32.Parse(a[1]), Int32.Parse(a[0])) : new DateTime(1604, Int32.Parse(a[1]), Int32.Parse(a[0]));
            return a.Length == 3 ? $"{dt.ToString("M")} {dt.Year}" : dt.ToString("M");
        }

        public static string GetNameOrDefaultString(int ownerId, string defaultStr = null) {
            if (!String.IsNullOrEmpty(defaultStr)) return defaultStr;
            string from = "";
            if (ownerId > 0) {
                User u = CacheManager.GetUser(ownerId);
                from = u != null ? $"{Localizer.Instance["from"]} {u.FirstNameGen} {u.LastNameGen}" : "";
            } else if (ownerId < 0) {
                Group u = CacheManager.GetGroup(ownerId);
                from = u != null ? $"{Localizer.Instance["from"]} \"{u.Name}\"" : "";
            }
            return from;
        }
    }
}
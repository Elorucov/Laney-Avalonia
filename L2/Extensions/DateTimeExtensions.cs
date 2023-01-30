using ELOR.Laney.Core.Localization;
using System;

namespace ELOR.Laney.Extensions {
    public static class DateTimeExtensions {
        // 13 с, 5 мин, 7 ч, 12 д, 2 м, 3 г.
        public static string ToDiffStringShort(this DateTime target) {
            string text = String.Empty;
            DateTime current = DateTime.Now;

            var diff = current - target;
            var mdiff = ((current.Year - target.Year) * 12) + current.Month - target.Month;
            var md = current.AddMonths(-1);

            if (md.Day < target.Day) {
                mdiff--;
            } else if (md.Day == target.Day && md.Hour < target.Hour) {
                mdiff--;
            }

            if (mdiff >= 12) {
                int years = mdiff / 12;
                text = Localizer.Instance.GetFormatted("years_short", years.ToString());
            } else if (mdiff > 0) {
                text = Localizer.Instance.GetFormatted("months_short", mdiff.ToString());
            } else if (diff.TotalDays >= 1) {
                text = Localizer.Instance.GetFormatted("days_short", diff.Days.ToString());
            } else if (diff.TotalHours >= 1) {
                text = Localizer.Instance.GetFormatted("hours_short", diff.Hours.ToString());
            } else if (diff.TotalMinutes >= 1) {
                text = Localizer.Instance.GetFormatted("minutes_short", diff.Minutes.ToString());
            }
            //else if (diff.TotalSeconds >= 10) {
            //    text = $"{diff.Seconds} s";
            //}

            return text;
        }

        public static string ToHumanizedDateString(this DateTime target) {
            string text = String.Empty;
            DateTime current = DateTime.Now;

            if (current.Date == target.Date) {
                text = Localizer.Instance["today"];
            } else if (current.Date.AddDays(-1) == target.Date) {
                text = Localizer.Instance["yesterday"];
            } else if (current.Date.AddDays(1) == target.Date) {
                text = Localizer.Instance["tomorrow"];
            } else {
                if (current.Year == target.Year) {
                    text = target.ToString("m");
                } else {
                    text = $"{target.ToString("m")} {target.Year}";
                }
            }

            return text;
        }

        public static string ToHumanizedString(this DateTime target, bool noAt = false) {
            DateTime current = DateTime.Now;
            string text = String.Empty;
            if (current.Date != target.Date) text = target.ToHumanizedDateString() + " ";
            string at = noAt && text.Length == 0 ? "" : $"{Localizer.Instance["time_at"]} ";
            text += $"{at}{target.ToString("H:mm")}";
            return text;
        }

        public static string ToHumanizedTimeOrDateString(this DateTime target) {
            DateTime current = DateTime.Now;
            string text = String.Empty;
            if (current.Date != target.Date) {
                return target.ToHumanizedDateString();
            } else {
                return target.ToString("H:mm");
            }
        }
    }
}
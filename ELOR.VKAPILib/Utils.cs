using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib {
    internal static class Utils {
        public static string ToEnumMemberAttribute(this Enum @enum) {
            var t = @enum.GetType().GetTypeInfo();
            EnumMemberAttribute ema = null;
            t.DeclaredMembers.ToList().ForEach(k => {
                if(k.Name == @enum.ToString()) {
                    ema = (EnumMemberAttribute)k.GetCustomAttribute(typeof(EnumMemberAttribute));
                }
            });
            if (ema == null) return @enum.ToString();
            return ema.Value;
        }

        internal static bool IsNullOrEmpty(this List<int> list) {
            return list == null || list.Count == 0;
        }

        internal static bool IsNullOrEmpty(this List<string> list) {
            return list == null || list.Count == 0;
        }

        internal static string Combine(this List<int> items, char sym = ',') {
            return String.Join(sym.ToString(), items);
        }

        internal static string Combine(this List<string> items, char sym = ',') {
            return String.Join(sym.ToString(), items);
        }

        internal static string ToVKFormat(this DateTime d) {
            string ds = d.Day <= 9 ? $"0{d.Day}" : d.Day.ToString();
            string ms = d.Month <= 9 ? $"0{d.Month}" : d.Month.ToString();
            return $"{ds}{ms}{d.Year}";
        }
    }
}

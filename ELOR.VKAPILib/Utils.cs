using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace ELOR.VKAPILib {
    internal static class Utils {
        public static string ToEnumMemberAttribute(this Enum @enum) {
            var t = @enum.GetType().GetTypeInfo();
            EnumMemberAttribute ema = null;
            t.DeclaredMembers.ToList().ForEach(k => {
                if (k.Name == @enum.ToString()) {
                    ema = (EnumMemberAttribute)k.GetCustomAttribute(typeof(EnumMemberAttribute));
                }
            });
            if (ema == null) return @enum.ToString();
            return ema.Value;
        }

        internal static bool IsNullOrEmpty(this List<int> list) {
            return list == null || list.Count == 0;
        }

        internal static bool IsNullOrEmpty(this List<long> list) {
            return list == null || list.Count == 0;
        }

        internal static bool IsNullOrEmpty(this List<string> list) {
            return list == null || list.Count == 0;
        }

        internal static string Combine(this List<int> items, char sym = ',') {
            return String.Join(sym.ToString(), items);
        }

        internal static string Combine(this List<long> items, char sym = ',') {
            return String.Join(sym.ToString(), items);
        }

        internal static string Combine(this List<string> items, char sym = ',') {
            return String.Join(sym.ToString(), items);
        }

        internal static string Combine<TKey, TValue>(this List<KeyValuePair<TKey, TValue>> items, char sym = ',') {
            StringBuilder sb = new StringBuilder();
            foreach (var pair in CollectionsMarshal.AsSpan(items)) {
                sb.Append($"{pair.Key.ToString()}_{pair.Value.ToString()}");
                sb.Append(sym);
            }
            sb.Remove(sb.Length - 2, 1);
            string result = sb.ToString();
            return result;
        }

        internal static string ToVKFormat(this DateTime d) {
            string ds = d.Day <= 9 ? $"0{d.Day}" : d.Day.ToString();
            string ms = d.Month <= 9 ? $"0{d.Month}" : d.Month.ToString();
            return $"{ds}{ms}{d.Year}";
        }
    }
}

using ELOR.Laney.DataModels;
using NeoSmart.Unicode;
using System.Collections.ObjectModel;
using System.Linq;

namespace ELOR.Laney.Core {

    // Название такое чтобы не конфликтовался с классом NeoSmart.Unicode.Emoji.
    public class L2Emoji {
        static ObservableCollection<EmojiGroup> _cached;
        public static ObservableCollection<EmojiGroup> All { get => GetEmojis(); }

        private static ObservableCollection<EmojiGroup> GetEmojis() {
            if (_cached != null) return _cached;

            var flagsEmojis = Emoji.All.Where(e => e.Group == "Flags");
            var basicEmojis = Emoji.Basic;
            basicEmojis.RemoveWhere(e => e.Group == "Flags");
            foreach (var emoji in flagsEmojis) {
                basicEmojis.Add(emoji);
            }

            _cached = new ObservableCollection<EmojiGroup>(basicEmojis.GroupBy(e => e.Group).Select(g => new EmojiGroup(g)));
            basicEmojis.Clear();
            return _cached;
        }
    }
}
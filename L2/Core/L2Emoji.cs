using NeoSmart.Unicode;
using System.Collections.ObjectModel;
using System.Linq;

namespace ELOR.Laney.Core {

    // Название такое чтобы не конфликтовался с классом NeoSmart.Unicode.Emoji.
    public class L2Emoji {
        static ObservableCollection<IGrouping<string, SingleEmoji>> _cached;
        public static ObservableCollection<IGrouping<string, SingleEmoji>> All { get => GetEmojis(); }

        private static ObservableCollection<IGrouping<string, SingleEmoji>> GetEmojis() {
            if (_cached != null) return _cached;

            var flagsEmojis = Emoji.All.Where(e => e.Group == "Flags");
            var basicEmojis = Emoji.Basic;
            basicEmojis.RemoveWhere(e => e.Group == "Flags");
            foreach (var emoji in flagsEmojis) {
                basicEmojis.Add(emoji);
            }

            _cached = new ObservableCollection<IGrouping<string, SingleEmoji>>(basicEmojis.GroupBy(e => e.Group));
            basicEmojis.Clear();
            return _cached;
        }
    }
}
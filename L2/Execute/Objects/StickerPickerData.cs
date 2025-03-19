using ELOR.VKAPILib.Objects;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ELOR.Laney.Execute.Objects {
    public class StickerPickerData {
        public StickerPickerData() { }

        [JsonPropertyName("recent_stickers")]
        public List<Sticker> RecentStickers { get; set; }

        [JsonPropertyName("favorite_stickers")]
        public List<Sticker> FavoriteStickers { get; set; }
    }
}

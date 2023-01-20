using ELOR.VKAPILib.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ELOR.Laney.Execute.Objects {
    public class StickerPickerData {
        [JsonProperty("recent_stickers")]
        public List<Sticker> RecentStickers { get; set; }

        [JsonProperty("favorite_stickers")]
        public List<Sticker> FavoriteStickers { get; set; }
    }
}

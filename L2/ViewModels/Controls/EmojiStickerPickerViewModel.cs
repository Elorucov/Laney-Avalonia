using ELOR.Laney.Core;
using ELOR.Laney.DataModels;
using ELOR.Laney.Execute;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VKUI.Controls;

namespace ELOR.Laney.ViewModels.Controls {
    public class EmojiStickerPickerViewModel : CommonViewModel {
        private ObservableCollection<TabItem<object>> _tabs = new ObservableCollection<TabItem<object>>();
        private TabItem<object> _selectedTab;

        public ObservableCollection<TabItem<object>> Tabs { get { return _tabs; } set { _tabs = value; OnPropertyChanged(); } }
        public TabItem<object> SelectedTab { get { return _selectedTab; } set { _selectedTab = value; OnPropertyChanged(); } }

        private VKSession session;

        public EmojiStickerPickerViewModel(VKSession session) {
            this.session = session;
            TabItem<object> emojiTab = new TabItem<object>(Assets.i18n.Resources.emoji, L2Emoji.All, VKIconNames.Icon20SmileOutline);
            Tabs.Add(emojiTab);
            SelectedTab = Tabs.FirstOrDefault();

            LoadStickerPacks();
        }

        // TODO: кэш
        private async void LoadStickerPacks() {
            if (DemoMode.IsEnabled) return;
            try {
                var req1 = await session.API.GetRecentStickersAndGraffitiesAsync();
                TabItem<object> favTab = new TabItem<object>(Assets.i18n.Resources.favorites, new ObservableCollection<Sticker>(req1.FavoriteStickers), VKIconNames.Icon20FavoriteOutline);
                TabItem<object> recentTab = new TabItem<object>(Assets.i18n.Resources.recent, new ObservableCollection<Sticker>(req1.RecentStickers), VKIconNames.Icon20RecentOutline);
                Tabs.Add(favTab);
                Tabs.Add(recentTab);

                var req2 = await session.API.Store.GetProductsAsync("stickers", new List<string> { "active" }, true);
                foreach (var product in req2.Items) {
                    TabItem<object> spTab = new TabItem<object>(product.Title, new ObservableCollection<Sticker>(product.Stickers), image: product.Previews.FirstOrDefault().Uri);
                    Tabs.Add(spTab);
                }
                Log.Information($"EmojiStickerPickerVM: loaded {req2.Items.Count} sticker packs");
            } catch (Exception ex) {
                Log.Error(ex, "EmojiStickerPickerVM: Cannot get stickers!");
                // TODO: snackbar.
            }
        }
    }
}
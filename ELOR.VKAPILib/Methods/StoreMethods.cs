using ELOR.VKAPILib.Objects;

namespace ELOR.VKAPILib.Methods {

    public class StoreMethods : MethodsSectionBase {
        internal StoreMethods(VKAPI api) : base(api) { }

        public async Task<StoreProductsList> GetProductsAsync(string type, List<string> filters, bool extended) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "type", type },
                { "filters", String.Join(",", filters) }
            };
            if (extended) parameters.Add("extended", "1");
            return await API.CallMethodAsync<StoreProductsList>("store.getProducts", parameters);
        }

        public async Task<StickersKeywordsResponse> GetStickersKeywordsAsync(int chunk, string hash, bool allProducts) {
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "aliases", "1" },
                { "need_stickers", "1" }
            };
            if (allProducts) parameters.Add("all_products", "1");
            parameters.Add("chunk", chunk.ToString());
            if (!string.IsNullOrEmpty(hash)) parameters.Add("hash", hash);
            return await API.CallMethodAsync<StickersKeywordsResponse>("store.getStickersKeywords", parameters);
        }
    }
}
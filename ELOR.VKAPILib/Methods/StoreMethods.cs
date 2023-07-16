using ELOR.VKAPILib.Objects;

namespace ELOR.VKAPILib.Methods {

    public class StoreMethods : MethodsSectionBase {
        internal StoreMethods(VKAPI api) : base(api) { }

        public async Task<StoreProductsList> GetProductsAsync(string type, List<string> filters, bool extended) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("type", type);
            parameters.Add("filters", String.Join(",", filters));
            if (extended) parameters.Add("extended", "1");
            return await API.CallMethodAsync<StoreProductsList>("store.getProducts", parameters);
        }
    }
}
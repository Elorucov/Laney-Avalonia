using ELOR.VKAPILib.Attributes;
using ELOR.VKAPILib.Objects;

namespace ELOR.VKAPILib.Methods {

    [Section("store")]
    public class StoreMethods : MethodsSectionBase {
        internal StoreMethods(VKAPI api) : base(api) { }

        [Method("getProducts")]
        public async Task<VKList<StoreProduct>> GetProductsAsync(string type, List<string> filters, bool extended) {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("type", type);
            parameters.Add("filters", String.Join(",", filters));
            if (extended) parameters.Add("extended", "1");
            return await API.CallMethodAsync<VKList<StoreProduct>>(this, parameters);
        }
    }
}

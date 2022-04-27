using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.VKAPILib.Objects {
    public class VKList<T> {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("items")]
        public List<T> Items { get; set; }

        [JsonProperty("profiles")]
        public List<User> Profiles { get; set; }

        [JsonProperty("groups")]
        public List<Group> Groups { get; set; }

        [JsonProperty("conversations")]
        public List<Conversation> Conversations { get; set; }
    }
}

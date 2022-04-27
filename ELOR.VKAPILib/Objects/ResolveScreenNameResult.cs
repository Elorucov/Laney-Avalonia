using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ELOR.VKAPILib.Objects {
    public enum ScreenNameType {
        [EnumMember(Value = "user")]
        User,

        [EnumMember(Value = "group")]
        Group,

        [EnumMember(Value = "application")]
        Application,

        [EnumMember(Value = "vk_app")]
        VkApp,

        [EnumMember(Value = "community_application")]
        CommunityApplication,

        [EnumMember(Value = "internal_vkui")]
        InternalVKUI
    }

    public class ResolveScreenNameResult {
        [JsonProperty("object_id")]
        public int ObjectId { get; internal set; }

        [JsonProperty("type")]
        public ScreenNameType Type { get; internal set; }
    }
}

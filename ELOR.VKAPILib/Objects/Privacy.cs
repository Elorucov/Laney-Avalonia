using ELOR.VKAPILib.Attributes;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {

    public enum PrivacySettingValueType {
        [EnumMember(Value = "binary")]
        Binary,

        [EnumMember(Value = "list")]
        List
    }

    public class PrivacySettingValueOwners {
        public PrivacySettingValueOwners() { }

        [JsonPropertyName("allowed")]
        public List<int> Allowed { get; set; }
    }

    public class PrivacySettingValue {
        public PrivacySettingValue() { }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("owners")]
        public PrivacySettingValueOwners Owners { get; set; }

        [JsonPropertyName("is_enabled")]
        public bool IsEnabled { get; set; }
    }

    public class PrivacySetting {
        public PrivacySetting() { }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("value")]
        public PrivacySettingValue Value { get; set; }

        [JsonPropertyName("section")]
        public string Section { get; set; }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverterEx<PrivacySettingValueType>))]
        public PrivacySettingValueType Type { get; set; }

        [JsonPropertyName("supported_categories")]
        public List<string> SupportedCategories { get; set; }
    }

    public class PrivacySection {
        public PrivacySection() { }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class PrivacyCategory {
        public PrivacyCategory() { }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }

    public class PrivacyResponse {
        public PrivacyResponse() { }

        [JsonPropertyName("settings")]
        public List<PrivacySetting> Settings { get; set; }

        [JsonPropertyName("sections")]
        public List<PrivacySection> Sections { get; set; }

        [JsonPropertyName("supported_categories")]
        public List<PrivacyCategory> SupportedCategories { get; set; }
    }
}
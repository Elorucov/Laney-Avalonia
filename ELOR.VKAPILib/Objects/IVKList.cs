using ELOR.VKAPILib.Objects.Messages;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib.Objects {
    // Ради AOT и Json source generation пришлось отказаться от суперкрутого класса VKList<T>...

    public interface IVKList<T> {
        int Count { get; set; }
        List<T> Items { get; set; }
        List<User> Profiles { get; set; }
        List<Group> Groups { get; set; }
    }

    public class IntList : IVKList<int> {
        public IntList() { }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<int> Items { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }
    }

    public class LongList : IVKList<long> {
        public LongList() { }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<long> Items { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }
    }

    public class AppsList : IVKList<App> {
        public AppsList() { }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<App> Items { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }
    }

    public class DocumentsList : IVKList<Document> {
        public DocumentsList() { }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<Document> Items { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }
    }

    public class UsersList {
        public UsersList() { }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<User> Items { get; set; }
    }

    public class GroupsList {
        public GroupsList() { }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<Group> Items { get; set; }
    }

    public class ImportantMessagesResponse {
        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }

        [JsonPropertyName("conversations")]
        public List<Conversation> Conversations { get; set; }

        [JsonPropertyName("messages")]
        public MessagesList Messages { get; set; }
    }

    public class MessagesList : IVKList<Message> {
        public MessagesList() { }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<Message> Items { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }

        [JsonPropertyName("conversations")]
        public List<Conversation> Conversations { get; set; }
    }

    public class ConversationsList : IVKList<Conversation> {
        public ConversationsList() { }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<Conversation> Items { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }
    }

    public class MessageTemplatesList {
        public MessageTemplatesList() { }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<MessageTemplate> Items { get; set; }

        [JsonPropertyName("group_id")]
        public long GroupId { get; set; } // needed for some "execute" methods.
    }

    public class PhotosList : IVKList<Photo> {
        public PhotosList() { }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<Photo> Items { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }
    }

    public class StoreProductsList {
        public StoreProductsList() { }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<StoreProduct> Items { get; set; }
    }

    public class VideosList : IVKList<Video> {
        public VideosList() { }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<Video> Items { get; set; }

        [JsonPropertyName("profiles")]
        public List<User> Profiles { get; set; }

        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }
    }
}
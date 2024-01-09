﻿using ELOR.VKAPILib.Objects;
using ELOR.VKAPILib.Objects.Auth;
using ELOR.VKAPILib.Objects.Messages;
using ELOR.VKAPILib.Objects.Upload;
using System.Text.Json.Serialization;

namespace ELOR.VKAPILib {
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(List<int>))]
    [JsonSerializable(typeof(APIException))]
    [JsonSerializable(typeof(App))]
    [JsonSerializable(typeof(AppsList))]
    [JsonSerializable(typeof(BotKeyboard))]
    [JsonSerializable(typeof(ConversationsList))]
    [JsonSerializable(typeof(DocumentsList))]
    [JsonSerializable(typeof(ImportantMessagesResponse))]
    [JsonSerializable(typeof(LongList))]
    [JsonSerializable(typeof(MarkAsImportantResponse))]
    [JsonSerializable(typeof(MessagesList))]
    [JsonSerializable(typeof(MessageSendResponse))]
    [JsonSerializable(typeof(MessageTemplatesList))]
    [JsonSerializable(typeof(PhotosList))]
    [JsonSerializable(typeof(UsersList))]
    [JsonSerializable(typeof(VideosList))]
    [JsonSerializable(typeof(AddTemplateResponse))]
    [JsonSerializable(typeof(ChatLink))]
    [JsonSerializable(typeof(ChatPreviewResponse))]
    [JsonSerializable(typeof(CreateChatResponse))]
    [JsonSerializable(typeof(ConversationAttachmentsResponse))]
    [JsonSerializable(typeof(ConversationsResponse))]
    [JsonSerializable(typeof(Dictionary<string, int>))]
    [JsonSerializable(typeof(DeleteConversationResponse))]
    [JsonSerializable(typeof(DocumentUploadResult))]
    [JsonSerializable(typeof(DocumentSaveResult))]
    [JsonSerializable(typeof(GroupsList))]
    [JsonSerializable(typeof(IsAllowedResponse))]
    [JsonSerializable(typeof(JoinChatResponse))]
    [JsonSerializable(typeof(LongPollHistoryResponse))]
    [JsonSerializable(typeof(LongPollServerInfo))]
    [JsonSerializable(typeof(List<MessageDeleteResponse>))]
    [JsonSerializable(typeof(List<PhotoSaveResult>))]
    [JsonSerializable(typeof(PhotoUploadResult))]
    [JsonSerializable(typeof(PhotoUploadServer))]
    [JsonSerializable(typeof(PhotoSaveResult))]
    [JsonSerializable(typeof(Poll))]
    [JsonSerializable(typeof(List<PollBackground>))]
    [JsonSerializable(typeof(PrivacyResponse))]
    [JsonSerializable(typeof(PrivacySettingValue))]
    [JsonSerializable(typeof(QueueSubscribeResponse))]
    [JsonSerializable(typeof(ResolveScreenNameResult))]
    [JsonSerializable(typeof(SetChatPhotoResponse))]
    [JsonSerializable(typeof(StickersKeywordsResponse))]
    [JsonSerializable(typeof(StoreProductsList))]
    [JsonSerializable(typeof(User))]
    [JsonSerializable(typeof(List<User>))]
    [JsonSerializable(typeof(VideoUploadResult))]
    [JsonSerializable(typeof(VideoUploadServer))]
    [JsonSerializable(typeof(VkUploadServer))]
    [JsonSerializable(typeof(List<Attachment>))]
    [JsonSerializable(typeof(OauthResponse))]
    [JsonSerializable(typeof(DirectAuthResponse))]
    public partial class BuildInJsonContext : JsonSerializerContext { }
}
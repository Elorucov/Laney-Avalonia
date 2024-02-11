using Avalonia.Threading;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.DataModels;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using static System.Collections.Specialized.BitVector32;

namespace ELOR.Laney.ViewModels.Controls {
    public enum MessageVMState {
        Loading, Unread, Read, Deleted, Failed
    }

    public enum MessageVMSenderType {
        Unknown, User, Group
    }

    public enum MessageUIType {
        Empty, Standart, Complex, SingleImage, Story, Sticker, StoryWithSticker, Graffiti, Gift
    }

    public sealed class MessageViewModel : ViewModelBase, IComparable {
        private VKSession session;

        private int _id;
        private long _peerId;
        private int _randomId;
        private int _conversationMessageId;
        private long _senderId;
        private MessageVMSenderType _senderType;
        private string _senderName;
        private Uri _senderAvatar;
        private long _adminAuthorId;
        private bool _isImportant;
        private DateTime _sentTime;
        private DateTime? _editTime;
        private string _text;
        private List<Attachment> _attachments = new List<Attachment>();
        private List<MessageViewModel> _forwardedMessages = new List<MessageViewModel>();
        private MessageViewModel _replyMessage;
        private Geo _location;
        private VKAPILib.Objects.Action _action;
        private BotKeyboard _keyboard;
        private BotTemplate _template;
        private string _payload;
        private int _ttl;
        private bool _isExpired;
        private bool _isUnavailable;
        private int _selectedReactionId;
        private ObservableCollection<MessageReaction> _reactions;
        private MessageVMState _state;

        private bool _isSenderNameVisible;
        private bool _isSenderAvatarVisible;
        private bool _isDateBetweenVisible;
        private Gift _gift;
        private MessageUIType _uiType;
        private int _imagesCount;
        private Uri _previewImageUri;

        public int Id { get { return _id; } private set { _id = value; OnPropertyChanged(); } }
        public long PeerId { get { return _peerId; } private set { _peerId = value; OnPropertyChanged(); } }
        public int RandomId { get { return _randomId; } private set { _randomId = value; OnPropertyChanged(); } }
        public int ConversationMessageId { get { return _conversationMessageId; } private set { _conversationMessageId = value; OnPropertyChanged(); } }
        public long SenderId { get { return _senderId; } private set { _senderId = value; OnPropertyChanged(); } }
        public MessageVMSenderType SenderType { get { return _senderType; } private set { _senderType = value; OnPropertyChanged(); } }
        public string SenderName { get { return _senderName; } private set { _senderName = value; OnPropertyChanged(); } }
        public Uri SenderAvatar { get { return _senderAvatar; } private set { _senderAvatar = value; OnPropertyChanged(); } }
        public long AdminAuthorId { get { return _adminAuthorId; } private set { _adminAuthorId = value; OnPropertyChanged(); } }
        public bool IsImportant { get { return _isImportant; } private set { _isImportant = value; OnPropertyChanged(); } }
        public DateTime SentTime { get { return _sentTime; } private set { _sentTime = value; OnPropertyChanged(); } }
        public DateTime? EditTime { get { return _editTime; } private set { _editTime = value; OnPropertyChanged(); } }
        public string Text { get { return _text; } private set { _text = value; OnPropertyChanged(); } }
        public List<Attachment> Attachments { get { return _attachments; } private set { _attachments = value; OnPropertyChanged(); } }
        public List<MessageViewModel> ForwardedMessages { get { return _forwardedMessages; } private set { _forwardedMessages = value; OnPropertyChanged(); } }
        public MessageViewModel ReplyMessage { get { return _replyMessage; } private set { _replyMessage = value; OnPropertyChanged(); } }
        public Geo Location { get { return _location; } private set { _location = value; OnPropertyChanged(); } }
        public VKAPILib.Objects.Action Action { get { return _action; } private set { _action = value; OnPropertyChanged(); } }
        public BotKeyboard Keyboard { get { return _keyboard; } private set { _keyboard = value; OnPropertyChanged(); } }
        public BotTemplate Template { get { return _template; } private set { _template = value; OnPropertyChanged(); } }
        public string Payload { get { return _payload; } private set { _payload = value; OnPropertyChanged(); } }
        public int TTL { get { return _ttl; } private set { _ttl = value; OnPropertyChanged(); } }
        public bool IsExpired { get { return _isExpired; } private set { _isExpired = value; OnPropertyChanged(); } }
        public bool IsUnavailable { get { return _isUnavailable; } private set { _isUnavailable = value; OnPropertyChanged(); } }
        public int SelectedReactionId { get { return _selectedReactionId; } set { _selectedReactionId = value; OnPropertyChanged(); } }
        public ObservableCollection<MessageReaction> Reactions { get { return _reactions; } private set { _reactions = value; OnPropertyChanged(); } }
        public MessageVMState State { get { return _state; } set { _state = value; OnPropertyChanged(); } }
        public bool IsOutgoing { get { return session.Id == SenderId; } }

        // UI specific
        public bool IsSenderNameVisible { get { return _isSenderNameVisible; } private set { _isSenderNameVisible = value; OnPropertyChanged(); } }
        public bool IsSenderAvatarVisible { get { return _isSenderAvatarVisible; } private set { _isSenderAvatarVisible = value; OnPropertyChanged(); } }
        public bool IsDateBetweenVisible { get { return _isDateBetweenVisible; } private set { _isDateBetweenVisible = value; OnPropertyChanged(); } }
        public Gift Gift { get { return _gift; } private set { _gift = value; OnPropertyChanged(); } }
        public MessageUIType UIType { get { return _uiType; } private set { _uiType = value; OnPropertyChanged(); } }
        public int ImagesCount { get { return _imagesCount; } private set { _imagesCount = value; OnPropertyChanged(); } }
        public Uri PreviewImageUri { get { return _previewImageUri; } private set { _previewImageUri = value; OnPropertyChanged(); } }
        public bool CanShowInUI { get { return Action == null && !IsExpired; } }

        public VKSession OwnerSession { get => session; }

        #region Events

        public event EventHandler MessageEdited;

        #endregion

        public MessageViewModel(Message msg, VKSession session = null) {
            this.session = session;
            Setup(msg);
            PropertyChanged += MessageViewModel_PropertyChanged;
        }

        private void Setup(Message msg) {
            Id = msg.Id;
            PeerId = msg.PeerId;
            RandomId = msg.RandomId;
            ConversationMessageId = msg.ConversationMessageId;
            SentTime = msg.DateTime;
            EditTime = msg.UpdateTimeUnix != 0 ? msg.UpdateTime : null;
            IsImportant = msg.Important;
            AdminAuthorId = msg.AdminAuthorId;
            SenderId = msg.FromId;
            Text = msg.Text;
            if (msg.Attachments != null) Attachments = msg.Attachments;
            Location = msg.Geo;
            if (msg.ReplyMessage != null) ReplyMessage = new MessageViewModel(msg.ReplyMessage, session);
            Action = msg.Action;
            Keyboard = msg.Keyboard;
            Template = msg.Template;
            Payload = msg.PayLoad;
            TTL = Math.Max(msg.ExpireTTL, msg.TTL);
            IsExpired = msg.IsExpired;
            IsUnavailable = msg.IsUnavailable;
            SelectedReactionId = msg.ReactionId;
            Reactions = msg.Reactions == null ? new ObservableCollection<MessageReaction>() : new ObservableCollection<MessageReaction>(msg.Reactions);

            if (msg.ForwardedMessages != null) {
                ForwardedMessages.Clear();
                foreach (var fmsg in msg.ForwardedMessages) {
                    ForwardedMessages.Add(new MessageViewModel(fmsg));
                }
            }

            SetSenderNameAndAvatar();
            UpdateUIType();
            if (msg.IsPartial || State != MessageVMState.Read) State = MessageVMState.Loading;

            if (session != null && !DemoMode.IsEnabled) {
                session.LongPoll.MessageEdited += LongPoll_MessageEdited;
                session.LongPoll.MessageFlagSet += LongPoll_MessageFlagSet;
                session.LongPoll.MessageFlagRemove += LongPoll_MessageFlagRemove;
                if (session.Id != SenderId) session.LongPoll.IncomingMessagesRead += LongPoll_MessagesRead;
                if (session.Id == SenderId) session.LongPoll.OutgoingMessagesRead += LongPoll_MessagesRead;
            }

            if (DemoMode.IsEnabled) {
                DemoModeSession ds = DemoMode.GetDemoSessionById(session.Id);
                if (ds.Times != null && ds.Times.ContainsKey(Id)) SentTime = DateTime.Now.AddSeconds(-ds.Times[Id]);
            }
        }

        private void SetSenderNameAndAvatar() {
            if (SenderId.IsUser()) {
                SenderType = MessageVMSenderType.User;
                User u = CacheManager.GetUser(SenderId);
                if (u == null) Log.Warning($"MessageViewModel: user with id {SenderId} was not found in cache!");
                SenderName = u == null ? $"id{SenderId}" : u.FullName;
                if (u != null) SenderAvatar = u.Photo;
            } else if (SenderId.IsGroup()) {
                SenderType = MessageVMSenderType.Group;
                Group g = CacheManager.GetGroup(SenderId);
                if (g == null) Log.Warning($"MessageViewModel: group with id {SenderId} was not found in cache!");
                SenderName = g == null ? $"club{SenderId}" : g.Name;
                if (g != null) SenderAvatar = g.Photo;
            }
        }

        private void MessageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(Attachments) || e.PropertyName == nameof(Text)
                 || e.PropertyName == nameof(ReplyMessage) || e.PropertyName == nameof(ForwardedMessages)
                  || e.PropertyName == nameof(Geo) || e.PropertyName == nameof(Keyboard)) {
                UpdateUIType();
            } else if (e.PropertyName == nameof(IsExpired) || e.PropertyName == nameof(Action)) {
                OnPropertyChanged(nameof(CanShowInUI));
            }
        }

        private void UpdateUIType() {
            if (Attachments.Count == 1) {
                var a = Attachments[0];
                if ((a.Type == AttachmentType.Photo || a.Type == AttachmentType.Video
                    || (a.Type == AttachmentType.Document && a.Document.Preview != null))
                    && String.IsNullOrEmpty(Text) && ReplyMessage == null
                    && ForwardedMessages.Count == 0 && Location == null && Keyboard == null) {
                    UIType = MessageUIType.SingleImage;
                } else if (a.Type == AttachmentType.Sticker && String.IsNullOrEmpty(Text)) {
                    UIType = MessageUIType.Sticker;
                    PreviewImageUri = a.Sticker.GetSizeAndUriForThumbnail(64).Uri;
                    return;
                } else if (a.Type == AttachmentType.Graffiti && String.IsNullOrEmpty(Text)) {
                    UIType = MessageUIType.Graffiti;
                    PreviewImageUri = a.Graffiti.Uri;
                    return;
                } else if (a.Type == AttachmentType.Story && String.IsNullOrEmpty(Text) && ReplyMessage == null
                    && ForwardedMessages.Count == 0 && Location == null && Keyboard == null) {
                    UIType = MessageUIType.Story;
                } else if (a.Type == AttachmentType.Gift) {
                    UIType = MessageUIType.Gift;
                    Gift = a.Gift;
                    PreviewImageUri = a.Gift.ThumbUri;
                    return;
                } else {
                    UIType = MessageUIType.Complex;
                }
            } else if (Attachments.Count == 2) {
                Attachment a1 = Attachments[0];
                Attachment a2 = Attachments[1];

                bool ss1 = a1.Type == AttachmentType.Sticker && a2.Type == AttachmentType.Story;
                bool ss2 = a1.Type == AttachmentType.Story && a2.Type == AttachmentType.Sticker;
                if (ss1 || ss2) {
                    UIType = MessageUIType.StoryWithSticker;
                } else {
                    UIType = MessageUIType.Complex;
                }

                bool firstImage = a1.Type == AttachmentType.Photo || a1.Type == AttachmentType.Video 
                    || (a1.Type == AttachmentType.Document && a1.Document.Preview != null);
                bool secondImage = a2.Type == AttachmentType.Photo || a2.Type == AttachmentType.Video
                    || (a2.Type == AttachmentType.Document && a2.Document.Preview != null);
            } else if (Attachments.Count == 0 && ForwardedMessages.Count == 0 && Location == null) {
                UIType = !String.IsNullOrEmpty(Text) || ReplyMessage != null ? MessageUIType.Standart : MessageUIType.Empty;
            } else {
                UIType = MessageUIType.Complex;
            }

            Gift = Attachments.Where(a => a.Type == AttachmentType.Gift).FirstOrDefault()?.Gift;

            var images = Attachments
                .Where(a => a.Type == AttachmentType.Photo || a.Type == AttachmentType.Video
                || (a.Type == AttachmentType.Document && a.Document.Preview != null));
            ImagesCount = images.Count();
            if (_imagesCount > 0) {
                Attachment a = images.FirstOrDefault();
                if (a.Photo != null) {
                    PreviewImageUri = a.Photo.GetSizeAndUriForThumbnail(Constants.CompactMessagePreviewSize, Constants.CompactMessagePreviewSize).Uri;
                } else if (a.Video != null) {
                    PreviewImageUri = a.Video.GetSizeAndUriForThumbnail(Constants.CompactMessagePreviewSize, Constants.CompactMessagePreviewSize).Uri;
                } else if (a.Photo != null) {
                    PreviewImageUri = a.Document.Preview?.Photo?.GetSizeAndUriForThumbnail(Constants.CompactMessagePreviewSize, Constants.CompactMessagePreviewSize).Uri;
                }
            }
        }

        public int CompareTo(object obj) {
            MessageViewModel mvm = obj as MessageViewModel;
            return ConversationMessageId.CompareTo(mvm.ConversationMessageId);
        }

        public void UpdateSenderInfoView(bool? isPrevFromSameSender, bool? isNextFromSameSender) {
            if (!PeerId.IsChat() || SenderId == VKSession.Main.Id) return;
            if (isPrevFromSameSender.HasValue) IsSenderNameVisible = !isPrevFromSameSender.Value;
            if (isNextFromSameSender.HasValue) IsSenderAvatarVisible = !isNextFromSameSender.Value;
        }

        public void UpdateDateBetweenVisibility(bool visible) {
            IsDateBetweenVisible = visible;
        }

        #region LongPoll events

        private async void LongPoll_MessageEdited(LongPoll longPoll, Message message, int flags) {
            bool isCurrentMessage = message.PeerId == PeerId && message.ConversationMessageId == ConversationMessageId;
            if (!isCurrentMessage) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                Setup(message);
                bool isUnread = flags.HasFlag(1);
                State = isUnread ? MessageVMState.Unread : MessageVMState.Read;
                MessageEdited?.Invoke(this, null);
            });
        }

        private async void LongPoll_MessageFlagSet(LongPoll longPoll, int messageId, int flags, long peerId) {
            if (messageId != ConversationMessageId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                if (flags.HasFlag(8)) { // Marked as important
                    IsImportant = true;
                }
            });
        }

        private async void LongPoll_MessageFlagRemove(LongPoll longPoll, int messageId, int flags, long peerId) {
            if (messageId != ConversationMessageId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                if (flags.HasFlag(8)) { // Unmarked as important
                    IsImportant = false;
                }
            });
        }

        private async void LongPoll_MessagesRead(LongPoll longPoll, long peerId, int messageId, int count) {
            if (peerId != PeerId) return;
            await Dispatcher.UIThread.InvokeAsync(() => {
                if (ConversationMessageId <= messageId) State = MessageVMState.Read;
            });
        }

        #endregion

        public override string ToString() {
            if (Action != null) {
                return new VKActionMessage(Action, SenderId).ToString();
            }

            if (!String.IsNullOrEmpty(Text)) return TextParser.GetParsedText(Text);
            if (_attachments.Count > 0) {
                int count = _attachments.Count;
                if (_attachments.All(a => a.Type == _attachments[0].Type) && Location == null) {
                    string type = _attachments[0].TypeString;
                    switch (_attachments[0].Type) {
                        case AttachmentType.Audio:
                        case AttachmentType.AudioMessage:
                        case AttachmentType.Document:
                        case AttachmentType.Photo:
                        case AttachmentType.Video: return Localizer.Instance.GetDeclensionFormatted2(count, type);
                        case AttachmentType.Call:
                        case AttachmentType.Curator:
                        case AttachmentType.Event:
                        case AttachmentType.Gift:
                        case AttachmentType.Graffiti:
                        case AttachmentType.GroupCallInProgress:
                        case AttachmentType.Link:
                        case AttachmentType.Market:
                        case AttachmentType.Podcast:
                        case AttachmentType.Poll:
                        case AttachmentType.Sticker:
                        case AttachmentType.Story:
                        case AttachmentType.Wall:
                        case AttachmentType.WallReply:
                        case AttachmentType.Narrative: return Localizer.Instance[type];
                        // case AttachmentType.TextpostPublish: return Localizer.Instance[type];
                        default: return Localizer.Instance.GetDeclensionFormatted2(count, "attachment");
                    }
                } else {
                    if (Location != null && count > 0) count++;
                    return Localizer.Instance.GetDeclensionFormatted2(count, "attachment");
                }
            }
            if (Location != null) return Localizer.Instance["geo"];
            if (_forwardedMessages.Count > 0) return Localizer.Instance.GetDeclensionFormatted2(_forwardedMessages.Count, "forwarded_message");

            return Localizer.Instance[IsExpired? "msg_expired" : "empty_message"];
        }

        public static List<MessageViewModel> BuildFromAPI(List<Message> messages, VKSession session, System.Action<MessageViewModel> afterBuild = null) {
            List<MessageViewModel> vms = new List<MessageViewModel>();
            foreach (var message in CollectionsMarshal.AsSpan(messages)) {
                MessageViewModel mvm = new MessageViewModel(message, session);
                afterBuild?.Invoke(mvm);
                vms.Add(mvm);
            }
            return vms;
        }
    }
}
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.DataModels;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ELOR.Laney.ViewModels.Controls {
    public enum MessageVMState {
        Sending, Unread, Read, Deleted, Failed
    }

    public enum MessageVMSenderType {
        Unknown, User, Group
    }

    public enum MessageUIType {
        Empty, Standart, Complex, SingleImage, Story, Sticker, StoryWithSticker, Graffiti, Gift
    }

    public sealed class MessageViewModel : ViewModelBase, IComparable {
        private int _id;
        private int _peerId;
        private int _randomId;
        private int _conversationMessageId;
        private int _senderId;
        private MessageVMSenderType _senderType;
        private string _senderName;
        private Uri _senderAvatar;
        private int _adminAuthorId;
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
        private MessageVMState _state;

        private bool _isSenderNameVisible;
        private bool _isSenderAvatarVisible;
        private bool _isDateBetweenVisible;
        private MessageUIType _uiType;
        private bool _containsMultipleImages;

        public int Id { get { return _id; } private set { _id = value; OnPropertyChanged(); } }
        public int PeerId { get { return _peerId; } private set { _peerId = value; OnPropertyChanged(); } }
        public int RandomId { get { return _randomId; } private set { _randomId = value; OnPropertyChanged(); } }
        public int ConversationMessageId { get { return _conversationMessageId; } private set { _conversationMessageId = value; OnPropertyChanged(); } }
        public int SenderId { get { return _senderId; } private set { _senderId = value; OnPropertyChanged(); } }
        public MessageVMSenderType SenderType { get { return _senderType; } private set { _senderType = value; OnPropertyChanged(); } }
        public string SenderName { get { return _senderName; } private set { _senderName = value; OnPropertyChanged(); } }
        public Uri SenderAvatar { get { return _senderAvatar; } private set { _senderAvatar = value; OnPropertyChanged(); } }
        public int AdminAuthorId { get { return _adminAuthorId; } private set { _adminAuthorId = value; OnPropertyChanged(); } }
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
        public MessageVMState State { get { return _state; } private set { _state = value; OnPropertyChanged(); } }

        // UI specific
        public bool IsSenderNameVisible { get { return _isSenderNameVisible; } private set { _isSenderNameVisible = value; OnPropertyChanged(); } }
        public bool IsSenderAvatarVisible { get { return _isSenderAvatarVisible; } private set { _isSenderAvatarVisible = value; OnPropertyChanged(); } }
        public bool IsDateBetweenVisible { get { return _isDateBetweenVisible; } private set { _isDateBetweenVisible = value; OnPropertyChanged(); } }
        public MessageUIType UIType { get { return _uiType; } private set { _uiType = value; OnPropertyChanged(); } }

        public bool ContainsMultipleImages { get { return _containsMultipleImages; } private set { _containsMultipleImages = value; OnPropertyChanged(); } }

        public MessageViewModel(Message msg) {
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
            Attachments = msg.Attachments;
            Location = msg.Geo;
            if (msg.ReplyMessage != null) ReplyMessage = new MessageViewModel(msg.ReplyMessage);
            Action = msg.Action;
            Keyboard = msg.Keyboard;
            Template = msg.Template;
            Payload = msg.PayLoad;
            TTL = Math.Max(msg.ExpireTTL, msg.TTL);
            IsExpired = msg.IsExpired;

            if (msg.ForwardedMessages != null) {
                ForwardedMessages.Clear();
                foreach (var fmsg in msg.ForwardedMessages) {
                    ForwardedMessages.Add(new MessageViewModel(fmsg));
                }
            }

            SetSenderNameAndAvatar();
            UpdateUIType();
        }

        private void SetSenderNameAndAvatar() {
            if (SenderId > 0) {
                User u = CacheManager.GetUser(SenderId);
                SenderName = u == null ? $"id{SenderId}" : u.FullName;
                if (u != null) SenderAvatar = u.Photo;
            } else if (SenderId < 0) {
                if (SenderId < -2000000000) {
                    SenderName = "E-Mail";
                    SenderAvatar = null;
                } else {
                    Group g = CacheManager.GetGroup(SenderId);
                    SenderName = g == null ? $"club{SenderId}" : g.Name;
                    if (g != null) SenderAvatar = g.Photo;
                }
            }
        }

        private void MessageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(Attachments) || e.PropertyName == nameof(Text)
                 || e.PropertyName == nameof(ReplyMessage) || e.PropertyName == nameof(ForwardedMessages)
                  || e.PropertyName == nameof(Geo) || e.PropertyName == nameof(Keyboard)) {
                UpdateUIType();
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
                } else if (a.Type == AttachmentType.Graffiti && String.IsNullOrEmpty(Text)) {
                    UIType = MessageUIType.Graffiti;
                } else if (a.Type == AttachmentType.Story && String.IsNullOrEmpty(Text) && ReplyMessage == null
                    && ForwardedMessages == null && Location == null && Keyboard == null) {
                    UIType = MessageUIType.Story;
                } else if (a.Type == AttachmentType.Gift) {
                    UIType = MessageUIType.Gift;
                } else {
                    UIType = MessageUIType.Complex;
                }
            } else if (Attachments.Count == 2) {
                Attachment a1 = Attachments[0];
                Attachment a2 = Attachments[1];

                bool hasSticker = a1.Type == AttachmentType.Sticker || a2.Type == AttachmentType.Sticker;
                bool hasStory = a1.Type == AttachmentType.Story || a2.Type == AttachmentType.Story;
                if (hasSticker && hasStory) {
                    UIType = MessageUIType.StoryWithSticker;
                } else {
                    UIType = MessageUIType.Complex;
                }

                bool firstImage = a1.Type == AttachmentType.Photo || a1.Type == AttachmentType.Video 
                    || (a1.Type == AttachmentType.Document && a1.Document.Preview != null);
                bool secondImage = a2.Type == AttachmentType.Photo || a2.Type == AttachmentType.Video
                    || (a2.Type == AttachmentType.Document && a2.Document.Preview != null);
                ContainsMultipleImages = firstImage && secondImage;
            } else if (Attachments.Count == 0 && ForwardedMessages.Count == 0 && Location == null && Keyboard == null) {
                UIType = !String.IsNullOrEmpty(Text) || ReplyMessage != null ? MessageUIType.Standart : MessageUIType.Empty;
            } else {
                UIType = MessageUIType.Complex;
            }
        }

        public int CompareTo(object obj) {
            MessageViewModel mvm = obj as MessageViewModel;
            return Id.CompareTo(mvm.Id);
        }

        public void UpdateSenderInfoView(bool? isPrevFromSameSender, bool? isNextFromSameSender) {
            if (PeerId < 2000000000 || SenderId == VKSession.Main.Id) return;
            if (isPrevFromSameSender.HasValue) IsSenderNameVisible = !isPrevFromSameSender.Value;
            if (isNextFromSameSender.HasValue) IsSenderAvatarVisible = !isNextFromSameSender.Value;
        }

        public void UpdateDateBetweenVisibility(bool visible) {
            IsDateBetweenVisible = visible;
        }

        public override string ToString() {
            if (Action != null) {
                return new VKActionMessage(Action, SenderId).ToString();
            }

            if (!String.IsNullOrEmpty(Text)) return Text;
            if (_attachments.Count > 0) {
                int count = _attachments.Count;
                if (_attachments.Any(a => a.Type == _attachments[0].Type) && Location == null) {
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
                        case AttachmentType.WallReply: return Localizer.Instance[type];
                        default: return Localizer.Instance.GetDeclensionFormatted2(count, "attachment");
                    }
                } else {
                    if (Location != null) {
                        if (count == 0) {
                            return Localizer.Instance["geo"];
                        } else {
                            count++;
                        }
                    }
                    return Localizer.Instance.GetDeclensionFormatted2(count, "attachment");
                }
            }
            if (_forwardedMessages.Count > 0) return Localizer.Instance.GetDeclensionFormatted2(_forwardedMessages.Count, "forwarded_message");

            return Localizer.Instance["empty_message"];
        }

        public static List<MessageViewModel> BuildFromAPI(List<Message> messages) {
            List<MessageViewModel> vms = new List<MessageViewModel>();
            foreach (var message in CollectionsMarshal.AsSpan<Message>(messages)) {
                vms.Add(new MessageViewModel(message));
            }
            return vms;
        }
    }
}
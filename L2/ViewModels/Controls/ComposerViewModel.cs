using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ELOR.Laney.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.Views.Modals;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VKUI.Controls;
using VKUI.Popups;

namespace ELOR.Laney.ViewModels.Controls {
    public class ComposerViewModel : CommonViewModel {
        private VKSession session;

        private bool _isGroupSession;
        private bool _canSendMessage;
        private int _editingMessageId;
        private string _text;
        private int _textSelectionStart;
        private int _textSelectionEnd;
        private ObservableCollection<OutboundAttachmentViewModel> _attachments = new ObservableCollection<OutboundAttachmentViewModel>();
        private MessageViewModel _reply;
        private BotKeyboard _botKeyboard;
        private List<Sticker> _suggestedStickers;

        private RelayCommand _sendCommand;
        private RelayCommand _recordAudioCommand;

        public bool IsGroupSession { get { return _isGroupSession; } private set { _isGroupSession = value; OnPropertyChanged(); } }
        public bool CanSendMessage { get { return _canSendMessage; } private set { _canSendMessage = value; OnPropertyChanged(); } }
        public int EditingMessageId { get { return _editingMessageId; } private set { _editingMessageId = value; OnPropertyChanged(); } }
        public string Text { get { return _text; } set { _text = value; OnPropertyChanged(); } }
        public int TextSelectionStart { get { return _textSelectionStart; } set { _textSelectionStart = value; OnPropertyChanged(); } }
        public int TextSelectionEnd { get { return _textSelectionEnd; } set { _textSelectionEnd = value; OnPropertyChanged(); } }
        public ObservableCollection<OutboundAttachmentViewModel> Attachments { get { return _attachments; } private set { _attachments = value; OnPropertyChanged(); } }
        public MessageViewModel Reply { get { return _reply; } private set { _reply = value; OnPropertyChanged(); } }
        public BotKeyboard BotKeyboard { get { return _botKeyboard; } set { _botKeyboard = value; OnPropertyChanged(); } }
        public List<Sticker> SuggestedStickers { get { return _suggestedStickers; } set { _suggestedStickers = value; OnPropertyChanged(); } }

        public RelayCommand SendCommand { get { return _sendCommand; } private set { _sendCommand = value; OnPropertyChanged(); } }
        public RelayCommand RecordAudioCommand { get { return _recordAudioCommand; } private set { _recordAudioCommand = value; OnPropertyChanged(); } }

        ChatViewModel Chat;
        Random Random = null;
        int RandomId = 0;
        int StickerId = 0;

        public ComposerViewModel(VKSession session, ChatViewModel chat) {
            this.session = session;
            IsGroupSession = session.IsGroup;
            Chat = chat;
            Attachments.CollectionChanged += (a, b) => CheckCanSendMessage();
            SendCommand = new RelayCommand(async (o) => await SendMessageAsync());
            RecordAudioCommand = new RelayCommand((o) => RecordAudio());

            Random = new Random();
            RandomId = Random.Next(Int32.MinValue, Int32.MaxValue);

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(Text)) {
                CheckCanSendMessage();
                CheckStickersSuggestions();
            }
        }

        private void CheckCanSendMessage() {
            CanSendMessage = !String.IsNullOrEmpty(Text) || Attachments.Count > 0 || StickerId > 0;
        }

        public void ShowAttachmentPickerContextMenu(Control target) {
            if (DemoMode.IsEnabled) return;
            ActionSheet ash = new ActionSheet {
                Placement = PlacementMode.TopEdgeAlignedLeft
            };

            ActionSheetItem photo = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20PictureOutline },
                Header = Assets.i18n.Resources.photo,
            };
            ActionSheetItem video = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20VideoOutline },
                Header = Assets.i18n.Resources.video,
            };
            ActionSheetItem file = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20DocumentOutline },
                Header = Assets.i18n.Resources.doc,
            };
            ActionSheetItem poll = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20PollOutline },
                Header = Assets.i18n.Resources.poll,
            };

            var session = VKSession.GetByDataContext(target);
            int count = Attachments.Where(a => a.Type == OutboundAttachmentType.Attachment).Count();

            photo.Click += async (a, b) => {
                AttachmentPicker ap = new AttachmentPicker(session, 10 - count, 0);
                await AddAttachmentsAsync(await ap.ShowDialog<object>(session.Window));
            };
            video.Click += async (a, b) => {
                AttachmentPicker ap = new AttachmentPicker(session, 10 - count, 1);
                await AddAttachmentsAsync(await ap.ShowDialog<object>(session.Window));
            };
            file.Click += async (a, b) => {
                AttachmentPicker ap = new AttachmentPicker(session, 10 - count, 2);
                await AddAttachmentsAsync(await ap.ShowDialog<object>(session.Window));
            };
            poll.Click += (a, b) => {
                ExceptionHelper.ShowNotImplementedDialog(session.Window);
            };

            ash.Items.Add(photo);
            ash.Items.Add(video);
            ash.Items.Add(file);
            ash.Items.Add(poll);
            ash.ShowAt(target);
        }

        public void ShowGroupTemplates(Button target) {
            if (Chat.PeerType != PeerType.User || !session.IsGroup) return;
            var currentChatUser = CacheManager.GetUser(Chat.PeerId);
            var currentAdmin = CacheManager.GetUser(VKSession.Main.Id);
            var groupName = CacheManager.GetGroup(session.GroupId).Name;

            var picker = new GroupMessageTemplates(session, currentChatUser, currentAdmin, groupName) {
                Width = 320,
                Height = 320
            };

            VKUIFlyout flyout = new VKUIFlyout {
                Content = picker
            };

            picker.TemplateSelected += (a, b) => {
                flyout.Hide();
                Text = b;
            };

            flyout.ShowAt(target);
        }

        public void ShowEmojiStickerPicker(Control target) {
            var picker = new EmojiStickerPicker {
                Width = 400,
                Height = 438,
                DataContext = new EmojiStickerPickerViewModel(session)
            };

            VKUIFlyout flyout = new VKUIFlyout {
                Content = picker
            };

            picker.EmojiPicked += Picker_EmojiPicked;
            picker.StickerPicked += async (a, b) => {
                flyout.Hide();
                await SendStickerAsync(b.StickerId);
            };

            flyout.ShowAt(target);
        }

        private void Picker_EmojiPicked(object sender, string e) {
            try {
                if (TextSelectionStart == TextSelectionEnd) {
                    if (String.IsNullOrEmpty(Text)) {
                        Text = e;
                    } else {
                        Text = Text.Insert(TextSelectionEnd, e);
                    }
                    TextSelectionStart += e.Length;
                    TextSelectionEnd += e.Length;
                } else {
                    int start = Math.Min(TextSelectionStart, TextSelectionEnd);
                    int end = Math.Max(TextSelectionStart, TextSelectionEnd);
                    string newText = Text.Remove(start, end - start);
                    Text = newText.Insert(start, e);
                    start += e.Length;
                    TextSelectionStart = start;
                    TextSelectionEnd = start;
                }
            } catch (ArgumentOutOfRangeException) { // Workaround for issue #20 that mostye not reproducible
                if (String.IsNullOrEmpty(Text)) {
                    Text = e;
                } else {
                    Text = Text.Insert(Text.Length - 1, e);
                }
            }
        }

        private async Task AddAttachmentsAsync(object pickerResult) {
            if (pickerResult == null) return;
            if (pickerResult is List<AttachmentBase> attachments) {
                foreach (AttachmentBase attachment in attachments) {
                    Attachments.Add(new OutboundAttachmentViewModel(session, attachment));
                }
            } else if (pickerResult is Tuple<int, List<IStorageFile>> pfiles) {
                foreach (IStorageFile file in pfiles.Item2) {
                    Attachments.Add(new OutboundAttachmentViewModel(session, file, pfiles.Item1));
                    await Task.Delay(500);
                }
            }
        }

        public void AddReply(MessageViewModel message) {
            Reply = message;
            var favm = Attachments.Where(a => a.Type == OutboundAttachmentType.ForwardedMessages).FirstOrDefault();
            if (favm != null) Attachments.Remove(favm);
        }

        public void AddForwardedMessages(long fromPeerId, List<MessageViewModel> messages, long groupId = 0) {
            if (messages == null || messages.Count == 0) return;
            var favm = Attachments.Where(a => a.Type == OutboundAttachmentType.ForwardedMessages).FirstOrDefault();
            if (favm != null) Attachments.Remove(favm);
            Attachments.Insert(0, new OutboundAttachmentViewModel(fromPeerId, messages, groupId));
            Reply = null;
        }

        public void StartEditing(MessageViewModel message) {
            Clear();
            EditingMessageId = message.ConversationMessageId;

            Text = message.Text;
            Reply = message.ReplyMessage;

            foreach (var attachment in CollectionsMarshal.AsSpan(message.Attachments)) {
                if (attachment.Type.CanAttachToSend()) {
                    var oavm = OutboundAttachmentViewModel.FromAttachmentBase(session, attachment);
                    if (oavm != null) Attachments.Add(oavm);
                }
            }

            // TODO: удаление пересланных сообщений
            // AddForwardedMessages(message.ForwardedMessages);
        }

        public void DeleteReply() {
            Reply = null;
        }

        public void CancelEditing() {
            Clear();
        }

        public async Task SendMessageAsync() {
            if (!CanSendMessage || IsLoading) return;

            int uploadingFiles = Attachments.Where(a => a.Type == OutboundAttachmentType.Attachment && a.IsUploading).Count();
            int failedFiles = Attachments.Where(a => a.Type == OutboundAttachmentType.Attachment && a.UploadException != null).Count();

            if (uploadingFiles > 0) {
                VKUIDialog dlg = new VKUIDialog("Cannot send a message at this moment", $"Please wait until {uploadingFiles} files has been uploaded");
                await dlg.ShowDialog(session.Window);
                return;
            }

            if (failedFiles > 0) {
                VKUIDialog dlg = new VKUIDialog("Cannot send a message", $"You have {failedFiles} failed attachments. Please re-upload or delete these attachments.");
                await dlg.ShowDialog(session.Window);
                return;
            }

            string text = !String.IsNullOrEmpty(Text) ? Text.Replace("\r\n", "\r").Replace("\r", "\r\n").Trim() : null;

            var attachments = Attachments.Where(a => a.Type == OutboundAttachmentType.Attachment)
                .Select(a => a?.Attachment?.ToString()).ToList();


            var favm = Attachments.Where(a => a.Type == OutboundAttachmentType.ForwardedMessages).FirstOrDefault();
            int replyTo = Reply?.ConversationMessageId ?? 0;
            string forward = String.Empty;

            if (replyTo > 0) {
                forward = $"{{\"peer_id\":{Chat.PeerId},\"conversation_message_ids\":[{replyTo}],\"is_reply\":true}}";
            } else if (favm != null) {
                List<int> cmids = new List<int>();
                if (favm.ForwardedMessagesFromGroupId > 0) {
                    long ownerId = favm.ForwardedMessagesFromGroupId * -1;
                    foreach (var m in favm.ForwardedMessages) cmids.Add(m.ConversationMessageId);
                    string cmidstr = String.Join(',', cmids);
                    forward = $"{{\"owner_id\":{ownerId},\"peer_id\":{favm.ForwardedMessagesFromPeerId},\"conversation_message_ids\":[{cmidstr}]}}";
                } else {
                    cmids = favm.ForwardedMessages.Select(m => m.ConversationMessageId).ToList();
                    string cmidstr = String.Join(',', cmids);
                    forward = $"{{\"peer_id\":{favm.ForwardedMessagesFromPeerId},\"conversation_message_ids\":[{cmidstr}]}}";
                }
            }

            bool dontParseLinks = Settings.DontParseLinks;
            bool disableMentions = Settings.DisableMentions;

            IsLoading = true;

            try {
                if (EditingMessageId == 0) {
                    Log.Verbose($"Sending message: session={session.Id}; peer_id={Chat.PeerId}, random={RandomId}");
                    var response = await session.API.Messages.SendAsync(session.GroupId,
                        Chat.PeerId, RandomId, text, 0, 0, attachments, forward, StickerId,
                        dontParseLinks: dontParseLinks, disableMentions: disableMentions);
                    RandomId = Random.Next(Int32.MinValue, Int32.MaxValue);
                    Log.Verbose($"Sending message result: {response.MessageId}; new random: {RandomId}");
                } else {
                    var response = await session.API.Messages.EditAsync(session.GroupId, Chat.PeerId, EditingMessageId,
                        text, 0, 0, attachments, true, true, dontParseLinks);
                    // TODO: keep snippets и сделать недоступным добавление пересланных, если активен режим редактирования. 
                    // TODO: удаление пересланных сообщений
                }
                Clear();
                IsLoading = false;
            } catch (Exception ex) {
                IsLoading = false;
                if (await ExceptionHelper.ShowErrorDialogAsync(session.Window, ex)) await SendMessageAsync();
            }
        }

        public async Task SendStickerAsync(int stickerId) {
            StickerId = stickerId;
            CheckCanSendMessage();
            await SendMessageAsync();
        }

        public void RecordAudio() {
            ExceptionHelper.ShowNotImplementedDialog(session.Window);
        }

        public void Clear() {
            EditingMessageId = 0;
            Reply = null;
            Text = null;
            Attachments.Clear();
            StickerId = 0;
        }

        // Подсказки стикеров

        private void CheckStickersSuggestions() {
            if (!Settings.SuggestStickers) {
                SuggestedStickers = null;
                return;
            }

            var stickers = StickersManager.GetStickersByWord(Text);
            SuggestedStickers = stickers;
        }
    }
}
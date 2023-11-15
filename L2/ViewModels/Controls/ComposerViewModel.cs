using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ELOR.Laney.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.Views.Modals;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using VKUI.Controls;
using VKUI.Popups;

namespace ELOR.Laney.ViewModels.Controls {
    public class ComposerViewModel : CommonViewModel {
        private VKSession session;
        
        private bool _canSendMessage;
        private int _editingMessageId;
        private string _text;
        private int _textSelectionStart;
        private int _textSelectionEnd;
        private ObservableCollection<OutboundAttachmentViewModel> _attachments = new ObservableCollection<OutboundAttachmentViewModel>();
        private MessageViewModel _reply;
        private BotKeyboard _botKeyboard;
        private RelayCommand _sendCommand;
        private RelayCommand _recordAudioCommand;

        public bool CanSendMessage { get { return _canSendMessage; } private set { _canSendMessage = value; OnPropertyChanged(); } }
        public int EditingMessageId { get { return _editingMessageId; } private set { _editingMessageId = value; OnPropertyChanged(); } }
        public string Text { get { return _text; } set { _text = value; OnPropertyChanged(); CheckCanSendMessage(); } }
        public int TextSelectionStart { get { return _textSelectionStart; } set { _textSelectionStart = value; OnPropertyChanged(); } }
        public int TextSelectionEnd { get { return _textSelectionEnd; } set { _textSelectionEnd = value; OnPropertyChanged(); } }
        public ObservableCollection<OutboundAttachmentViewModel> Attachments { get { return _attachments; } private set { _attachments = value; OnPropertyChanged(); } }
        public MessageViewModel Reply { get { return _reply; } private set { _reply = value; OnPropertyChanged(); } }
        public BotKeyboard BotKeyboard { get { return _botKeyboard; } set { _botKeyboard = value; OnPropertyChanged(); } }

        ChatViewModel Chat;
        Random Random = null;
        int RandomId = 0;
        int StickerId = 0;

        public RelayCommand SendCommand { get { return _sendCommand; } private set { _sendCommand = value; OnPropertyChanged(); } }
        public RelayCommand RecordAudioCommand { get { return _recordAudioCommand; } private set { _recordAudioCommand = value; OnPropertyChanged(); } }

        public ComposerViewModel(VKSession session, ChatViewModel chat) {
            this.session = session;
            Chat = chat;
            Attachments.CollectionChanged += (a, b) => CheckCanSendMessage();
            SendCommand = new RelayCommand((o) => SendMessage());
            RecordAudioCommand = new RelayCommand((o) => RecordAudio());

            Random = new Random();
            RandomId = Random.Next(Int32.MinValue, Int32.MaxValue);
        }

        private void CheckCanSendMessage() {
            CanSendMessage = !String.IsNullOrEmpty(Text) || Attachments.Count > 0 || StickerId > 0;
        }

        public void ShowAttachmentPickerContextMenu(Control target) {
            ActionSheet ash = new ActionSheet {
                Placement = PlacementMode.TopEdgeAlignedLeft
            };

            ActionSheetItem photo = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20PictureOutline },
                Header = Localizer.Instance["photo"],
            };
            ActionSheetItem video = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20VideoOutline },
                Header = Localizer.Instance["video"],
            };
            ActionSheetItem file = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20DocumentOutline },
                Header = Localizer.Instance["doc"],
            };
            ActionSheetItem poll = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20PollOutline },
                Header = Localizer.Instance["poll"],
            };

            var session = VKSession.GetByDataContext(target);
            int count = Attachments.Where(a => a.Type == OutboundAttachmentType.Attachment).Count();

            photo.Click += async (a, b) => {
                AttachmentPicker ap = new AttachmentPicker(session, 10 - count, 0);
                AddAttachments(await ap.ShowDialog<object>(session.Window));
            };
            video.Click += async (a, b) => {
                AttachmentPicker ap = new AttachmentPicker(session, 10 - count, 1);
                AddAttachments(await ap.ShowDialog<object>(session.Window));
            };
            file.Click += async (a, b) => {
                AttachmentPicker ap = new AttachmentPicker(session, 10 - count, 2);
                AddAttachments(await ap.ShowDialog<object>(session.Window));
            };
            poll.Click += (a, b) => {
                
            };

            ash.Items.Add(photo);
            ash.Items.Add(video);
            ash.Items.Add(file);
            ash.Items.Add(poll);
            ash.ShowAt(target);
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
            picker.StickerPicked += (a, b) => {
                flyout.Hide();
                StickerId = b.StickerId;
                CheckCanSendMessage();
                SendMessage();
            };

            flyout.ShowAt(target);
        }

        private void Picker_EmojiPicked(object sender, string e) {
            if (TextSelectionStart == TextSelectionEnd) {
                if (Text == null) {
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
        }

        private async void AddAttachments(object pickerResult) {
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

        public async void SendMessage() {
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
                .Select(a => a.Attachment.ToString()).ToList();


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
                if (await ExceptionHelper.ShowErrorDialogAsync(session.Window, ex)) SendMessage();
            }
        }

        public void RecordAudio() {
            ExceptionHelper.ShowNotImplementedDialogAsync(session.Window);
        }

        public void Clear() {
            EditingMessageId = 0;
            Reply = null;
            Text = null;
            Attachments.Clear();
            StickerId = 0;
        }
    }
}
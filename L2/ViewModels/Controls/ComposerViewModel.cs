using Avalonia.Controls;
using Avalonia.Platform.Storage.FileIO;
using ELOR.Laney.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Helpers;
using ELOR.Laney.Views.Modals;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VKUI.Controls;
using VKUI.Popups;

namespace ELOR.Laney.ViewModels.Controls {
    public class ComposerViewModel : CommonViewModel {
        private VKSession session;
        
        private bool _canSendMessage;
        private bool _isEditMode;
        private string _text;
        private int _textSelectionStart;
        private int _textSelectionEnd;
        private ObservableCollection<OutboundAttachmentViewModel> _attachments = new ObservableCollection<OutboundAttachmentViewModel>();
        private MessageViewModel _reply;
        private BotKeyboard _botKeyboard;
        private RelayCommand _sendCommand;
        private RelayCommand _recordAudioCommand;

        public bool CanSendMessage { get { return _canSendMessage; } private set { _canSendMessage = value; OnPropertyChanged(); } }
        public bool IsEditMode { get { return _isEditMode; } set { _isEditMode = value; OnPropertyChanged(); } }
        public string Text { get { return _text; } set { _text = value; OnPropertyChanged(); CheckCanSendMessage(); } }
        public int TextSelectionStart { get { return _textSelectionStart; } set { _textSelectionStart = value; OnPropertyChanged(); } }
        public int TextSelectionEnd { get { return _textSelectionEnd; } set { _textSelectionEnd = value; OnPropertyChanged(); } }
        public ObservableCollection<OutboundAttachmentViewModel> Attachments { get { return _attachments; } set { _attachments = value; OnPropertyChanged(); } }
        public MessageViewModel Reply { get { return _reply; } set { _reply = value; OnPropertyChanged(); } }
        public BotKeyboard BotKeyboard { get { return _botKeyboard; } set { _botKeyboard = value; OnPropertyChanged(); } }

        ChatViewModel Chat;
        public RelayCommand SendCommand { get { return _sendCommand; } private set { _sendCommand = value; OnPropertyChanged(); } }
        public RelayCommand RecordAudioCommand { get { return _recordAudioCommand; } private set { _recordAudioCommand = value; OnPropertyChanged(); } }

        public ComposerViewModel(VKSession session, ChatViewModel chat) {
            this.session = session;
            Chat = chat;
            Attachments.CollectionChanged += (a, b) => CheckCanSendMessage();
            SendCommand = new RelayCommand((o) => SendMessage());
            RecordAudioCommand = new RelayCommand((o) => RecordAudio());
        }

        private void CheckCanSendMessage() {
            CanSendMessage = !String.IsNullOrEmpty(Text) || Attachments.Count > 0;
        }

        public void ShowAttachmentPickerContextMenu(Control target) {
            ActionSheet ash = new ActionSheet {
                Placement = FlyoutPlacementMode.TopEdgeAlignedLeft
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

            photo.Click += async (a, b) => {
                AttachmentPicker ap = new AttachmentPicker(session, 10 - Attachments.Count, 0);
                AddAttachments(await ap.ShowDialog<object>(session.Window));
            };
            video.Click += async (a, b) => {
                AttachmentPicker ap = new AttachmentPicker(session, 10 - Attachments.Count, 1);
                AddAttachments(await ap.ShowDialog<object>(session.Window));
            };
            file.Click += async (a, b) => {
                AttachmentPicker ap = new AttachmentPicker(session, 10 - Attachments.Count, 2);
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
            picker.StickerPicked += async (a, b) => {
                flyout.Hide();

                VKUIDialog dlg = new VKUIDialog("Coming soon!", "Not ready yet...");
                await dlg.ShowDialog(session.Window);
            };

            flyout.ShowAt(target);
        }

        private void Picker_EmojiPicked(object sender, string e) {
            if (TextSelectionStart == TextSelectionEnd) {
                Text = Text.Insert(TextSelectionEnd, e);
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
            } else if (pickerResult is Tuple<int, List<BclStorageFile>> pfiles) {
                foreach (BclStorageFile file in pfiles.Item2) {
                    Attachments.Add(new OutboundAttachmentViewModel(session, file, pfiles.Item1));
                    await Task.Delay(500);
                }
            }
        }

        public void DeleteReply() {
            Reply = null;
        }
    
        public void SendMessage() {
            ExceptionHelper.ShowNotImplementedDialogAsync(session.Window);
        }

        public void RecordAudio() {
            ExceptionHelper.ShowNotImplementedDialogAsync(session.Window);
        }
    }
}
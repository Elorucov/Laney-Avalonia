using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Views.Modals;
using ELOR.VKAPILib.Objects;
using System.Collections.ObjectModel;
using VKUI.Controls;
using VKUI.Popups;

namespace ELOR.Laney.ViewModels.Controls {
    public class ComposerViewModel : CommonViewModel {
        private bool _isEditMode;
        private string _text;
        private ObservableCollection<OutboundAttachmentViewModel> _attachments = new ObservableCollection<OutboundAttachmentViewModel>();
        private MessageViewModel _reply;
        private BotKeyboard _botKeyboard;

        public bool IsEditMode { get { return _isEditMode; } set { _isEditMode = value; OnPropertyChanged(); } }
        public string Text { get { return _text; } set { _text = value; OnPropertyChanged(); } }
        public ObservableCollection<OutboundAttachmentViewModel> Attachments { get { return _attachments; } set { _attachments = value; OnPropertyChanged(); } }
        public MessageViewModel Reply { get { return _reply; } set { _reply = value; OnPropertyChanged(); } }
        public BotKeyboard BotKeyboard { get { return _botKeyboard; } set { _botKeyboard = value; OnPropertyChanged(); } }

        ChatViewModel Chat;

        public ComposerViewModel(ChatViewModel chat) {
            Chat = chat;
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

            photo.Click += (a, b) => {
                AttachmentPicker ap = new AttachmentPicker(session, 10 - Attachments.Count, 0);
                ap.ShowDialog<object>(session.Window);
            };
            video.Click += (a, b) => {
                AttachmentPicker ap = new AttachmentPicker(session, 10 - Attachments.Count, 1);
                ap.ShowDialog<object>(session.Window);
            };
            file.Click += (a, b) => {
                AttachmentPicker ap = new AttachmentPicker(session, 10 - Attachments.Count, 2);
                ap.ShowDialog<object>(session.Window);
            };
            poll.Click += (a, b) => {
                
            };

            ash.Items.Add(photo);
            ash.Items.Add(video);
            ash.Items.Add(file);
            ash.Items.Add(poll);
            ash.ShowAt(target);
        }
    }
}
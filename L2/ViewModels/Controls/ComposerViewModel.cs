﻿using Avalonia.Controls;
using Avalonia.Platform.Storage.FileIO;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
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

        public ComposerViewModel(VKSession session, ChatViewModel chat) {
            this.session = session;
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
    }
}
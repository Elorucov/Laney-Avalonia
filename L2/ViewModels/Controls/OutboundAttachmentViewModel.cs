using Avalonia.Media.Imaging;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.ObjectModel;
using VKUI.Controls;

namespace ELOR.Laney.ViewModels.Controls {
    public enum OutboundAttachmentType { Attachment, ForwardedMessages, Place }
    public enum OutboundAttachmentUploadState { Unknown, InProgress, Success, Failed }
    public enum OutboundAttachmentUploadFileType { Photo, Video, Doc, AudioMessage, Audio }

    public class OutboundAttachmentViewModel : CommonViewModel {
        private OutboundAttachmentType _type;
        private string _iconId;
        private string _displayName;
        private AttachmentBase _attachment;
        private Bitmap _previewImage;
        private string _extraInfo;
        private double _uploadProgress;
        private OutboundAttachmentUploadState _uploadState;
        private ObservableCollection<MessageViewModel> _forwardedMessages;
        private Tuple<double, double> _place;

        public OutboundAttachmentType Type { get { return _type; } private set { _type = value; OnPropertyChanged(); } }
        public string IconId { get { return _iconId; } private set { _iconId = value; OnPropertyChanged(); } }
        public string DisplayName { get { return _displayName; } private set { _displayName = value; OnPropertyChanged(); } }
        public AttachmentBase Attachment { get { return _attachment; } private set { _attachment = value; OnPropertyChanged(); } }
        public Bitmap PreviewImage { get { return _previewImage; } private set { _previewImage = value; OnPropertyChanged(); } }
        public string ExtraInfo { get { return _extraInfo; } private set { _extraInfo = value; OnPropertyChanged(); } }
        public double UploadProgress { get { return _uploadProgress; } private set { _uploadProgress = value; OnPropertyChanged(); } }
        public OutboundAttachmentUploadState UploadState { get { return _uploadState; } private set { _uploadState = value; OnPropertyChanged(); } }
        public ObservableCollection<MessageViewModel> ForwardedMessages { get { return _forwardedMessages; } private set { _forwardedMessages = value; OnPropertyChanged(); } }
        public Tuple<double, double> Place { get { return _place; } private set { _place = value; OnPropertyChanged(); } }

        public OutboundAttachmentViewModel(AttachmentBase attachment) {
            UploadState = OutboundAttachmentUploadState.Success;
            Log.Information($"Init outbound attachment {attachment}");
            switch (attachment.GetType().Name) {
                case nameof(Photo): SetUp(attachment as Photo); break;
                case nameof(Video): SetUp(attachment as Video); break;
                case nameof(Document): SetUp(attachment as Document); break;
                case nameof(Poll): SetUp(attachment as Poll); break;
                case nameof(AudioMessage): SetUp(attachment as AudioMessage); break;
                case nameof(Audio): SetUp(attachment as Audio); break;
                case nameof(Podcast): SetUp(attachment as Podcast); break;
                case nameof(Graffiti): SetUp(attachment as Graffiti); break;
                case nameof(Story): SetUp(attachment as Story); break;
                default:
                    Attachment = attachment;
                    IconId = VKIconNames.Icon24Question;
                    DisplayName = attachment.ObjectType;
                    Log.Warning($"Type of attachment {attachment} is unsupported!");
                    break;
            }
        }

        #region Setup

        private async void SetUp(Photo p) {
            IconId = VKIconNames.Icon24Gallery;
            PreviewImage = await LNetExtensions.GetBitmapAsync(p.GetSizeAndUriForThumbnail().Uri);
            Attachment = p;
        }

        private async void SetUp(Video v) {
            IconId = VKIconNames.Icon24Video;
            PreviewImage = await LNetExtensions.GetBitmapAsync(v.GetSizeAndUriForThumbnail().Uri);
            ExtraInfo = v.DurationTime.ToString(@"h\:mm\:ss");
            Attachment = v;
        }

        private async void SetUp(Document d) {
            IconId = VKIconNames.Icon24Document;
            if (d.Preview != null) {
                PreviewImage = await LNetExtensions.GetBitmapAsync(d.GetSizeAndUriForThumbnail(88).Uri);
                ExtraInfo = d.Extension.ToUpper();
            } else {
                DisplayName = d.Title;
            }
            Attachment = d;
        }

        private void SetUp(Poll p) {
            IconId = VKIconNames.Icon24Poll;
            DisplayName = p.Question;
            Attachment = p;
        }

        private void SetUp(AudioMessage a) {
            IconId = VKIconNames.Icon24Voice;
            DisplayName = a.DurationTime.ToString(@"h\:mm\:ss");
            Attachment = a;
        }

        private void SetUp(Audio a) {
            IconId = VKIconNames.Icon24Music;
            DisplayName = a.Title;
            Attachment = a;
        }

        private void SetUp(Podcast p) {
            IconId = VKIconNames.Icon24Music;
            DisplayName = p.Title;
            Attachment = p;
        }

        private void SetUp(Graffiti g) {
            IconId = VKIconNames.Icon24BrushOutline;
            DisplayName = Localizer.Instance["atch_graffiti"];
            Attachment = g;
        }

        private void SetUp(Story s) {
            IconId = VKIconNames.Icon24Story;
            DisplayName = Localizer.Instance["atch_story"];
            Attachment = s;
        }

        #endregion
    }
}
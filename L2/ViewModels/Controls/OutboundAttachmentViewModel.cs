using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media.Imaging;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
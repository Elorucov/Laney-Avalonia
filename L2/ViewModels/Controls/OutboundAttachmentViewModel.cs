﻿using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage.FileIO;
using Avalonia.Threading;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Core.Network;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;
using ELOR.VKAPILib.Objects.Upload;
using ExCSS;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using VKUI.Controls;

namespace ELOR.Laney.ViewModels.Controls {
    public enum OutboundAttachmentType { Attachment, ForwardedMessages, Place }
    public enum OutboundAttachmentUploadFileType { Photo, Video, Doc, AudioMessage, Audio }

    public class OutboundAttachmentViewModel : CommonViewModel {
        private VKSession session;

        private OutboundAttachmentType _type;
        private string _iconId;
        private string _displayName;
        private AttachmentBase _attachment;
        private Bitmap _previewImage;
        private string _extraInfo;
        private bool _isUploading;
        private double _uploadProgress;
        private Exception _uploadException;
        private ObservableCollection<MessageViewModel> _forwardedMessages;
        private Tuple<double, double> _place;

        public OutboundAttachmentType Type { get { return _type; } private set { _type = value; OnPropertyChanged(); } }
        public string IconId { get { return _iconId; } private set { _iconId = value; OnPropertyChanged(); } }
        public string DisplayName { get { return _displayName; } private set { _displayName = value; OnPropertyChanged(); } }
        public AttachmentBase Attachment { get { return _attachment; } private set { _attachment = value; OnPropertyChanged(); } }
        public Bitmap PreviewImage { get { return _previewImage; } private set { _previewImage = value; OnPropertyChanged(); } }
        public string ExtraInfo { get { return _extraInfo; } private set { _extraInfo = value; OnPropertyChanged(); } }
        public bool IsUploading { get { return _isUploading; } private set { _isUploading = value; OnPropertyChanged(); } }
        public double UploadProgress { get { return _uploadProgress; } private set { _uploadProgress = value; OnPropertyChanged(); } }
        public Exception UploadException { get { return _uploadException; } private set { _uploadException = value; OnPropertyChanged(); } }
        public ObservableCollection<MessageViewModel> ForwardedMessages { get { return _forwardedMessages; } private set { _forwardedMessages = value; OnPropertyChanged(); } }
        public Tuple<double, double> Place { get { return _place; } private set { _place = value; OnPropertyChanged(); } }

        OutboundAttachmentUploadFileType uploadFileType = OutboundAttachmentUploadFileType.Doc;
        BclStorageFile file = null;

        public OutboundAttachmentViewModel(VKSession session, AttachmentBase attachment) {
            this.session = session;
            Type = OutboundAttachmentType.Attachment;
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

        public OutboundAttachmentViewModel(VKSession session, BclStorageFile file, int type) {
            this.session = session;
            this.file = file;
            IsUploading = true;
            Type = OutboundAttachmentType.Attachment;
            Log.Information($"Init outbound attachment {file.Name}");
            PrepareToUpload(type);
        }

        #region Setup
        int width = Constants.OutboundAttachmentUIWidth;

        private async void SetUp(Photo p) {
            IconId = VKIconNames.Icon24Gallery;
            PreviewImage = await LNetExtensions.GetBitmapAsync(p.GetSizeAndUriForThumbnail(width).Uri, width);
            Attachment = p;
        }

        private async void SetUp(Video v) {
            IconId = VKIconNames.Icon24Video;
            DisplayName = v.Title;
            if (v.Image != null) PreviewImage = await LNetExtensions.GetBitmapAsync(v.GetSizeAndUriForThumbnail(width).Uri, width);
            ExtraInfo = v.DurationTime.ToString(@"h\:mm\:ss");
            Attachment = v;
        }

        private async void SetUp(Document d) {
            IconId = VKIconNames.Icon24Document;
            if (d.Preview != null) {
                PreviewImage = await LNetExtensions.GetBitmapAsync(d.GetSizeAndUriForThumbnail(width).Uri, width);
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

        private async void PrepareToUpload(int type) {
            DisplayName = file.Name;
            uploadFileType = OutboundAttachmentUploadFileType.Doc;

            switch (type) {
                case Constants.PhotoUploadCommand:
                    uploadFileType = OutboundAttachmentUploadFileType.Photo;
                    IconId = VKIconNames.Icon24Gallery;
                    break;
                case Constants.VideoUploadCommand:
                    uploadFileType = OutboundAttachmentUploadFileType.Video;
                    IconId = VKIconNames.Icon24Video;
                    break;
                case Constants.FileUploadCommand:
                    uploadFileType = OutboundAttachmentUploadFileType.Doc;
                    IconId = VKIconNames.Icon24Document;
                    break;
            }

            if (type == Constants.PhotoUploadCommand) {
                await Task.Delay(100); // надо, чтобы UI вложения появился перед получением превьюхи
                Stream stream = await file.OpenReadAsync();
                Bitmap bitmap = await stream.TryGetBitmapFromStreamAsync(width);
                PreviewImage = bitmap;
                await stream.FlushAsync();
            }

            await DoUploadFileInternal(file, uploadFileType);
        }

        #endregion

        private IFileUploader uploader;

        public async void DoUploadFile() {
            if (!IsUploading) await DoUploadFileInternal(file, uploadFileType);
        }

        private async Task<bool> DoUploadFileInternal(BclStorageFile file, OutboundAttachmentUploadFileType uploadFileType) {
            UploadException = null;
            UploadProgress = 0;
            IsUploading = true;

            try {
                VkUploadServer server;
                switch (uploadFileType) {
                    case OutboundAttachmentUploadFileType.Photo:
                        server = await session.API.Photos.GetMessagesUploadServerAsync(session.GroupId);
                        PhotoUploadServer pus = server as PhotoUploadServer;
                        uploader = new VKHttpClientFileUploader("photo", pus.Uri, file);
                        uploader.UploadFailed += Uploader_UploadFailed;
                        uploader.ProgressChanged += Uploader_ProgressChanged;
                        var pr = await uploader.UploadAsync();
                        if (pr == null) throw new ArgumentNullException("Upload error, no response!");
                        UploadProgress = 100;
                        PhotoUploadResult pur = JsonConvert.DeserializeObject<PhotoUploadResult>(pr);
                        if (String.IsNullOrEmpty(pur.Photo)) throw new Exception("File is not uploaded!");
                        var presult = await session.API.Photos.SaveMessagesPhotoAsync(session.GroupId, pur.Server, pur.Photo, pur.Hash);
                        if (presult.Count > 0) {
                            PhotoSaveResult psr = presult[0];
                            SetUp(new Photo { Id = psr.Id, OwnerId = psr.OwnerId, AccessKey = psr.AccessKey, Sizes = psr.Sizes });
                        } else {
                            throw new ArgumentException("photos.save response is incorrect!");
                        }
                        break;
                    case OutboundAttachmentUploadFileType.Doc:
                    case OutboundAttachmentUploadFileType.AudioMessage:
                        server = await session.API.Docs.GetMessagesUploadServerAsync(session.GroupId, 0, uploadFileType == OutboundAttachmentUploadFileType.AudioMessage);
                        uploader = new VKHttpClientFileUploader("file", server.Uri, file);
                        uploader.UploadFailed += Uploader_UploadFailed;
                        uploader.ProgressChanged += Uploader_ProgressChanged;
                        var dr = await uploader.UploadAsync();
                        if (dr == null) throw new ArgumentNullException("Upload error, no response!");
                        UploadProgress = 100;
                        DocumentUploadResult dur = JsonConvert.DeserializeObject<DocumentUploadResult>(dr);
                        if (String.IsNullOrEmpty(dur.File)) {
                            throw new Exception(!String.IsNullOrEmpty(dur.ErrorDescription) ? dur.ErrorDescription : dur.Error);
                        }
                        var dresult = await session.API.Docs.SaveAsync(session.GroupId, dur.File, file.Name);
                        switch (dresult.Type) {
                            case AttachmentType.AudioMessage:
                                SetUp(dresult.AudioMessage); break;
                            case AttachmentType.Graffiti:
                                SetUp(dresult.Graffiti); break;
                            case AttachmentType.Document:
                                SetUp(dresult.Document); break;
                        }
                        break;
                    case OutboundAttachmentUploadFileType.Video:
                        server = await session.API.Video.SaveAsync(session.GroupId, file.Name, String.Empty, true);
                        uploader = new VKHttpClientFileUploader("video_file", server.Uri, file);
                        uploader.UploadFailed += Uploader_UploadFailed;
                        uploader.ProgressChanged += Uploader_ProgressChanged;
                        var vr = await uploader.UploadAsync();
                        if (vr == null) throw new ArgumentNullException("Upload error, no response!");
                        UploadProgress = 100;
                        VideoUploadResult vur = JsonConvert.DeserializeObject<VideoUploadResult>(vr);
                        Video video = new Video { 
                            Id = vur.VideoId,
                            OwnerId = vur.OwnerId,
                            AccessKey = (server as VideoUploadServer).AccessKey,
                            Title = file.Name
                        };
                        var videoapi = await session.API.Video.GetAsync(session.GroupId, $"{vur.OwnerId}_{vur.VideoId}_{video.AccessKey}");
                        if (videoapi.Items.Count > 0) {
                            video = videoapi.Items[0];
                        }
                        SetUp(video);
                        ExtraInfo = "--:--";
                        break;
                }
                IsUploading = false;
                UploadProgress = 0;
                return true;
            } catch (Exception ex) {
                Log.Error($"File upload error! Type: {uploadFileType}", ex);
                UploadException = ex;
                IsUploading = false;
                UploadProgress = 0;
                return false;
            }
        }

        private async void Uploader_UploadFailed(object sender, Exception e) {
            await Dispatcher.UIThread.InvokeAsync(() => {
                Log.Error($"Exception was thrown in uploader!", e);
                UploadException = e;
                IsUploading = false;
                UploadProgress = 0;
            });
        }

        private async void Uploader_ProgressChanged(object sender, double percent) {
            await Dispatcher.UIThread.InvokeAsync(() => UploadProgress = percent);
            //switch (uploadFileType) {
            //    case OutboundAttachmentUploadFileType.Photo:
            //        APIHelper.SendActivity(ELOR.VKAPILib.Methods.ActivityType.Photo, peerId);
            //        break;
            //    case OutboundAttachmentUploadFileType.Video:
            //        APIHelper.SendActivity(ELOR.VKAPILib.Methods.ActivityType.Video, peerId);
            //        break;
            //    case OutboundAttachmentUploadFileType.Doc:
            //        APIHelper.SendActivity(ELOR.VKAPILib.Methods.ActivityType.File, peerId);
            //        break;
            //    case OutboundAttachmentUploadFileType.AudioMessage:
            //        APIHelper.SendActivity(ELOR.VKAPILib.Methods.ActivityType.Audiomessage, peerId);
            //        break;
            //}
        }
    
        public void CancelUpload() {
            uploader?.Cancel();
            uploader = null;
        }
    }
}
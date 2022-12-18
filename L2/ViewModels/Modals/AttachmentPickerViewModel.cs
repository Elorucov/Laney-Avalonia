using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Execute;
using ELOR.Laney.Execute.Objects;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;

namespace ELOR.Laney.ViewModels.Modals {
    public class AttachmentPickerViewModel : ViewModelBase {
        private ObservableCollection<AlbumLite> _photoAlbums;
        private AlbumLite _selectedPhotoAlbum;
        private ObservableCollection<AlbumLite> _videoAlbums;
        private AlbumLite _selectedVideoAlbum;
        private int _documentTypeIndex = -1;

        private CollectionViewModel<Photo> _photos = new CollectionViewModel<Photo>();
        private CollectionViewModel<Video> _videos = new CollectionViewModel<Video>();
        private CollectionViewModel<Document> _documents = new CollectionViewModel<Document>();
        private ObservableCollection<AttachmentBase> _selectedAttachments = new ObservableCollection<AttachmentBase>();
        private int _selectedAttachmentsCount;

        public ObservableCollection<AlbumLite> PhotoAlbums { get { return _photoAlbums; } private set { _photoAlbums = value; OnPropertyChanged(); } }
        public AlbumLite SelectedPhotoAlbum { get { return _selectedPhotoAlbum; } set { _selectedPhotoAlbum = value; OnPropertyChanged(); } }
        public ObservableCollection<AlbumLite> VideoAlbums { get { return _videoAlbums; } private set { _videoAlbums = value; OnPropertyChanged(); } }
        public AlbumLite SelectedVideoAlbum { get { return _selectedVideoAlbum; } set { _selectedVideoAlbum = value; OnPropertyChanged(); } }
        public int DocumentTypeIndex { get { return _documentTypeIndex; } set { _documentTypeIndex = value; OnPropertyChanged(); } }

        public CollectionViewModel<Photo> Photos { get { return _photos; } private set { _photos = value; OnPropertyChanged(); } }
        public CollectionViewModel<Video> Videos { get { return _videos; } private set { _videos = value; OnPropertyChanged(); } }
        public CollectionViewModel<Document> Documents { get { return _documents; } private set { _documents = value; OnPropertyChanged(); } }
        public ObservableCollection<AttachmentBase> SelectedAttachments { get { return _selectedAttachments; } set { _selectedAttachments = value; OnPropertyChanged(); } }
        public int SelectedAttachmentsCount { get { return _selectedAttachmentsCount; } private set { _selectedAttachmentsCount = value; OnPropertyChanged(); } }

        private bool noMorePhotos = false;
        private bool noMoreVideos = false;
        private bool noMoreDocs = false;
        private VKSession session;

        public AttachmentPickerViewModel(VKSession session) {
            this.session = session;
            PropertyChanged += (a, b) => {
                switch (b.PropertyName) {
                    case nameof(SelectedPhotoAlbum): Photos.Items.Clear(); noMorePhotos = false; LoadPhotos(); break;
                    case nameof(SelectedVideoAlbum): Videos.Items.Clear(); noMoreVideos = false; LoadVideos(); break;
                    case nameof(DocumentTypeIndex): Documents.Items.Clear(); noMoreDocs = false; LoadDocuments(); break;
                }
            };

            SelectedAttachments.CollectionChanged += UpdateCounter;
        }

        private void UpdateCounter(object sender, NotifyCollectionChangedEventArgs e) {
            SelectedAttachmentsCount = SelectedAttachments.Count;
        }

        #region Photos

        public async void LoadPhotoAlbums() {
            if (Photos.IsLoading) return;
            Photos.Placeholder = null;
            Photos.IsLoading = true;

            var albums = await session.API.GetPhotoAlbumsAsync(session.Id);
            try {
                if (albums.Count > 0) albums[0].Title = Localizer.Instance["all"];
                PhotoAlbums = new ObservableCollection<AlbumLite>(albums);
                Photos.IsLoading = false;
                SelectedPhotoAlbum = PhotoAlbums.First();
            } catch (Exception ex) {
                Photos.IsLoading = false;
                Photos.Placeholder = PlaceholderViewModel.GetForException(ex, () => LoadPhotoAlbums());
            }
        }

        public async void LoadPhotos() {
            if (Photos.IsLoading || noMorePhotos) return;
            Photos.Placeholder = null;
            Photos.IsLoading = true;

            VKList<Photo> photos = null;
            switch (SelectedPhotoAlbum.Id) {
                case 0:
                    photos = await session.API.Photos.GetAllAsync(session.Id, true, Photos.Items.Count, 100);
                    break;
                case -9000:
                    photos = await session.API.Photos.GetUserPhotosAsync(session.Id, Photos.Items.Count, 100);
                    break;
                default:
                    photos = await session.API.Photos.GetAsync(session.Id, SelectedPhotoAlbum.Id, null, true, false, Photos.Items.Count, 100);
                    break;
            }

            try {
                noMorePhotos = photos.Items.Count == 0;
                photos.Items.ForEach((p) => Photos.Items.Add(p));
            } catch (Exception ex) {
                if (Photos.Items.Count == 0) {
                    Photos.Placeholder = PlaceholderViewModel.GetForException(ex, () => LoadPhotos());
                } else {
                    // TODO: Error dialog.
                }
            }
            Photos.IsLoading = false;
        }

        #endregion

        #region Videos

        public async void LoadVideoAlbums() {
            if (Videos.IsLoading) return;
            Videos.Placeholder = null;
            Videos.IsLoading = true;

            var albums = await session.API.GetVideoAlbumsAsync(session.Id);
            try {
                VideoAlbums = new ObservableCollection<AlbumLite>(albums);
                Videos.IsLoading = false;
                SelectedVideoAlbum = VideoAlbums.First();
            } catch (Exception ex) {
                Videos.IsLoading = false;
                Videos.Placeholder = PlaceholderViewModel.GetForException(ex, () => LoadVideoAlbums());
            }
        }

        public async void LoadVideos() {
            if (Videos.IsLoading || noMoreVideos) return;
            Videos.Placeholder = null;
            Videos.IsLoading = true;

            VKList<Video> videos = await session.API.Video.GetAsync(session.Id, null, SelectedVideoAlbum.Id, Videos.Items.Count, 50, true);

            try {
                noMoreVideos = videos.Items.Count == 0;
                videos.Items.ForEach((v) => Videos.Items.Add(v));
            } catch (Exception ex) {
                if (Videos.Items.Count == 0) {
                    Videos.Placeholder = PlaceholderViewModel.GetForException(ex, () => LoadVideos());
                } else {
                    // TODO: Error dialog.
                }
            }
            Videos.IsLoading = false;
        }

        #endregion

        #region Files

        public async void LoadDocuments() {
            if (Documents.IsLoading || noMoreDocs) return;
            Documents.Placeholder = null;
            Documents.IsLoading = true;

            try {
                VKList<Document> docs = await session.API.Docs.GetAsync(session.Id, DocumentTypeIndex, Documents.Items.Count, 50);
                docs.Items.ForEach(d => Documents.Items.Add(d));
            } catch (Exception ex) {
                if (Documents.Items.Count == 0) {
                    Documents.Placeholder = PlaceholderViewModel.GetForException(ex, () => LoadDocuments());
                } else {
                    // TODO: Error dialog.
                }
            }

            Documents.IsLoading = false;
        }

        #endregion
    }
}
using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Execute;
using ELOR.Laney.Execute.Objects;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

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
        private Window ownerWindow;

        public AttachmentPickerViewModel(VKSession session, Window owner) {
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

            try {
                var albums = await session.API.GetPhotoAlbumsAsync(session.Id);
                if (albums.Count > 0) albums[0].Title = Assets.i18n.Resources.all;
                PhotoAlbums = new ObservableCollection<AlbumLite>(albums);
                Photos.IsLoading = false;
                SelectedPhotoAlbum = PhotoAlbums.First();
            } catch (Exception ex) {
                Photos.IsLoading = false;
                Photos.Placeholder = PlaceholderViewModel.GetForException(ex, (o) => LoadPhotoAlbums());
            }
        }

        public async void LoadPhotos() {
            if (Photos.IsLoading || noMorePhotos) return;
            Photos.Placeholder = null;
            Photos.IsLoading = true;

            PhotosList photos = null;
            try {
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

                noMorePhotos = photos.Items.Count == 0;
                photos.Items.ForEach(Photos.Items.Add);
            } catch (Exception ex) {
                if (Photos.Items.Count == 0) {
                    Photos.Placeholder = PlaceholderViewModel.GetForException(ex, (o) => LoadPhotos());
                } else {
                    if (await ExceptionHelper.ShowErrorDialogAsync(ownerWindow, ex)) LoadPhotos();
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

            try {
                var albums = await session.API.GetVideoAlbumsAsync(session.Id);
                VideoAlbums = new ObservableCollection<AlbumLite>(albums);
                Videos.IsLoading = false;
                SelectedVideoAlbum = VideoAlbums.First();
            } catch (Exception ex) {
                Videos.IsLoading = false;
                Videos.Placeholder = PlaceholderViewModel.GetForException(ex, (o) => LoadVideoAlbums());
            }
        }

        public async void LoadVideos() {
            if (Videos.IsLoading || noMoreVideos) return;
            Videos.Placeholder = null;
            Videos.IsLoading = true;

            try {
                VideosList videos = await session.API.Video.GetAsync(session.Id, null, SelectedVideoAlbum.Id, Videos.Items.Count, 50, true);
                noMoreVideos = videos.Items.Count == 0;
                videos.Items.ForEach(Videos.Items.Add);
            } catch (Exception ex) {
                if (Videos.Items.Count == 0) {
                    Videos.Placeholder = PlaceholderViewModel.GetForException(ex, (o) => LoadVideos());
                } else {
                    if (await ExceptionHelper.ShowErrorDialogAsync(ownerWindow, ex)) LoadVideos();
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
                DocumentsList docs = await session.API.Docs.GetAsync(session.Id, DocumentTypeIndex, Documents.Items.Count, 50);
                docs.Items.ForEach(d => Documents.Items.Add(d));
            } catch (Exception ex) {
                if (Documents.Items.Count == 0) {
                    Documents.Placeholder = PlaceholderViewModel.GetForException(ex, (o) => LoadDocuments());
                } else {
                    if (await ExceptionHelper.ShowErrorDialogAsync(ownerWindow, ex)) LoadDocuments();
                }
            }

            Documents.IsLoading = false;
        }

        #endregion
    }
}
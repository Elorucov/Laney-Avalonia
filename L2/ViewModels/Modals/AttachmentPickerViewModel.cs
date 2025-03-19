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
using System.Threading.Tasks;

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
            PropertyChanged += async (a, b) => {
                switch (b.PropertyName) {
                    case nameof(SelectedPhotoAlbum): Photos.Items.Clear(); noMorePhotos = false; await LoadPhotosAsync(); break;
                    case nameof(SelectedVideoAlbum): Videos.Items.Clear(); noMoreVideos = false; await LoadVideosAsync(); break;
                    case nameof(DocumentTypeIndex): Documents.Items.Clear(); noMoreDocs = false; await LoadDocumentsAsync(); break;
                }
            };

            SelectedAttachments.CollectionChanged += UpdateCounter;
        }

        private void UpdateCounter(object sender, NotifyCollectionChangedEventArgs e) {
            SelectedAttachmentsCount = SelectedAttachments.Count;
        }

        #region Photos

        public async Task LoadPhotoAlbumsAsync() {
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
                Photos.Placeholder = PlaceholderViewModel.GetForException(ex, async (o) => await LoadPhotoAlbumsAsync());
            }
        }

        public async Task LoadPhotosAsync() {
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
                    Photos.Placeholder = PlaceholderViewModel.GetForException(ex, async (o) => await LoadPhotosAsync());
                } else {
                    if (await ExceptionHelper.ShowErrorDialogAsync(ownerWindow, ex)) await LoadPhotosAsync();
                }
            }
            Photos.IsLoading = false;
        }

        #endregion

        #region Videos

        public async Task LoadVideoAlbumsAsync() {
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
                Videos.Placeholder = PlaceholderViewModel.GetForException(ex, async (o) => await LoadVideoAlbumsAsync());
            }
        }

        public async Task LoadVideosAsync() {
            if (Videos.IsLoading || noMoreVideos) return;
            Videos.Placeholder = null;
            Videos.IsLoading = true;

            try {
                VideosList videos = await session.API.Video.GetAsync(session.Id, null, SelectedVideoAlbum.Id, Videos.Items.Count, 50, true);
                noMoreVideos = videos.Items.Count == 0;
                videos.Items.ForEach(Videos.Items.Add);
            } catch (Exception ex) {
                if (Videos.Items.Count == 0) {
                    Videos.Placeholder = PlaceholderViewModel.GetForException(ex, async (o) => await LoadVideosAsync());
                } else {
                    if (await ExceptionHelper.ShowErrorDialogAsync(ownerWindow, ex)) await LoadVideosAsync();
                }
            }
            Videos.IsLoading = false;
        }

        #endregion

        #region Files

        public async Task LoadDocumentsAsync() {
            if (Documents.IsLoading || noMoreDocs) return;
            Documents.Placeholder = null;
            Documents.IsLoading = true;

            try {
                DocumentsList docs = await session.API.Docs.GetAsync(session.Id, DocumentTypeIndex, Documents.Items.Count, 50);
                docs.Items.ForEach(d => Documents.Items.Add(d));
            } catch (Exception ex) {
                if (Documents.Items.Count == 0) {
                    Documents.Placeholder = PlaceholderViewModel.GetForException(ex, async (o) => await LoadDocumentsAsync());
                } else {
                    if (await ExceptionHelper.ShowErrorDialogAsync(ownerWindow, ex)) await LoadDocumentsAsync();
                }
            }

            Documents.IsLoading = false;
        }

        #endregion
    }
}
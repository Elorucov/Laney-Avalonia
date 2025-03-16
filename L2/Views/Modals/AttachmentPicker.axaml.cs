using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels.Modals;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals {
    public partial class AttachmentPicker : DialogWindow {
        private AttachmentPickerViewModel ViewModel { get { return DataContext as AttachmentPickerViewModel; } }
        private int Limit;

        public AttachmentPicker() {
            InitializeComponent();
        }

        public AttachmentPicker(VKSession session, int limit, int tab = 0) {
            InitializeComponent();
            Unloaded += (a, b) => BitmapManager.ClearCachedImages();

            Limit = limit;
            Tag = session;
            DataContext = new AttachmentPickerViewModel(session, this);
            this.FixDialogWindows(TitleBar, Tabs);

            RectangleGeometry rg = new RectangleGeometry(new Avalonia.Rect(Width - 64, 0, Width, 48));
            TitleBar.Clip = rg;

            PhotosSV.RegisterIncrementalLoadingEvent(LoadMorePhotos);
            VideosSV.RegisterIncrementalLoadingEvent(LoadMoreVideos);
            DocsSV.RegisterIncrementalLoadingEvent(LoadMoreDocs);

            Tabs.SelectedIndex = tab;
            LoadTab(tab);
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Tabs != null) LoadTab(Tabs.SelectedIndex);
        }

        private void LoadTab(int index) {
            switch (index) {
                case 0: if (ViewModel.PhotoAlbums == null) ViewModel.LoadPhotoAlbums(); break;
                case 1: if (ViewModel.VideoAlbums == null) ViewModel.LoadVideoAlbums(); break;
                case 2: if (ViewModel.DocumentTypeIndex < 0) ViewModel.DocumentTypeIndex = 0; break;
            }
        }

        private void ListSelectionChanged(object sender, SelectionChangedEventArgs e) {
            ListBox listBox = sender as ListBox;

            if (e.AddedItems.Count > 0) {
                if (ViewModel.SelectedAttachmentsCount >= Limit) {
                    if (e.AddedItems.Count == 1) listBox.SelectedItems.Remove(e.AddedItems[0]);
                } else {
                    ViewModel.SelectedAttachments.Add(e.AddedItems[0] as AttachmentBase);
                }
            }
            if (e.RemovedItems.Count > 0) {
                ViewModel.SelectedAttachments.Remove(e.RemovedItems[0] as AttachmentBase);
            }
        }

        private void LoadMorePhotos() {
            ViewModel.LoadPhotos();
        }

        private void LoadMoreVideos() {
            ViewModel.LoadVideos();
        }

        private void LoadMoreDocs() {
            ViewModel.LoadDocuments();
        }

        private async void OpenFilePickerForPhoto(object sender, RoutedEventArgs e) {
            if (!StorageProvider.CanOpen) return;
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                AllowMultiple = true,
                FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.ImageAll }
            });
            if (files.Count > 0) Close(new Tuple<int, List<IStorageFile>>(Constants.PhotoUploadCommand, files.ToList()));
        }

        private async void OpenFilePickerForVideo(object sender, RoutedEventArgs e) {
            if (!StorageProvider.CanOpen) return;
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                AllowMultiple = true,
                FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.All } // Video!!!
            });
            if (files.Count > 0) Close(new Tuple<int, List<IStorageFile>>(Constants.VideoUploadCommand, files.ToList()));
        }

        private async void OpenFilePickerForDoc(object sender, RoutedEventArgs e) {
            if (!StorageProvider.CanOpen) return;
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                AllowMultiple = true,
                FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.All }
            });
            if (files.Count > 0) Close(new Tuple<int, List<IStorageFile>>(Constants.FileUploadCommand, files.ToList()));
        }

        private void CloseAndAttach(object sender, RoutedEventArgs e) {
            Close(ViewModel.SelectedAttachments.ToList());
        }
    }
}
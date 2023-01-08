using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Platform.Storage.FileIO;
using DynamicData;
using ELOR.Laney.Core;
using ELOR.Laney.ViewModels.Modals;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
            Limit = limit;
            DataContext = new AttachmentPickerViewModel(session);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                Grid.SetRow(Tabs, 1);
                Grid.SetRowSpan(Tabs, 1);
                TitleBar.CanShowTitleAndMove = true;
            }

            PhotosList.SelectionChanged += ListSelectionChanged;
            VideosList.SelectionChanged += ListSelectionChanged;
            DocsList.SelectionChanged += ListSelectionChanged;

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

        private async void OpenFilePickerForPhoto(object sender, RoutedEventArgs e) {
            if (!StorageProvider.CanOpen) return;
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { 
                AllowMultiple = true,
                FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.ImageAll }
            });
            if (files.Count > 0) Close(new Tuple<int, List<BclStorageFile>>(Constants.PhotoUploadCommand, files.Cast<BclStorageFile>().ToList()));
        }

        private async void OpenFilePickerForVideo(object sender, RoutedEventArgs e) {
            if (!StorageProvider.CanOpen) return;
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                AllowMultiple = true,
                FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.All } // Video!!!
            });
            if (files.Count > 0) Close(new Tuple<int, List<BclStorageFile>>(Constants.VideoUploadCommand, files.Cast<BclStorageFile>().ToList()));
        }

        private async void OpenFilePickerForDoc(object sender, RoutedEventArgs e) {
            if (!StorageProvider.CanOpen) return;
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                AllowMultiple = true,
                FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.All }
            });
            if (files.Count > 0) Close(new Tuple<int, List<BclStorageFile>>(Constants.FileUploadCommand, files.Cast<BclStorageFile>().ToList()));
        }

        private void CloseAndAttach(object sender, RoutedEventArgs e) {
            Close(ViewModel.SelectedAttachments.ToList());
        }
    }
}
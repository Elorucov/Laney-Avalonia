using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using DynamicData;
using ELOR.Laney.Core;
using ELOR.Laney.ViewModels.Modals;
using ELOR.VKAPILib.Objects;
using System;
using System.Runtime.InteropServices;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals {
    public partial class AttachmentPicker : DialogWindow {
        private AttachmentPickerViewModel ViewModel { get { return DataContext as AttachmentPickerViewModel; } }

        public AttachmentPicker() {
            InitializeComponent();
        }

        public AttachmentPicker(VKSession session, int limit, int tab = 0) {
            InitializeComponent();
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
                if (ViewModel.SelectedAttachmentsCount >= 10) {
                    if (e.AddedItems.Count == 1) listBox.SelectedItems.Remove(e.AddedItems[0]);
                } else {
                    ViewModel.SelectedAttachments.Add(e.AddedItems[0] as AttachmentBase);
                }
            }
            if (e.RemovedItems.Count > 0) {
                ViewModel.SelectedAttachments.Remove(e.RemovedItems[0] as AttachmentBase);
            }
        }
    }
}
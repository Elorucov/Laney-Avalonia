using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels.Modals;
using ELOR.Laney.Views.Media;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using VKUI.Controls;
using VKUI.Popups;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals {
    public partial class PeerProfile : DialogWindow {
        PeerProfileViewModel ViewModel { get => DataContext as PeerProfileViewModel; }
        VKSession session;

        public PeerProfile() { }

        public PeerProfile(VKSession session, long peerId) {
            InitializeComponent();
            this.session = session;

            DataContext = new PeerProfileViewModel(session, peerId);
            ViewModel.CloseWindowRequested += ViewModel_CloseWindowRequested;

            if (peerId.IsChat()) {
                Tabs.Items.Remove(UserInfoTab);
            } else {
                Tabs.Items.Remove(ChatMembersTab);
            }

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            Tabs.SelectionChanged += Tabs_SelectionChanged;

            Unloaded += (a, b) => BitmapManager.ClearCachedImages();

            new IncrementalLoader(PhotosSV, ViewModel.LoadPhotos);
            new IncrementalLoader(VideosSV, ViewModel.LoadVideos);
            // TODO: Audios here
            new IncrementalLoader(DocsSV, ViewModel.LoadDocs);
            new IncrementalLoader(LinksSV, ViewModel.LoadLinks);

            // RelativeSource is not working when CompiledBindings=true!
            FirstButton.CommandParameter = FirstButton;
            SecondButton.CommandParameter = SecondButton;
            ThirdButton.CommandParameter = ThirdButton;
            MoreButton.CommandParameter = MoreButton;
#if LINUX
            TitleBar.IsVisible = false;
#elif MAC
            PeerInfo.Margin = new Avalonia.Thickness(24, 28, 24, 24);
#endif
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(PeerProfileViewModel.IsLoading) && !ViewModel.IsLoading) {
                if (ViewModel.Placeholder == null) ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

                // Remove members tab for channels.
                if (ViewModel.Id.IsChat() && ViewModel.DisplayedMembers.Count == 0) Tabs.Items.Remove(ChatMembersTab);
            }
        }

        private void Tabs_SelectionChanged(object sender, Avalonia.Controls.SelectionChangedEventArgs e) {
            if (Tabs == null) return; // Без этого произойдёт краш при открытии PeerProfile, фиг знает кто вызывает это событие, если Tabs == null...
            switch (Tabs.SelectedIndex) {
                case 1:
                    if (ViewModel.Photos.Items.Count == 0 && !ViewModel.Photos.End) ViewModel.LoadPhotos();
                    break;
                case 2:
                    if (ViewModel.Videos.Items.Count == 0 && !ViewModel.Videos.End) ViewModel.LoadVideos();
                    break;
                case 4:
                    if (ViewModel.Documents.Items.Count == 0 && !ViewModel.Documents.End) ViewModel.LoadDocs();
                    break;
                case 5:
                    if (ViewModel.Share.Items.Count == 0 && !ViewModel.Share.End) ViewModel.LoadLinks();
                    break;
            }
        }

        private void ViewModel_CloseWindowRequested(object sender, EventArgs e) {
            (sender as PeerProfileViewModel).CloseWindowRequested -= ViewModel_CloseWindowRequested;
            Close();
        }

        private void OnAttachmentContextRequested(object sender, ContextRequestedEventArgs e) {
            Button b = sender as Button;
            ConversationAttachment a = b.DataContext as ConversationAttachment;
            
            ActionSheet ash = new ActionSheet();
            ActionSheetItem gotomsg = new ActionSheetItem {
                Before = new VKIcon { Id = VKIconNames.Icon20MessageArrowRightOutline },
                Header = Localizer.Instance["go_to_message"]
            };
            gotomsg.Click += (b, c) => {
                session.GoToChat(ViewModel.Id, a.CMID);
                Close();
            };
            ash.Items.Add(gotomsg);
            ash.ShowAt(b, true);
        }

        private void OnAttachmentClick(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Button b = sender as Button;
            ConversationAttachment a = b.DataContext as ConversationAttachment;
            
            switch (a.Attachment.Type) {
                case AttachmentType.Photo:
                    if (a.Attachment.Photo != null) Gallery.Show(new List<IPreview> { a.Attachment.Photo }, a.Attachment.Photo);
                    break;
                case AttachmentType.Document:
                    if (a.Attachment.Document.Type == DocumentType.Image || a.Attachment.Document.Type == DocumentType.GIF) {
                        if (a.Attachment.Document != null) Gallery.Show(new List<IPreview> { a.Attachment.Document }, a.Attachment.Document);
                    }
                    break;
                case AttachmentType.Link:
                    Router.LaunchLink(session, a.Attachment.Link.Uri);
                    break;
            }
        }
    }
}
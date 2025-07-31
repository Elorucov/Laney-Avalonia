using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels;
using ELOR.Laney.ViewModels.Modals;
using ELOR.Laney.Views.Media;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using VKUI.Controls;
using VKUI.Popups;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals {
    public partial class PeerProfile : DialogWindow {
        PeerProfileViewModel ViewModel { get => DataContext as PeerProfileViewModel; }
        VKSession session;

        public PeerProfile() {
            InitializeComponent();
            if (!Design.IsDesignMode) throw new ArgumentException();
        }

        public PeerProfile(VKSession session, long peerId) {
            InitializeComponent();

            if (Design.IsDesignMode)
                return;

            this.session = session;
            Tag = session;

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

            new IncrementalLoader(PhotosSV, async () => await ViewModel.LoadPhotosAsync());
            new IncrementalLoader(VideosSV, async () => await ViewModel.LoadVideosAsync());
            new IncrementalLoader(AudiosSV, async () => await ViewModel.LoadAudiosAsync());
            new IncrementalLoader(DocsSV, async () => await ViewModel.LoadDocsAsync());
            new IncrementalLoader(LinksSV, async () => await ViewModel.LoadLinksAsync());

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

                // Remove members tab for channels or unavailable chats.
                if (ViewModel.Id.IsChat() && ViewModel.ChatMembers == null) Tabs.Items.Remove(ChatMembersTab);
            }
        }

        private void Tabs_SelectionChanged(object sender, Avalonia.Controls.SelectionChangedEventArgs e) {
            if (Tabs == null) return; // Без этого произойдёт краш при открытии PeerProfile, фиг знает кто вызывает это событие, если Tabs == null...

            TabItem tab = Tabs.SelectedItem as TabItem;
            if (tab == null || tab.Tag == null) return;
            int.TryParse(tab.Tag.ToString(), out int id);

            new System.Action(async () => {
                switch (id) {
                    case 1:
                        if (ViewModel.Photos.Items.Count == 0 && !ViewModel.Photos.End) await ViewModel.LoadPhotosAsync();
                        break;
                    case 2:
                        if (ViewModel.Videos.Items.Count == 0 && !ViewModel.Videos.End) await ViewModel.LoadVideosAsync();
                        break;
                    case 3:
                        if (ViewModel.Audios.Items.Count == 0 && !ViewModel.Audios.End) await ViewModel.LoadAudiosAsync();
                        break;
                    case 4:
                        if (ViewModel.Documents.Items.Count == 0 && !ViewModel.Documents.End) await ViewModel.LoadDocsAsync();
                        break;
                    case 5:
                        if (ViewModel.Share.Items.Count == 0 && !ViewModel.Share.End) await ViewModel.LoadLinksAsync();
                        break;
                }
            })();
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
                Header = Assets.i18n.Resources.go_to_message
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
                    // TODO: Show all loaded photos in gallery.
                    if (a.Attachment.Photo != null) Gallery.Show(new List<IPreview> { a.Attachment.Photo }, a.Attachment.Photo);
                    break;
                case AttachmentType.Audio:
                    if (a.Attachment.Audio != null) {
                        // TODO: remove duplicate audios before sending list to audioplayer.
                        List<Audio> audios = ViewModel.Audios.Items.Select(aa => aa.Attachment.Audio).ToList();
                        AudioPlayerViewModel.PlaySong(audios, a.Attachment.Audio, ViewModel.Header);
                    }
                    break;
                case AttachmentType.Document:
                    if (a.Attachment.Document.Type == DocumentType.Image || a.Attachment.Document.Type == DocumentType.GIF) {
                        if (a.Attachment.Document != null) Gallery.Show(new List<IPreview> { a.Attachment.Document }, a.Attachment.Document);
                    }
                    break;
                case AttachmentType.Link:
                    new System.Action(async () => await Router.LaunchLink(session, a.Attachment.Link.Uri))();
                    break;
            }
        }
    }
}
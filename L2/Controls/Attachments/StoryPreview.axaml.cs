using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.VKAPILib.Objects;
using System;

namespace ELOR.Laney.Controls.Attachments {
    public partial class StoryPreview : UserControl {
        Story story;

        public StoryPreview(Story story) {
            InitializeComponent();
            this.story = story;

            AttachedToLogicalTree += StoryPreview_AttachedToLogicalTree;
        }

        private void StoryPreview_AttachedToLogicalTree(object sender, Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) {
            Setup();
        }

        private async void Setup() {
            VKSession session = VKSession.GetByDataContext(this);

            if (story.IsDeleted) {
                SetRestrictionInfo(Assets.i18n.Resources.stp_deleted);
                return;
            }

            if (story.OwnerId == session.Id) {
                AuthorAvatar.IsVisible = false;
                AuthorName.Text = Assets.i18n.Resources.stp_your_story;
            } else {
                var owner = CacheManager.GetNameAndAvatar(story.OwnerId);
                if (owner != null) {
                    AuthorName.Text = $"{owner.Item1} {owner.Item2}";
                    AuthorAvatar.SetImage(owner.Item3);
                }
            }

            if (story.IsExpired) {
                SetRestrictionInfo(Assets.i18n.Resources.stp_expired);
                return;
            }

            if (story.CanSee == 0) {
                SetRestrictionInfo(Assets.i18n.Resources.stp_private);
                return;
            }

            Uri preview = null;
            switch (story.Type) {
                case StoryType.Photo: preview = story.Photo.GetSizeAndUriForThumbnail(PreviewRoot.Width, PreviewRoot.Height).Uri; break;
                case StoryType.Video: preview = story.Video.FirstFrameForStory.Uri; break;
            }

            if (preview == null) return;

            await PreviewRoot.SetImageBackgroundAsync(preview, PreviewRoot.Width, PreviewRoot.Height);
        }

        private void SetRestrictionInfo(string info) {
            RestrictionText.Text = info;
            RestrictionText.IsVisible = true;
        }
    }
}
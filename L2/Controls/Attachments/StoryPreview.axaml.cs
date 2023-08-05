using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
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
                SetRestrictionInfo(Localizer.Instance["stp_deleted"]);
                return;
            }

            if (story.OwnerId == session.Id) {
                StoryAuthor.Text = Localizer.Instance["stp_your_story"];
            } else {
                StoryAuthor.Text = $"{Localizer.Instance["story"]} {VKAPIHelper.GetNameOrDefaultString(story.OwnerId)}";
            }

            if (story.IsExpired) {
                SetRestrictionInfo(Localizer.Instance["stp_expired"]);
                return;
            }

            if (story.CanSee == 0) {
                SetRestrictionInfo(Localizer.Instance["stp_private"]);
                return;
            }

            int width = Convert.ToInt32(PreviewRoot.Width);
            Uri preview = null;
            switch (story.Type) {
                case StoryType.Photo: preview = story.Photo.GetSizeAndUriForThumbnail(width).Uri; break;
                case StoryType.Video: preview = story.Video.FirstFrameForStory.Uri; break;
            }

            if (preview == null) return;

            await PreviewRoot.SetImageBackgroundAsync(preview, width);
        }

        private void SetRestrictionInfo(string info) {
            RestrictionText.Text = info;
            RestrictionText.IsVisible = true;
        }
    }
}
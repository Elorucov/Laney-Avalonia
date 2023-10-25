using Avalonia.Controls;
using Avalonia.Styling;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.VKAPILib.Objects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ELOR.Laney.Views.Media {
    public partial class Gallery : Window {
        public static void Show(List<IPreview> items, IPreview target) {
            new Gallery(items, target).Show();
        }

        List<IPreview> items;
        IPreview target;
        public Gallery() {
            InitializeComponent();
            RequestedThemeVariant = ThemeVariant.Dark;
        }

        private Gallery(List<IPreview> items, IPreview target) {
            InitializeComponent();
            RequestedThemeVariant = ThemeVariant.Dark;

            this.items = items;
            this.target = target;

            Activated += Gallery_Activated;
        }

        private void GalleryItems_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Title = $"{Localizer.Instance["wnd_gallery"]} — {GalleryItems.SelectedIndex + 1}/{GalleryItems.Items.Count}";

            AttachmentBase attachment = GalleryItems.SelectedItem as AttachmentBase;
            if (attachment != null) {
                var owner = CacheManager.GetNameAndAvatar(attachment.OwnerId);
                OwnerAvatar.IsVisible = owner != null;
                if (owner != null) {
                    string name = string.Join(" ", new string[] { owner.Item1, owner.Item2 });
                    OwnerName.Text = name;
                    OwnerAvatar.Background = attachment.OwnerId.GetGradient();
                    OwnerAvatar.Initials = name;
                    OwnerAvatar.SetImageAsync(owner.Item3);
                }

                if (attachment is Photo photo) {
                    Date.Text = photo.Date.ToHumanizedString(true);
                    Description.Text = photo.Text;
                    Description.IsVisible = !string.IsNullOrEmpty(photo.Text);
                } else if (attachment is Document doc) {
                    Date.Text = doc.DateTime.ToHumanizedString(true);
                    Description.Text = doc.Title;
                    Description.IsVisible = true;
                }
            }
        }

        private async void Gallery_Activated(object sender, System.EventArgs e) {
            Activated -= Gallery_Activated;
            if (items == null || items.Count == 0) {
                Close();
                return;
            }
            target = target != null ? target : items[0];

            GalleryItems.ItemsSource = items;
            await Task.Delay(100); // required.
            GalleryItems.SelectedItem = target;
        }

        private void ImageDataContextChanged(object sender, System.EventArgs e) {
            Image image = sender as Image;
            IPreview item = image.DataContext as IPreview;

            if (item is Photo photo) {
                image.SetUriSourceAsync(photo.MaximalSizedPhoto.Uri);
            } else if (item is Document doc && (doc.Type == DocumentType.Image || doc.Type == DocumentType.GIF)) {
                image.SetUriSourceAsync(doc.Uri);
            }
        }
    }
}
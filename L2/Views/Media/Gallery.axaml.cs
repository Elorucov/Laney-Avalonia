using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
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
            dbg.IsVisible = Settings.ShowDebugInfoInGallery;
            RequestedThemeVariant = ThemeVariant.Dark;
        }

        private Gallery(List<IPreview> items, IPreview target) {
            InitializeComponent();
            dbg.IsVisible = Settings.ShowDebugInfoInGallery;
            RequestedThemeVariant = ThemeVariant.Dark;

            this.items = items;
            this.target = target;

            Activated += Gallery_Activated;
        }

        private void GalleryItems_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Title = $"{Assets.i18n.Resources.wnd_gallery} — {GalleryItems.SelectedIndex + 1}/{GalleryItems.Items.Count}";

            AttachmentBase attachment = GalleryItems.SelectedItem as AttachmentBase;
            if (attachment != null) {
                dbgi.Text = attachment.ToString();
                var owner = CacheManager.GetNameAndAvatar(attachment.OwnerId);
                OwnerAvatar.IsVisible = owner != null;
                if (owner != null) {
                    string name = string.Join(" ", new string[] { owner.Item1, owner.Item2 });
                    OwnerName.Text = name;
                    OwnerAvatar.Background = attachment.OwnerId.GetGradient();
                    OwnerAvatar.Initials = name;
                    OwnerAvatar.SetImageAsync(owner.Item3, OwnerAvatar.Width, OwnerAvatar.Height);
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
            ZoomBorder zb = image.Parent as ZoomBorder;

            if (item is Photo photo) {
                var p = photo.MaximalSizedPhoto;
                image.SetUriSourceAsync(p.Uri);
            } else if (item is Document doc && (doc.Type == DocumentType.Image || doc.Type == DocumentType.GIF)) {
                image.SetUriSourceAsync(doc.Uri);
            }
        }

        private void ZoomBorder_PropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e) {
            ZoomBorder zb = sender as ZoomBorder;
            switch (e.Property.Name) {
                case nameof(ZoomBorder.OffsetX):
                case nameof(ZoomBorder.OffsetY):
                case nameof(ZoomBorder.ZoomX):
                case nameof(ZoomBorder.ZoomY):
                case nameof(ZoomBorder.Width):
                case nameof(ZoomBorder.Height):
                    dbgt.Text = $"W:     {zb.Width}\nH:     {zb.Height}\nOffsX: {zb.OffsetX}\nOffsY: {zb.OffsetY}\nZoomX: {zb.ZoomX}\nZoomY: {zb.ZoomY}";
                    break;
            }
        }

        private void ZoomBorder_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            ZoomBorder zb = sender as ZoomBorder;
            zb.Width = Width;
            zb.Height = Height;
            SizeChanged += (a, b) => {
                zb.Width = b.NewSize.Width;
                zb.Height = b.NewSize.Height;
            };
        }
    }
}
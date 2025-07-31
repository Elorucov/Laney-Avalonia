using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Styling;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.VKAPILib.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ELOR.Laney.Views.Media {
    public partial class Gallery : Window {
        public static void Show(List<IPreview> items, IPreview target) {
            new Gallery(items, target).Show();
        }

        List<IPreview> _items;
        IPreview _target;

        public Gallery() {
            InitializeComponent();
            dbg.IsVisible = Settings.ShowDebugInfoInGallery;
            RequestedThemeVariant = ThemeVariant.Dark;
        }

        private Gallery(List<IPreview> items, IPreview target) {
            InitializeComponent();
            dbg.IsVisible = Settings.ShowDebugInfoInGallery;
            RequestedThemeVariant = ThemeVariant.Dark;

            this._items = items;
            this._target = target;

            Activated += Gallery_Activated;
        }

        private void GalleryItems_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            UpdateInfo();
        }

        private void UpdateInfo() {
            Title = $"{Assets.i18n.Resources.wnd_gallery} — {GalleryItems.SelectedIndex + 1}/{GalleryItems.Items.Count}";

            AttachmentBase attachment = GalleryItems.SelectedItem as AttachmentBase;
            if (attachment != null) {
                dbgi.Text = attachment.ToString();
                var owner = CacheManager.GetNameAndAvatar(attachment.OwnerId);
                OwnerAvatar.IsVisible = owner.Item3 != null;
                string name = string.Join(" ", new string[] { owner.Item1, owner.Item2 });
                OwnerName.Text = name;
                OwnerAvatar.Background = attachment.OwnerId.GetGradient();
                OwnerAvatar.Initials = name;
                if (owner.Item3 != null) OwnerAvatar.SetImage(owner.Item3, OwnerAvatar.Width, OwnerAvatar.Height);

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

        private void Gallery_Activated(object sender, System.EventArgs e) {
            Activated -= Gallery_Activated;
            if (_items == null || _items.Count == 0) {
                Close();
                return;
            }
            _target = _target != null ? _target : _items[0];

            GalleryItems.SelectionChanged += GalleryItems_SelectionChanged;
            new System.Action(async () => {
                GalleryItems.ItemsSource = _items;
                await Task.Delay(50); // required, without delay scrolling to target in FlipView not properly working.
                GalleryItems.SelectedItem = _target;
            })();
            if (_items.FirstOrDefault() == _target) UpdateInfo(); // required, because GalleryItems_SelectionChanged doesn't called after first loading (because default index is 0).
        }

        private void ImageDataContextChanged(object sender, System.EventArgs e) {
            Image image = sender as Image;
            IPreview item = image.DataContext as IPreview;

            if (item is Photo photo) {
                var p = photo.MaximalSizedPhoto;
                image.SetUriSource(p.Uri);
            } else if (item is Document doc && (doc.Type == DocumentType.Image || doc.Type == DocumentType.GIF)) {
                image.SetUriSource(doc.Uri);
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
            Control child = zb.Child;

            zb.Width = Width;
            zb.Height = Height;

            SizeChanged += (a, b) => {
                zb.Width = b.NewSize.Width;
                zb.Height = b.NewSize.Height;
            };
        }
    }
}
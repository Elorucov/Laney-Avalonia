using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Diagnostics;
using System.IO;

namespace VKUI.Controls {
    public sealed class Avatar : TemplatedControl {
        public Avatar() { }

        #region Properties

        public static readonly StyledProperty<Bitmap> ImageProperty =
            AvaloniaProperty.Register<Avatar, Bitmap>(nameof(Image));

        public static readonly StyledProperty<string> InitialsProperty =
            AvaloniaProperty.Register<Avatar, string>(nameof(Initials));

        public Bitmap Image {
            get => GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public string Initials {
            get => GetValue(InitialsProperty);
            set => SetValue(InitialsProperty, value);
        }

        #endregion

        #region Template elements

        Ellipse ImageEllipse;

        #endregion

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            ImageEllipse = e.NameScope.Find<Ellipse>(nameof(ImageEllipse));
            SetImage();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);

            if (change.Property == ImageProperty) {
                if (change.OldValue != change.NewValue) SetImage();
            }

            if (change.Property == BoundsProperty) SetImage();
        }

        private void SetImage() {
            if (ImageEllipse == null) return;
            double size = Math.Min(Bounds.Width, Bounds.Height);
            ImageEllipse.Width = size;
            ImageEllipse.Height = size;

            if (Image == null) {
                ImageEllipse.Fill = null;
                return;
            }

            try {
                ImageEllipse.Fill = new ImageBrush(Image);
            } catch (Exception ex) {
                Debug.WriteLine($"Error while drawing in Avatar! 0x{ex.HResult.ToString("x8")}");
            }
        }
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace ToastNotifications.Avalonia {
    public class Toast : TemplatedControl {
        #region Properties

        public static readonly StyledProperty<string> HeaderProperty = AvaloniaProperty.Register<Toast, string>(nameof(Header), String.Empty);
        public static readonly StyledProperty<string> BodyProperty = AvaloniaProperty.Register<Toast, string>(nameof(Body), String.Empty);
        public static readonly StyledProperty<Bitmap> AvatarProperty = AvaloniaProperty.Register<Toast, Bitmap>(nameof(Avatar), null);
        public static readonly StyledProperty<Bitmap> ImageProperty = AvaloniaProperty.Register<Toast, Bitmap>(nameof(Image), null);

        public string Header {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public string Body {
            get => GetValue(BodyProperty);
            set => SetValue(BodyProperty, value);
        }

        public Bitmap Avatar {
            get => GetValue(AvatarProperty);
            set => SetValue(AvatarProperty, value);
        }
        public Bitmap Image {
            get => GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        #endregion

        public event EventHandler<RoutedEventArgs> CloseButtonClick;
        public event EventHandler<object> Click;

        #region Template

        Border Root;
        Ellipse CircleAvatar;
        Button CloseButton;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            Root = e.NameScope.Find<Border>(nameof(Root));
            CircleAvatar = e.NameScope.Find<Ellipse>(nameof(CircleAvatar));
            CloseButton = e.NameScope.Find<Button>(nameof(CloseButton));

            Root.PointerPressed += (a, b) => {
                Click?.Invoke(this, Tag);
            };

            CloseButton.Click += (a, b) => {
                CloseButtonClick?.Invoke(this, b);
            };

            CheckAvatar();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);
            if (change.Property == AvatarProperty) {
                CheckAvatar();
            }
        }

        #endregion

        private void CheckAvatar() {
            if (CircleAvatar == null) return;
            if (Avatar == null) {
                CircleAvatar.IsVisible = false;
                return;
            }
            CircleAvatar.Fill = new ImageBrush(Avatar);
            CircleAvatar.IsVisible = true;
        }
    }
}
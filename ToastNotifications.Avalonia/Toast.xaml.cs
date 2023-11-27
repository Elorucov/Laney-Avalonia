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
        public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<Toast, string>(nameof(Title), String.Empty);
        public static readonly StyledProperty<string> BodyProperty = AvaloniaProperty.Register<Toast, string>(nameof(Body), String.Empty);
        public static readonly StyledProperty<string> FootnoteProperty = AvaloniaProperty.Register<Toast, string>(nameof(Footnote), String.Empty);
        public static readonly StyledProperty<string> InputWatermarkProperty = AvaloniaProperty.Register<Toast, string>(nameof(InputWatermark), String.Empty);
        public static readonly StyledProperty<bool> IsWriteBarVisibleProperty = AvaloniaProperty.Register<Toast, bool>(nameof(IsWriteBarVisible), false);
        public static readonly StyledProperty<Bitmap> AppLogoProperty = AvaloniaProperty.Register<Toast, Bitmap>(nameof(AppLogo), null);
        public static readonly StyledProperty<Bitmap> AvatarProperty = AvaloniaProperty.Register<Toast, Bitmap>(nameof(Avatar), null);
        public static readonly StyledProperty<Bitmap> ImageProperty = AvaloniaProperty.Register<Toast, Bitmap>(nameof(Image), null);

        public string Header {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public string Title {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Body {
            get => GetValue(BodyProperty);
            set => SetValue(BodyProperty, value);
        }

        public string Footnote {
            get => GetValue(FootnoteProperty);
            set => SetValue(FootnoteProperty, value);
        }

        public string InputWatermark {
            get => GetValue(InputWatermarkProperty);
            set => SetValue(InputWatermarkProperty, value);
        }

        public bool IsWriteBarVisible {
            get => GetValue(IsWriteBarVisibleProperty);
            set => SetValue(IsWriteBarVisibleProperty, value);
        }

        public Bitmap AppLogo {
            get => GetValue(AppLogoProperty);
            set => SetValue(AppLogoProperty, value);
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
        public event EventHandler<string> SendButtonClick;
        public event EventHandler<object> Click;

        #region Template

        Border Root;
        Rectangle AppLogoArea;
        Ellipse CircleAvatar;
        Image ImageArea;
        Button CloseButton;
        Button SendButton;
        TextBox TextArea;
        Grid WriteBar;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            Root = e.NameScope.Find<Border>(nameof(Root));
            AppLogoArea = e.NameScope.Find<Rectangle>(nameof(AppLogoArea));
            CircleAvatar = e.NameScope.Find<Ellipse>(nameof(CircleAvatar));
            ImageArea = e.NameScope.Find<Image>(nameof(ImageArea));
            CloseButton = e.NameScope.Find<Button>(nameof(CloseButton));
            SendButton = e.NameScope.Find<Button>(nameof(SendButton));
            TextArea = e.NameScope.Find<TextBox>(nameof(TextArea));
            WriteBar = e.NameScope.Find<Grid>(nameof(WriteBar));

            Root.PointerPressed += (a, b) => {
                Click?.Invoke(this, Tag);
            };

            CloseButton.Click += (a, b) => {
                CloseButtonClick?.Invoke(this, b);
            };

            SendButton.Click += (a, b) => {
                if (!String.IsNullOrEmpty(TextArea.Text)) SendButtonClick?.Invoke(this, TextArea.Text);
            };

            CheckAppLogo();
            CheckAvatar();
            CheckImage();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);
            if (change.Property == AppLogoProperty) {
                CheckAppLogo();
            } else if (change.Property == AvatarProperty) {
                CheckAvatar();
            } else if (change.Property == ImageProperty) {
                CheckImage();
            }
        }

        private void CheckAppLogo() {
            if (AppLogoArea == null) return;
            if (AppLogo == null) {
                AppLogoArea.IsVisible = false;
                return;
            }
            AppLogoArea.Fill = new ImageBrush(AppLogo);
            AppLogoArea.IsVisible = true;
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

        private void CheckImage() {
            if (ImageArea == null) return;
            if (Image == null) {
                ImageArea.IsVisible = false;
                return;
            }
            ImageArea.Source = Image;
            ImageArea.IsVisible = true;
        }
    }
}
using Avalonia;
using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;

namespace ELOR.Laney.Controls.Attachments {
    public partial class CarouselElementUI : UserControl {

        public static readonly StyledProperty<CarouselElement> ElementProperty =
            AvaloniaProperty.Register<CarouselElementUI, CarouselElement>(nameof(Element));

        public CarouselElement Element {
            get => GetValue(ElementProperty);
            set => SetValue(ElementProperty, value);
        }

        public CarouselElementUI() {
            InitializeComponent();
            PropertyChanged += CarouselElementUI_PropertyChanged;
            Unloaded += CarouselElementUI_Unloaded;
        }

        private void RootButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            ExceptionHelper.ShowNotImplementedDialogAsync(VKSession.GetByDataContext(this).ModalWindow);
        }

        private void CarouselElementUI_Unloaded(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            PropertyChanged -= CarouselElementUI_PropertyChanged;
            Unloaded -= CarouselElementUI_Unloaded;
        }

        private void CarouselElementUI_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e) {
            if (e.Property == ElementProperty && Element != null) {
                var photo = Element.Photo.GetSizeAndUriForThumbnail(CardImage.Width, CardImage.Height).Uri;
                new System.Action(async () => await CardImage.SetImageBackgroundAsync(photo, CardImage.Width, CardImage.Height))();

                Buttons.Children.Clear();
                VKAPIHelper.GenerateButtons(Buttons, Element.Buttons);
            }
        }
    }
}
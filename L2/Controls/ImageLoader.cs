using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using ELOR.Laney.Extensions;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace ELOR.Laney.Controls {
    public static class ImageLoader {
        static ImageLoader() {
            SourceProperty.Changed
                .Where(args => args.IsEffectiveValueChange)
                .Subscribe(args => OnSourceChanged((Image)args.Sender, args.NewValue.Value));

            BackgroundSourceProperty.Changed
                .Where(args => args.IsEffectiveValueChange)
                .Subscribe(args => OnBackgroundSourceChanged((Border)args.Sender, args.NewValue.Value));

            FillSourceProperty.Changed
                .Where(args => args.IsEffectiveValueChange)
                .Subscribe(args => OnFillSourceChanged((Shape)args.Sender, args.NewValue.Value));
        }

        private static async void OnSourceChanged(Image sender, Uri? uri) {
            // SetIsLoading(sender, true);

            var bitmap = uri == null
                ? null
                : await LNetExtensions.TryGetCachedBitmapAsync(uri);
            if (GetSource(sender) != uri) return;
            sender.Source = bitmap;

            // SetIsLoading(sender, false);
        }

        private static void OnBackgroundSourceChanged(Border sender, Uri uri) {
            sender.Background = App.GetResource<SolidColorBrush>("VKBackgroundHoverBrush");
            sender.SetImageBackgroundAsync(uri);
        }

        private static void OnFillSourceChanged(Shape sender, Uri uri) {
            sender.Fill = App.GetResource<SolidColorBrush>("VKBackgroundHoverBrush");
            sender.SetImageFillAsync(uri);
        }

        public static readonly AttachedProperty<Uri?> SourceProperty = AvaloniaProperty.RegisterAttached<Image, Uri?>("Source", typeof(ImageLoader));

        public static Uri? GetSource(Image element) {
            return element.GetValue(SourceProperty);
        }

        public static void SetSource(Image element, Uri? value) {
            element.SetValue(SourceProperty, value);
        }

        public static readonly AttachedProperty<Uri?> BackgroundSourceProperty = AvaloniaProperty.RegisterAttached<Border, Uri?>("BackgroundSource", typeof(ImageLoader));

        public static Uri? GetBackgroundSource(Border element) {
            return element.GetValue(BackgroundSourceProperty);
        }

        public static void SetBackgroundSource(Border element, Uri? value) {
            element.SetValue(BackgroundSourceProperty, value);
        }

        public static readonly AttachedProperty<Uri?> FillSourceProperty = AvaloniaProperty.RegisterAttached<Shape, Uri?>("FillSource", typeof(ImageLoader));

        public static Uri? GetFillSource(Shape element) {
            return element.GetValue(FillSourceProperty);
        }

        public static void SetFillSource(Shape element, Uri? value) {
            element.SetValue(FillSourceProperty, value);
        }

        //public static readonly AttachedProperty<bool> IsLoadingProperty = AvaloniaProperty.RegisterAttached<Image, bool>("IsLoading", typeof(ImageLoader));

        //public static bool GetIsLoading(Image element) {
        //    return element.GetValue(IsLoadingProperty);
        //}

        //private static void SetIsLoading(Image element, bool value) {
        //    element.SetValue(IsLoadingProperty, value);
        //}
    }
}
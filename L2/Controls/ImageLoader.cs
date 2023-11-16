using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using Serilog;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using VKUI.Controls;

namespace ELOR.Laney.Controls {
    public static class ImageLoader {
        static ImageLoader() {
            SourceProperty.Changed
                .Subscribe(args => OnSourceChanged((Image)args.Sender, args.NewValue.Value));

            BackgroundSourceProperty.Changed
                .Subscribe(args => OnBackgroundSourceChanged((Border)args.Sender, args.NewValue.Value));

            FillSourceProperty.Changed
                .Subscribe(args => OnFillSourceChanged((Shape)args.Sender, args.NewValue.Value));

            ImageProperty.Changed
                .Subscribe(args => OnImageChanged((Avatar)args.Sender, args.NewValue.Value));
        }

        private static async void OnSourceChanged(Image sender, Uri? uri) {
            // SetIsLoading(sender, true);

            double dw = sender.Width != 0 ? sender.Width : sender.DesiredSize.Width;
            double dh = sender.Height != 0 ? sender.Height : sender.DesiredSize.Height;

            try {
                var bitmap = uri == null
                ? null
                : await BitmapManager.GetBitmapAsync(uri, dw, dh);
                if (GetSource(sender) != uri) return;
                sender.Source = bitmap;
                sender.Unloaded += Sender_Unloaded;
            } catch (Exception ex) {
                Log.Error(ex, "Cannot set bitmap to Image!");
                sender.Source = null;
            }

            // SetIsLoading(sender, false);
        }

        private static async void OnBackgroundSourceChanged(Border sender, Uri uri) {
            double dw = sender.Width != 0 ? sender.Width : sender.DesiredSize.Width;
            double dh = sender.Height != 0 ? sender.Height : sender.DesiredSize.Height;

            sender.Background = App.GetResource<SolidColorBrush>("VKBackgroundHoverBrush");
            await sender.SetImageBackgroundAsync(uri, dw, dh);
            sender.Unloaded += Sender_Unloaded;
        }

        private static void OnFillSourceChanged(Shape sender, Uri uri) {
            double dw = sender.Width != 0 ? sender.Width : sender.DesiredSize.Width;
            double dh = sender.Height != 0 ? sender.Height : sender.DesiredSize.Height;

            sender.Fill = App.GetResource<SolidColorBrush>("VKBackgroundHoverBrush");
            sender.SetImageFillAsync(uri, dw, dh);
            sender.Unloaded += Sender_Unloaded;
        }

        private static void OnImageChanged(Avatar sender, Uri uri) {
            double dw = sender.Width != 0 ? sender.Width : sender.DesiredSize.Width;
            double dh = sender.Height != 0 ? sender.Height : sender.DesiredSize.Height;

            sender.SetImageAsync(uri, dw, dh);
            sender.Unloaded += Sender_Unloaded;
        }

        private static void Sender_Unloaded(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Debug.WriteLine($"Unloading image UI: {sender.GetType()}");
            if (sender is Image img) {
                img.Unloaded -= Sender_Unloaded;
                img.Source = null;
            } else if (sender is Border b) {
                b.Unloaded -= Sender_Unloaded;
                b.Background = null;
            } else if (sender is Shape s) {
                s.Unloaded -= Sender_Unloaded;
                s.Fill = null;
            } else if (sender is Avatar ava) {
                ava.Unloaded -= Sender_Unloaded;
                ava.Image = null;
            }
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

        public static readonly AttachedProperty<Uri?> ImageProperty = AvaloniaProperty.RegisterAttached<Avatar, Uri?>("Image", typeof(ImageLoader));

        public static Uri? GetImage(Avatar element) {
            return element.GetValue(ImageProperty);
        }

        public static void SetImage(Avatar element, Uri? value) {
            element.SetValue(ImageProperty, value);
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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using Avalonia.VisualTree;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VKUI.Controls;

namespace ELOR.Laney.Controls {
    public static class ImageLoader {
        static ImageLoader() {
            SourceProperty.Changed
                .Subscribe(args => OnSourceChanged((Image)args.Sender, args.NewValue.Value));

            SvgSourceProperty.Changed
                .Subscribe(args => OnSvgSourceChanged((Image)args.Sender, args.NewValue.Value));

            BackgroundSourceProperty.Changed
                .Subscribe(args => OnBackgroundSourceChanged((Control)args.Sender, args.NewValue.Value));

            FillSourceProperty.Changed
                .Subscribe(args => OnFillSourceChanged((Shape)args.Sender, args.NewValue.Value));

            ImageProperty.Changed
                .Subscribe(args => OnImageChanged((Avatar)args.Sender, args.NewValue.Value));
        }

        #region Weak load

        private static void OnAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            Control control = sender as Control;
            control.AttachedToVisualTree -= OnAttachedToVisualTree;

            Uri? uri = control.Resources["uri"] as Uri;
            new Action(async () => await RegisterLoadImageAfterAppearingOnScreen(control, uri))();
        }

        static Dictionary<ScrollViewer, Dictionary<Control, Uri>> registeredControlsInScroll = new Dictionary<ScrollViewer, Dictionary<Control, Uri>>();

        private static async Task RegisterLoadImageAfterAppearingOnScreen(Control sender, Uri? uri) {
            await Task.Delay(8);
            var parentScroll = sender.FindAncestorOfType<ListBox>();
            if (parentScroll != null) {
                var t = sender.TransformToVisual(parentScroll);
                if (t != null) {
                    var y = t.Value.M32;
                    Debug.WriteLine($"+++ Image control pos: {y}");
                    double pos = y + sender.DesiredSize.Height;
                    if (pos <= 0) {  // Control is out of visible area
                        ScrollViewer sv = parentScroll.Scroll as ScrollViewer;
                        Debug.WriteLine($"+++ Registering image control...");
                        if (registeredControlsInScroll.ContainsKey(sv)) {
                            if (!registeredControlsInScroll[sv].ContainsKey(sender)) {
                                registeredControlsInScroll[sv].Add(sender, uri);
                            }
                        } else {
                            registeredControlsInScroll.Add(sv, new Dictionary<Control, Uri> { { sender, uri } });
                            sv.ScrollChanged += CheckIsControlHasAppearOnScreen;
                        }

                        return;
                    }
                }
            }

            SetSourceInternal(sender, uri);
        }

        private static void CheckIsControlHasAppearOnScreen(object sender, ScrollChangedEventArgs e) {
            ScrollViewer sv = sender as ScrollViewer;
            if (registeredControlsInScroll.ContainsKey(sv)) {
                var controls = registeredControlsInScroll[sv];

                List<Control> toRemove = new List<Control>();

                foreach (var c in controls) {
                    Control control = c.Key;
                    var t = control.TransformToVisual(sv);
                    if (t != null) {
                        var y = t.Value.M32;
                        double pos = y + control.DesiredSize.Height;
                        if (pos > 0) {  // Control is appear on screen
                            Debug.WriteLine("+++ Image control has appeared on screen!");
                            toRemove.Add(control);
                            SetSourceInternal(control, c.Value);
                        }
                    }
                }
                foreach (var control in CollectionsMarshal.AsSpan(toRemove)) {
                    controls.Remove(control);
                }
                registeredControlsInScroll[sv] = controls;
            }
        }

        private static void SetSourceInternal(Control sender, Uri uri) {
            if (sender is Image image) {
                OnSourceChangedInternal(image, uri);
            } else if (sender is TemplatedControl tc && sender is not Avatar) {
                OnBackgroundSourceChangedInternal(tc, uri);
            } else if (sender is Border border) {
                OnBackgroundSourceChangedInternal(border, uri);
            } else if (sender is Shape shape) {
                OnFillSourceChangedInternal(shape, uri);
            } else if (sender is Avatar avatar) {
                OnImageChangedInternal(avatar, uri);
            }
        }

        #endregion

        private static void OnSourceChanged(Image sender, Uri? uri) {
            if (sender.Parent == null) {
                sender.Resources.Add("uri", uri);
                sender.AttachedToVisualTree += OnAttachedToVisualTree;
            } else {
                new Action(async () => await RegisterLoadImageAfterAppearingOnScreen(sender, uri))();
            }
        }

        private static void OnSourceChangedInternal(Image sender, Uri? uri) {
            double dw = sender.Width != 0 ? sender.Width : sender.DesiredSize.Width;
            double dh = sender.Height != 0 ? sender.Height : sender.DesiredSize.Height;

            try {
                if (uri != null) new Action(async () => {
                    if (GetSource(sender) != uri) return;
                    Bitmap bitmap = null;
                    bitmap = await BitmapManager.GetBitmapAsync(uri, dw, dh);
                    sender.Source = bitmap;
                })();
                // sender.Unloaded += Sender_Unloaded;
            } catch (Exception ex) {
                Log.Error(ex, "Cannot set bitmap to Image!");
                sender.Source = null;
            }
        }

        private static void OnSvgSourceChanged(Image sender, Uri uri) {
            if (uri == null) {
                sender.Source = null;
                return;
            }
            double dw = sender.Width != 0 ? sender.Width : sender.DesiredSize.Width;
            double dh = sender.Height != 0 ? sender.Height : sender.DesiredSize.Height;

            new Action(async () => {
                SvgImage image = await CacheManager.GetStaticReactionImageAsync(uri);
                if (image == null) return;
                sender.Source = image;
            })();
            // sender.Unloaded += Sender_Unloaded;
        }

        private static void OnBackgroundSourceChanged(Control sender, Uri uri) {
            if (sender.Parent == null) {
                sender.Resources.Add("uri", uri);
                sender.AttachedToVisualTree += OnAttachedToVisualTree;
            } else {
                new Action(async () => await RegisterLoadImageAfterAppearingOnScreen(sender, uri))();
            }
        }

        private static void OnBackgroundSourceChangedInternal(TemplatedControl sender, Uri uri) {
            double dw = sender.Width != 0 ? sender.Width : sender.DesiredSize.Width;
            double dh = sender.Height != 0 ? sender.Height : sender.DesiredSize.Height;

            sender.Background = App.GetResource<SolidColorBrush>("VKBackgroundHoverBrush");
            new Action(async () => await sender.SetImageBackgroundAsync(uri, dw, dh))();
            // sender.Unloaded += Sender_Unloaded;
        }

        private static void OnBackgroundSourceChangedInternal(Border sender, Uri uri) {
            double dw = sender.Width != 0 ? sender.Width : sender.DesiredSize.Width;
            double dh = sender.Height != 0 ? sender.Height : sender.DesiredSize.Height;

            sender.Background = App.GetResource<SolidColorBrush>("VKBackgroundHoverBrush");
            new Action(async () => await sender.SetImageBackgroundAsync(uri, dw, dh))();
            // sender.Unloaded += Sender_Unloaded;
        }

        private static void OnFillSourceChanged(Shape sender, Uri uri) {
            if (sender.Parent == null) {
                sender.Resources.Add("uri", uri);
                sender.AttachedToVisualTree += OnAttachedToVisualTree;
            } else {
                new Action(async () => await RegisterLoadImageAfterAppearingOnScreen(sender, uri))();
            }
        }

        private static void OnFillSourceChangedInternal(Shape sender, Uri uri) {
            double dw = sender.Width != 0 ? sender.Width : sender.DesiredSize.Width;
            double dh = sender.Height != 0 ? sender.Height : sender.DesiredSize.Height;

            sender.Fill = App.GetResource<SolidColorBrush>("VKBackgroundHoverBrush");
            sender.SetImageFill(uri, dw, dh);
            // sender.Unloaded += Sender_Unloaded;
        }

        private static void OnImageChanged(Avatar sender, Uri uri) {
            if (sender.Parent == null) {
                sender.Resources.Add("uri", uri);
                sender.AttachedToVisualTree += OnAttachedToVisualTree;
            } else {
                new Action(async () => await RegisterLoadImageAfterAppearingOnScreen(sender, uri))();
            }
        }

        private static void OnImageChangedInternal(Avatar sender, Uri uri) {
            double dw = sender.Width != 0 ? sender.Width : sender.DesiredSize.Width;
            double dh = sender.Height != 0 ? sender.Height : sender.DesiredSize.Height;

            sender.SetImage(uri, dw, dh);
            // sender.Unloaded += Sender_Unloaded;
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

        public static readonly AttachedProperty<Uri?> SvgSourceProperty = AvaloniaProperty.RegisterAttached<Image, Uri?>("Source", typeof(ImageLoader));

        public static Uri? GetSvgSource(Image element) {
            return element.GetValue(SvgSourceProperty);
        }

        public static void SetSvgSource(Image element, Uri? value) {
            element.SetValue(SvgSourceProperty, value);
        }

        public static readonly AttachedProperty<Uri?> BackgroundSourceProperty = AvaloniaProperty.RegisterAttached<Control, Uri?>("BackgroundSource", typeof(ImageLoader));

        public static Uri? GetBackgroundSource(TemplatedControl element) {
            return element.GetValue(BackgroundSourceProperty);
        }

        public static Uri? GetBackgroundSource(Border element) {
            return element.GetValue(BackgroundSourceProperty);
        }

        public static void SetBackgroundSource(TemplatedControl element, Uri? value) {
            element.SetValue(BackgroundSourceProperty, value);
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
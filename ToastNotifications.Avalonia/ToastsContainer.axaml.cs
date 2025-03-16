using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using System.Runtime.InteropServices;

namespace ToastNotifications.Avalonia {
    internal partial class ToastsContainer : Window {
        internal ToastsContainer() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            SetPosition();
        }

        internal ToastsContainer(Action<string> log = null) {
            InitializeComponent();
            Log = log;
#if DEBUG
            this.AttachDevTools();
#endif
            SetPosition();
        }

        Action<string> Log;

        bool topAligned = false;
        bool leftAligned = false;
        Screen lastScreen = null;

        private void SetPosition(bool cycle = false) {
            var screen = Screens.ScreenFromWindow(this);
            if (screen == null) {
                if (lastScreen == null) {
                    Log?.Invoke("SetPosition: Cannot get screen!");
                    return;
                } else {
                    // В macOS после того, как мы получили окно в первый раз, в дальнейшем возвращает null.
                    Log?.Invoke("SetPosition: Cannot get screen now, but last time we got this...");
                    screen = lastScreen;
                }
            } else {
                lastScreen = screen;
            }

            var working = screen.WorkingArea;
            double scale = screen.Scaling;
            bool needBigMargin = false;

            double sw = (double)screen.Bounds.Width / scale;
            double sh = (double)screen.Bounds.Height / scale;
            double ww = working.Width / scale;
            double wh = working.Height / scale;
            double wx = working.X / scale;
            double wy = working.Y / scale;

            if (wh > sh) {
                needBigMargin = true;
                Log?.Invoke("SetPosition: Working area's height is GREATER than screen height! WTF?!");
            }

            NotificationItems.Measure(new Size(MaxWidth, wh));
            double height = NotificationItems.DesiredSize.Height;

            topAligned = sh > wh && wy != 0;
            leftAligned = sw > ww && wx != 0;
            int posx = Convert.ToInt32((leftAligned ? wx : ww - MaxWidth) * scale);
            int posy = Convert.ToInt32((topAligned ? wy : wh - height) * scale);
            if (topAligned) posy = posy + 9;

            Position = new PixelPoint(posx, needBigMargin ? posy + 48 : posy);
            Height = needBigMargin ? height - 96 : height;

            IsVisible = height > 0;
            Log?.Invoke($"SetPosition: topAligned={topAligned}; leftAligned={leftAligned}; sw={sw}; sh={sh}; wx={wx}; wy={wy}; ww={ww}; wh={wh}; tx={posx}; ty={posy}; th={height}; isVisible={IsVisible}");
        }

        internal void AddToastToContainer(ToastNotification notification, Bitmap appLogo) {
            if (NotificationItems.Children.Count >= 4) NotificationItems.Children.RemoveAt(0);
            Toast toast = new Toast() {
                Header = notification.Header,
                Title = notification.Title,
                Body = notification.Message,
                Footnote = notification.Footnote,
                AppLogo = appLogo,
                Avatar = notification.Avatar,
                Image = notification.Image,
                IsWriteBarVisible = notification.OnSendClick != null,
                Margin = new Thickness(12, 3, 12, 9)
            };

            // Linux DE moment... (maybe also macOS?)
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                toast.Loaded += async (a, b) => {
                    await Task.Delay(20);
                    SetPosition();
                };
            }

            IsVisible = true;
            NotificationItems.Children.Add(toast);
            Log?.Invoke($"AddToastToContainer: toast {toast.GetHashCode()} added in window!");

            SetPosition();

            DispatcherTimer timer = new DispatcherTimer {
                Interval = notification.Expiration,
            };
            timer.Tick += (a, b) => {
                timer.Stop();
                RemoveToast(toast);
            };
            timer.Start();

            toast.GotFocus += (a, b) => {
                timer.Stop();
            };
            toast.LostFocus += (a, b) => {
                timer.Start();
            };

            toast.CloseButtonClick += (a, b) => {
                timer.Stop();
                notification.OnClose?.Invoke();
                RemoveToast(toast);
            };
            toast.SendButtonClick += (a, b) => {
                timer.Stop();
                notification.OnSendClick?.Invoke(b);
                RemoveToast(toast);
            };
            toast.Click += (a, b) => {
                timer.Stop();
                notification.OnClick?.Invoke();
                RemoveToast(toast);
            };
        }

        private void RemoveToast(Toast toast) {
            NotificationItems.Children.Remove(toast);
            Log?.Invoke($"AddToastToContainer: toast {toast.GetHashCode()} removed!");
            SetPosition();
        }
    }
}
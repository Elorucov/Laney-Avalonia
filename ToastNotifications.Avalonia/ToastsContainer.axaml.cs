using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace ToastNotifications.Avalonia {
    internal partial class ToastsContainer : Window {
        internal ToastsContainer() {
            InitializeComponent();
            #if DEBUG
            this.AttachDevTools();
            #endif
            SetPosition();
        }

        bool topAligned = false;
        bool leftAligned = false;

        private void SetPosition() {
            var screen = Screens.ScreenFromWindow(this);
            if (screen == null) {
                Debug.WriteLine("ToastContainer: Cannot get screen!");
                return;
            }

            var working = screen.WorkingArea;
            Width = 384;
            NotificationItems.Measure(new Size(Width, working.Height));
            double height = NotificationItems.DesiredSize.Height;

            topAligned = screen.Bounds.Height > working.Height && working.Y != 0;
            leftAligned = screen.Bounds.Width > working.Width && working.X != 0;
            int posx = leftAligned ? working.X : working.Width - Convert.ToInt32(Width);
            int posy = topAligned ? working.Y : working.Height - Convert.ToInt32(height);
            if (topAligned) posy = posy + 9;

            Position = new PixelPoint(posx, posy);
            Height = height;

            // In Windows, minimum window height forced to 39, so we hide window.
            // On other platforms, hiding window cause some strange bugs.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                IsVisible = height > 0;
            }
        }

        internal void AddToastToContainer(ToastNotification notification) {
            if (NotificationItems.Children.Count >= 4) NotificationItems.Children.RemoveAt(0);
            Toast toast = new Toast() {
                Header = notification.Header,
                Title = notification.Title,
                Body = notification.Message,
                Footnote = notification.Footnote,
                Avatar = notification.Avatar,
                Image = notification.Image,
                IsWriteBarVisible = notification.OnSendClick != null,
                Margin = new Thickness(12, 3, 12, 9)
            };
            
            IsVisible = true;
            NotificationItems.Children.Add(toast);
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
            SetPosition();
        }
    }
}
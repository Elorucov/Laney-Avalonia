using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace ToastNotifications.Avalonia {
    internal partial class ToastsContainer : Window {
        internal ToastsContainer() {
            InitializeComponent();
            SetPosition();
            SizeChanged += ToastContaner_SizeChanged;
        }

        private void ToastContaner_SizeChanged(object sender, SizeChangedEventArgs e) {
            SetPosition();
            if (e.NewSize.Height <= NotificationItems.Margin.Top + NotificationItems.Margin.Bottom) IsVisible = false;
        }

        private void SetPosition() {
            var screen = Screens.ScreenFromWindow(this);
            var working = screen.WorkingArea;
            MaxHeight = working.Height;

            bool topAligned = screen.Bounds.Height > working.Height && working.Y != 0;
            bool leftAligned = screen.Bounds.Width > working.Width && working.X != 0;
            int posx = leftAligned ? working.X : working.Width - Convert.ToInt32(Width);
            int posy = topAligned ? working.Y : working.Height - Convert.ToInt32(DesiredSize.Height);

            Position = new PixelPoint(posx, posy);
        }

        internal void AddToastToContainer(ToastNotification notification) {
            if (NotificationItems.Children.Count >= 4) NotificationItems.Children.RemoveAt(0);
            Toast toast = new Toast() { 
                Header = notification.Title,
                Body = notification.Message,
                Avatar = notification.Avatar,
                Image = notification.Image,
                Margin = new Thickness(12, 6, 12, 6)
            };
            

            NotificationItems.Children.Add(toast);
            IsVisible = true;

            DispatcherTimer timer = new DispatcherTimer { 
                Interval = notification.Expiration,
            };
            timer.Tick += (a, b) => {
                timer.Stop();
                NotificationItems.Children.Remove(toast);
            };
            timer.Start();

            toast.CloseButtonClick += (a, b) => {
                timer.Stop();
                notification.OnClose?.Invoke();
                NotificationItems.Children.Remove(toast);
            };
            toast.Click += (a, b) => {
                timer.Stop();
                notification.OnClick?.Invoke();
                NotificationItems.Children.Remove(toast);
            };
        }
    }
}
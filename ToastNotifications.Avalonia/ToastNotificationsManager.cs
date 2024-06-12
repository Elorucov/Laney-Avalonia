using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;

namespace ToastNotifications.Avalonia {
    public class ToastNotificationsManager : INotificationManager {
        internal static ToastsContainer Container { get; private set; }
        public static double ExpirationMilliseconds { get; set; } = 7000;
        internal Bitmap AppLogo { get; private set; }
        internal Action<string> Log { get; private set; }

        public ToastNotificationsManager(Bitmap appLogo = null, Action<string> log = null) {
            AppLogo = appLogo;
            Log = log;
        }

        public void Show(INotification notification) {
            if (ExpirationMilliseconds < 1000 || ExpirationMilliseconds > 15000)
                throw new ArgumentException($"{ExpirationMilliseconds} should be between 1000 and 15000!");

            if (Container == null) {
                Container = new ToastsContainer(Log);
            }

            if (notification is ToastNotification tn) {
                if (tn.Expiration.TotalMilliseconds == 0) tn.Expiration = TimeSpan.FromMilliseconds(ExpirationMilliseconds);
                Container.AddToastToContainer(tn, AppLogo);
            } else {
                throw new ArgumentException($"ToastNotification required!");
            }
        }

        public void Close(INotification notification) {
            
        }

        public void CloseAll() {
            
        }
    }
}
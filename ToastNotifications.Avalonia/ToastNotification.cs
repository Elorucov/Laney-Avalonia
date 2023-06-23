using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;

namespace ToastNotifications.Avalonia {
    public class ToastNotification : INotification {
        public string Title { get; private set; }
        public string Message { get; private set; }
        public string Caption { get; private set; }
        public Bitmap Avatar { get; private set; }
        public Bitmap Image { get; private set; }
        public NotificationType Type => NotificationType.Information;
        public TimeSpan Expiration { get; internal set; }
        public Action OnClick { get; set; }
        public Action OnClose { get; set; }
        public object AssociatedObject { get; private set; } 

        public ToastNotification(object assObj, string header, string text, string caption = null, Bitmap avatar = null, Bitmap image = null) {
            AssociatedObject = assObj;
            
            Title = header;
            Message = text;
            Caption = caption;
            Avatar = avatar;
            Image = image;
        }
    }
}
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;

namespace ToastNotifications.Avalonia {
    public class ToastNotification : INotification {
        public string Title { get; private set; }
        public string Header { get; private set; }
        public string Message { get; private set; }
        public string Footnote { get; private set; }
        public Bitmap Avatar { get; private set; }
        public Bitmap Image { get; private set; }
        public NotificationType Type => NotificationType.Information;
        public TimeSpan Expiration { get; internal set; }
        public Action OnClick { get; set; }
        public Action OnClose { get; set; }
        public Action<string> OnSendClick { get; set; }
        public object AssociatedObject { get; private set; } 

        public ToastNotification(object assObj, string header, string title, string text, string footnote = null, Bitmap avatar = null, Bitmap image = null) {
            AssociatedObject = assObj;

            Header = header;
            Title = title;
            Message = text;
            Footnote = footnote;
            Avatar = avatar;
            Image = image;
        }
    }
}
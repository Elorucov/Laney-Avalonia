using ELOR.Laney.Core;

namespace ELOR.Laney.ViewModels.SettingsCategories {
    public class NotificationsViewModel : CommonViewModel {
        public bool Private { get { return Settings.NotificationsPrivate; } set { Settings.NotificationsPrivate = value; OnPropertyChanged(); } }
        public bool PrivateSound { get { return Settings.NotificationsPrivateSound; } set { Settings.NotificationsPrivateSound = value; OnPropertyChanged(); } }
        public bool GroupChat { get { return Settings.NotificationsGroupChat; } set { Settings.NotificationsGroupChat = value; OnPropertyChanged(); } }
        public bool GroupChatSound { get { return Settings.NotificationsGroupChatSound; } set { Settings.NotificationsGroupChatSound = value; OnPropertyChanged(); } }

        public NotificationsViewModel() { }
    }
}
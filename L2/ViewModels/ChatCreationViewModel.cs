using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Media.Imaging;
using ELOR.Laney.Core;
using ELOR.Laney.DataModels;
using ELOR.Laney.Helpers;
using ELOR.Laney.Views.Modals;
using ELOR.VKAPILib.Methods;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ELOR.Laney.ViewModels {
    public class ChatCreationViewModel : ViewModelBase {
        private string _chatName;
        private Bitmap _chatPhotoPreview;
        private ObservableCollection<GroupWithSelection<string, User>> _groupedFriends;
        private ObservableCollection<User> _selectedFriends = new ObservableCollection<User>();
        private string _searchQuery;
        private bool _isLoading;
        private PlaceholderViewModel _placeholder;
        private bool _canCreateChat;

        private RelayCommand _chatPhotoSetCommand;
        private RelayCommand _customizeChatSettingsCommand;
        private RelayCommand _createCommand;

        public string ChatName { get { return _chatName; } set { _chatName = value; OnPropertyChanged(); } }
        public Bitmap ChatPhotoPreview { get { return _chatPhotoPreview; } set { _chatPhotoPreview = value; OnPropertyChanged(); } }
        public ObservableCollection<GroupWithSelection<string, User>> GroupedFriends { get { return _groupedFriends; } private set { _groupedFriends = value; OnPropertyChanged(); } }
        public ObservableCollection<User> SelectedFriends { get { return _selectedFriends; } set { _selectedFriends = value; OnPropertyChanged(); } }
        public string SearchQuery { get { return _searchQuery; } set { _searchQuery = value; OnPropertyChanged(); } }
        public bool IsLoading { get { return _isLoading; } private set { _isLoading = value; OnPropertyChanged(); } }
        public PlaceholderViewModel Placeholder { get { return _placeholder; } private set { _placeholder = value; OnPropertyChanged(); } }
        public bool CanCreateChat { get { return !String.IsNullOrEmpty(ChatName) || SelectedFriends.Count > 0; } }

        public RelayCommand ChatPhotoSetCommand { get { return _chatPhotoSetCommand; } private set { _chatPhotoSetCommand = value; OnPropertyChanged(); } }
        public RelayCommand CustomizeChatSettingsCommand { get { return _customizeChatSettingsCommand; } private set { _customizeChatSettingsCommand = value; OnPropertyChanged("CustomizeChatSetingsCommand"); } }
        public RelayCommand CreateCommand { get { return _createCommand; } private set { _createCommand = value; OnPropertyChanged(); } }

        // private StorageFile ChatPhoto;
        public ObservableCollection<User> Friends = new ObservableCollection<User>();
        private ChatPermissions Permissions;
        private VKSession session;
        private System.Action GoToBackAction;

        public ChatCreationViewModel(VKSession session, System.Action goToBackAction) {
            GoToBackAction = goToBackAction;
            this.session = session;
            ChatPhotoSetCommand = new RelayCommand((o) => ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow));
            CustomizeChatSettingsCommand = new RelayCommand((o) => ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow));
            CreateCommand = new RelayCommand((o) => CreateChat());

            PropertyChanged += (a, b) => { 
                switch (b.PropertyName) {
                    case nameof(ChatName):
                        OnPropertyChanged(nameof(CanCreateChat));
                        break;
                }
            };

            LoadFriends();
        }

        private async void LoadFriends() {
            if (IsLoading) return;
            IsLoading = true;
            Placeholder = null;
            Friends.Clear();
            GroupedFriends = null;

            try {
                var response = await session.API.Friends.GetAsync(VKAPIHelper.UserFields, order: FriendsOrder.Name);
                // CacheManager.Add(response.Items);
                Friends = new ObservableCollection<User>(response.Items);
                GroupFriends();
            } catch (Exception ex) {
                Placeholder = PlaceholderViewModel.GetForException(ex, (o) => LoadFriends());
            }

            IsLoading = false;
        }

        private void GroupFriends() {
            // TODO: сделать отдельный класс или метод для группировки по алфавиту
            GroupedFriends = new ObservableCollection<GroupWithSelection<string, User>>(Friends.GroupBy(f => 
                !String.IsNullOrEmpty(f.FirstName) ? f.FirstName[0].ToString().ToUpper() : "~")
                .Select(g => new GroupWithSelection<string, User>(g, FriendsSelectionChanged)));
        }

        private void FriendsSelectionChanged(object sender, SelectionModelSelectionChangedEventArgs<User> e) {
            foreach (User friend in e.SelectedItems) {
                SelectedFriends.Add(friend);
            }
            foreach (User friend in e.DeselectedItems) {
                SelectedFriends.Remove(friend);
            }
            OnPropertyChanged(nameof(CanCreateChat));
        }

        public void RemoveFriendFromSelected(User friend) {
            foreach (var group in GroupedFriends) {
                int index = group.Items.IndexOf(friend);
                if (index == -1) continue;
                if (group.Selected.SelectedIndexes.Contains(index)) {
                    group.Selected.Deselect(index);
                    break;
                }
            }
        }

        private async void CreateChat() {
            if (String.IsNullOrEmpty(ChatName) && SelectedFriends.Count == 0) return;
            List<int> userIds = SelectedFriends.Select(u => u.Id).ToList();
            if (userIds.Count == 0) userIds.Add(session.UserId);

            VKUIWaitDialog<CreateChatResponse> wd = new VKUIWaitDialog<CreateChatResponse>();
            try {
                CreateChatResponse response = await wd.ShowAsync(session.ModalWindow, session.API.Messages.CreateChatAsync(0, userIds, ChatName));
                GoToBackAction?.Invoke();
                session.GetToChat(2000000000 + response.ChatId);
            } catch (Exception ex) {
                if (await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex)) CreateChat();
            }
        }
    }
}

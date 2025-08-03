using Avalonia.Controls.Selection;
using Avalonia.Media.Imaging;
using ELOR.Laney.Core;
using ELOR.Laney.DataModels;
using ELOR.Laney.Helpers;
using ELOR.Laney.Views.Modals;
using ELOR.VKAPILib;
using ELOR.VKAPILib.Methods;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace ELOR.Laney.ViewModels {
    public class ChatCreationViewModel : CommonViewModel {
        private string _chatName;
        private Bitmap _chatPhotoPreview;
        private ObservableCollection<AlphabeticalUsers> _groupedFriends;
        private ObservableCollection<User> _selectedFriends = new ObservableCollection<User>();
        private string _searchQuery;

        private RelayCommand _chatPhotoSetCommand;
        private RelayCommand _customizeChatSettingsCommand;
        private RelayCommand _createCommand;

        public string ChatName { get { return _chatName; } set { _chatName = value; OnPropertyChanged(); } }
        public Bitmap ChatPhotoPreview { get { return _chatPhotoPreview; } set { _chatPhotoPreview = value; OnPropertyChanged(); } }
        public ObservableCollection<AlphabeticalUsers> GroupedFriends { get { return _groupedFriends; } private set { _groupedFriends = value; OnPropertyChanged(); } }
        public ObservableCollection<User> SelectedFriends { get { return _selectedFriends; } set { _selectedFriends = value; OnPropertyChanged(); } }
        public string SearchQuery { get { return _searchQuery; } set { _searchQuery = value; OnPropertyChanged(); } }
        public bool CanCreateChat { get { return !String.IsNullOrEmpty(ChatName) || SelectedFriends.Count > 0; } }

        public RelayCommand ChatPhotoSetCommand { get { return _chatPhotoSetCommand; } private set { _chatPhotoSetCommand = value; OnPropertyChanged(); } }
        public RelayCommand CustomizeChatSettingsCommand { get { return _customizeChatSettingsCommand; } private set { _customizeChatSettingsCommand = value; OnPropertyChanged("CustomizeChatSetingsCommand"); } }
        public RelayCommand CreateCommand { get { return _createCommand; } private set { _createCommand = value; OnPropertyChanged(); } }

        private Dictionary<string, string> _permissions = new Dictionary<string, string> {
            { "invite", "owner_and_admins" },
            { "change_info", "owner_and_admins"},
            { "change_pin", "owner_and_admins" },
            { "use_mass_mentions", "owner_and_admins" },
            { "see_invite_link", "owner_and_admins" },
            { "call", "all" },
            { "change_admins", "owner" },
            { "change_style", "owner_and_admins" }
        };

        // private StorageFile ChatPhoto;
        public readonly ObservableCollection<User> _friends = new ObservableCollection<User>();
        private readonly VKSession _session;
        private readonly System.Action _goToBackAction;

        public ChatCreationViewModel(VKSession session, System.Action goToBackAction) {
            _goToBackAction = goToBackAction;
            _session = session;
            ChatPhotoSetCommand = new RelayCommand((o) => ExceptionHelper.ShowNotImplementedDialog(session.ModalWindow));
            CustomizeChatSettingsCommand = new RelayCommand((o) => OpenPermissionsEditor());
            CreateCommand = new RelayCommand(async (o) => await CreateChatAsync());

            PropertyChanged += (a, b) => {
                switch (b.PropertyName) {
                    case nameof(ChatName):
                        OnPropertyChanged(nameof(CanCreateChat));
                        break;
                }
            };

            new System.Action(async () => await LoadFriendsAsync())();
        }

        private async Task LoadFriendsAsync() {
            if (IsLoading) return;
            IsLoading = true;
            Placeholder = null;
            _friends.Clear();
            GroupedFriends = null;

            try {
                var response = await _session.API.Friends.GetAsync(VKAPIHelper.UserFields, order: FriendsOrder.Name);
                // CacheManager.Add(response.Items);
                foreach (var item in CollectionsMarshal.AsSpan(response.Items)) _friends.Add(item);
                GroupFriends();
            } catch (Exception ex) {
                Placeholder = PlaceholderViewModel.GetForException(ex, async (o) => await LoadFriendsAsync());
            }

            IsLoading = false;
        }

        private void GroupFriends() {
            // TODO: сделать отдельный класс или метод для группировки по алфавиту
            GroupedFriends = new ObservableCollection<AlphabeticalUsers>(_friends.GroupBy(f =>
                !String.IsNullOrEmpty(f.FirstName) ? f.FirstName[0].ToString().ToUpper() : "~")
                .Select(g => new AlphabeticalUsers(g, FriendsSelectionChanged)));
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

        private void OpenPermissionsEditor() {
            new System.Action(async () => {
                ChatEditor modal = new ChatEditor(_session, _permissions);
                var result = await modal.ShowDialog<ChatEditorResult>(_session.Window);
                if (result != null) _permissions = result.Permissions;
            })();
        }

        private async Task CreateChatAsync() {
            if (String.IsNullOrEmpty(ChatName) && SelectedFriends.Count == 0) return;
            List<long> userIds = SelectedFriends.Select(u => u.Id).ToList();
            if (userIds.Count == 0) userIds.Add(_session.UserId);

            string permissions = JsonSerializer.Serialize(_permissions, new JsonSerializerOptions {
                TypeInfoResolver = BuildInJsonContext.Default
            });

            VKUIWaitDialog<CreateChatResponse> wd = new VKUIWaitDialog<CreateChatResponse>();
            try {
                CreateChatResponse response = await wd.ShowAsync(_session.ModalWindow, _session.API.Messages.CreateChatAsync(0, userIds, ChatName, permissions));
                _goToBackAction?.Invoke();
                _session.GoToChat(2000000000 + response.ChatId);
            } catch (Exception ex) {
                if (await ExceptionHelper.ShowErrorDialogAsync(_session.ModalWindow, ex)) await CreateChatAsync();
            }
        }
    }
}
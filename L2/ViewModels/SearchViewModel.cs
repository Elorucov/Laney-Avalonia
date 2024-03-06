using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.DataModels;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ELOR.Laney.ViewModels {
    public class SearchViewModel : ViewModelBase {
        private VKSession session;

        private int _currentTab;
        private string _query;
        private ObservableCollection<Entity> _foundChats;
        private ObservableCollection<FoundMessageItem> _foundMessages;
        private bool _isChatsLoading;
        private bool _isMessagesLoading;
        private PlaceholderViewModel _chatsPlaceholder;
        private PlaceholderViewModel _messagesPlaceholder;

        public int CurrentTab { get { return _currentTab; } set { _currentTab = value; OnPropertyChanged(); } }
        public string Query { get { return _query; } set { _query = value; OnPropertyChanged(); } }
        public ObservableCollection<Entity> FoundChats { get { return _foundChats; } private set { _foundChats = value; OnPropertyChanged(); } }
        public ObservableCollection<FoundMessageItem> FoundMessages { get { return _foundMessages; } private set { _foundMessages = value; OnPropertyChanged(); } }
        public bool IsChatsLoading { get { return _isChatsLoading; } set { _isChatsLoading = value; OnPropertyChanged(); } }
        public bool IsMessagesLoading { get { return _isMessagesLoading; } private set { _isMessagesLoading = value; OnPropertyChanged(); } }
        public PlaceholderViewModel ChatsPlaceholder { get { return _chatsPlaceholder; } private set { _chatsPlaceholder = value; OnPropertyChanged(); } }
        public PlaceholderViewModel MessagesPlaceholder { get { return _messagesPlaceholder; } private set { _messagesPlaceholder = value; OnPropertyChanged(); } }

        public SearchViewModel(VKSession session) {
            this.session = session;

            PropertyChanged += (a, b) => { 
                switch (b.PropertyName) {
                    case nameof(CurrentTab):
                        DoSearch();
                        break;
                }
            };
        }

        public void DoSearch() {
            switch (CurrentTab) {
                case 0:
                    FoundChats?.Clear();
                    if (!String.IsNullOrEmpty(Query)) SearchChats();
                    break;
                case 1:
                    FoundMessages?.Clear();
                    if (!String.IsNullOrEmpty(Query)) SearchMessages();
                    break;
            }
        }

        private async void SearchChats() {
            if (IsChatsLoading) return;
            ChatsPlaceholder = null;
            IsChatsLoading = true;
            int offset = FoundChats != null ? FoundChats.Count : 0;
            try {
                var response = await session.API.Messages.SearchConversationsAsync(session.GroupId, Query, 200, true, VKAPIHelper.Fields);
                IsChatsLoading = false;

                if (response.Count == 0) {
                    ChatsPlaceholder = new PlaceholderViewModel { Text = Localizer.Instance["nothing_found"] };
                    return;
                }

                if (FoundChats == null) FoundChats = new ObservableCollection<Entity>();
                foreach (var chat in response.Items) {
                    long id = chat.Peer.Id;
                    string name = $"{chat.Peer.Type} {chat.Peer.LocalId}";
                    Uri avatar = null;

                    if (chat.Peer.Type == PeerType.User) {
                        User u = response.Profiles.Where(i => i.Id == chat.Peer.LocalId).FirstOrDefault();
                        if (u != null) {
                            name = u.FullName;
                            avatar = u.Photo;
                        }
                    } else if (chat.Peer.Type == PeerType.Group) {
                        Group g = response.Groups.Where(i => i.Id == chat.Peer.LocalId).FirstOrDefault();
                        if (g != null) {
                            name = g.Name;
                            avatar = g.Photo;
                        }
                    } else if (chat.Peer.Type == PeerType.Chat) {
                        name = chat.ChatSettings?.Title;
                        avatar = chat.ChatSettings?.Photo?.Uri;
                    }

                    Entity item = new Entity(id, avatar, name, null, null);
                    FoundChats.Add(item);

                }
            } catch (Exception ex) {
                IsChatsLoading = false;
                if (FoundChats != null && FoundChats.Count > 0) {
                    if (await ExceptionHelper.ShowErrorDialogAsync(session.Window, ex)) SearchChats();
                } else {
                    ChatsPlaceholder = PlaceholderViewModel.GetForException(ex, (o) => SearchChats());
                }
            }
        }

        public async void SearchMessages() {
            if (IsMessagesLoading) return;
            MessagesPlaceholder = null;
            IsMessagesLoading = true;
            int offset = FoundMessages != null ? FoundMessages.Count : 0;
            try {
                var response = await session.API.Messages.SearchAsync(session.GroupId, Query, 0, offset: offset, count: 40, extended: true, fields: VKAPIHelper.Fields);
                IsMessagesLoading = false;

                if (response.Count == 0) {
                    MessagesPlaceholder = new PlaceholderViewModel { Text = Localizer.Instance["nothing_found"] };
                    return;
                }

                CacheManager.Add(response.Profiles);
                CacheManager.Add(response.Groups);
                if (FoundMessages == null) FoundMessages = new ObservableCollection<FoundMessageItem>();

                foreach (var message in response.Items) {
                    Conversation chat = response.Conversations.FirstOrDefault(c => message.PeerId == c.Peer.Id);
                    FoundMessageItem fmi = new FoundMessageItem(message.FromId == session.Id, message, chat);
                    FoundMessages.Add(fmi);
                }
            } catch (Exception ex) {
                IsMessagesLoading = false;
                if (FoundMessages != null && FoundMessages.Count > 0) {
                    if (await ExceptionHelper.ShowErrorDialogAsync(session.Window, ex)) SearchMessages();
                } else {
                    MessagesPlaceholder = PlaceholderViewModel.GetForException(ex, (o) => SearchMessages());
                }
            }
        }
    }
}
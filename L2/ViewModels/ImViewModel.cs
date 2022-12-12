using DynamicData;
using DynamicData.Binding;
using ELOR.Laney.Core;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Methods;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;

namespace ELOR.Laney.ViewModels {
    public sealed class ImViewModel : CommonViewModel {
        private VKSession session;

        private SourceCache<ChatViewModel, int> _chats = new SourceCache<ChatViewModel, int>(c => c.PeerId);
        private ReadOnlyObservableCollection<ChatViewModel> _sortedChats;
        private ChatViewModel _visualSelectedChat;
        private bool _isEmpty = true;

        public ReadOnlyObservableCollection<ChatViewModel> SortedChats { get { return _sortedChats; } }
        public ChatViewModel VisualSelectedChat { get { return _visualSelectedChat; } private set { _visualSelectedChat = value; OnPropertyChanged(); } }
        public bool IsEmpty { get { return _isEmpty; } private set { _isEmpty = value; OnPropertyChanged(); } }

        public ImViewModel(VKSession session) {
            this.session = session;

            session.PropertyChanged += (a, b) => {
                if (b.PropertyName == nameof(VKSession.CurrentOpenedChat))
                    VisualSelectedChat = session.CurrentOpenedChat;
            };

            var observableChats = _chats.Connect();
            var prop = observableChats.WhenPropertyChanged(c => c.SortIndex).Select(_ => Unit.Default);
            var loader = observableChats
                .Sort(SortExpressionComparer<ChatViewModel>.Descending(c => c.SortIndex), prop)
                .TreatMovesAsRemoveAdd()
                .Bind(out _sortedChats)
                .Subscribe(t => {
                    IsEmpty = _chats.Count == 0;
                    VisualSelectedChat = session.CurrentOpenedChat;
                    Debug.WriteLine($"Chats count: {_chats.Count}; sorted count: {_sortedChats.Count}");
                });

            LoadConversations();
        }

        public async void LoadConversations() {
            if (IsLoading) return;
            IsLoading = true;
            Placeholder = null;
            try {
                var response = await session.API.Messages.GetConversationsAsync(session.GroupId, VKAPIHelper.Fields, ConversationsFilter.All, true, 60, _chats.Count);
                CacheManager.Add(response.Profiles);
                CacheManager.Add(response.Groups);

                // List<ChatViewModel> loadedChats = new List<ChatViewModel>();
                foreach (var conv in response.Items) {
                    ChatViewModel chat = CacheManager.GetChat(session.Id, conv.Conversation.Peer.Id);
                    if (chat == null) {
                        chat = new ChatViewModel(session, conv.Conversation, conv.LastMessage);
                        CacheManager.Add(session.Id, chat);
                    }
                    // loadedChats.Add(chat);
                    _chats.AddOrUpdate(chat);
                }
                
            } catch (Exception ex) {
                Placeholder = PlaceholderViewModel.GetForException(ex, () => LoadConversations());
            }
            IsLoading = false;
        }
    }
}
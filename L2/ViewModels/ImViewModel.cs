using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using ELOR.Laney.Core;
using ELOR.Laney.DataModels;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Methods;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace ELOR.Laney.ViewModels {
    public sealed class ImViewModel : CommonViewModel {
        private VKSession session;

        private SourceCache<ChatViewModel, long> _chats = new SourceCache<ChatViewModel, long>(c => c.PeerId);
        private ReadOnlyObservableCollection<ChatViewModel> _sortedChats;
        private ChatViewModel _visualSelectedChat;
        private bool _isEmpty = true;

        public ReadOnlyObservableCollection<ChatViewModel> SortedChats { get { return _sortedChats; } }
        public ChatViewModel VisualSelectedChat { get { return _visualSelectedChat; } private set { _visualSelectedChat = value; OnPropertyChanged(); } }
        public bool IsEmpty { get { return _isEmpty; } private set { _isEmpty = value; OnPropertyChanged(); } }

        public ImViewModel(VKSession session) {
            this.session = session;

            session.CurrentOpenedChatChanged += (a, b) => {
                VisualSelectedChat = SortedChats.Where(c => c.PeerId == b).FirstOrDefault();
            };

            var observableChats = _chats.Connect();
            var prop = observableChats.WhenPropertyChanged(c => c.SortIndex).Select(_ => Unit.Default);
            var loader = observableChats
                .Sort(SortExpressionComparer<ChatViewModel>.Descending(c => c.SortIndex), prop)
                .TreatMovesAsRemoveAdd()
                .Bind(out _sortedChats)
                .Subscribe(t => {
                    IsEmpty = _chats.Count == 0;
                    Debug.WriteLine($"Chats count: {_chats.Count}; sorted count: {_sortedChats.Count}");
                });

            LoadConversations();

            if (!DemoMode.IsEnabled) {
                session.LongPoll.MessageReceived += LongPoll_MessageReceived;
                session.LongPoll.ConversationRemoved += LongPoll_ConversationRemoved;
            }
        }

        public async void LoadConversations() {
            if (DemoMode.IsEnabled) {
                DemoModeSession ds = DemoMode.GetDemoSessionById(session.Id);
                foreach (var conv in ds.Conversations) {
                    ChatViewModel chat = new ChatViewModel(session, conv.Conversation, conv.LastMessage);
                    CacheManager.Add(session.Id, chat);
                    _chats.AddOrUpdate(chat);
                }

                return;
            }

            if (IsLoading) return;
            IsLoading = true;
            Placeholder = null;
            try {
                var response = await session.API.Messages.GetConversationsAsync(session.GroupId, VKAPIHelper.Fields, ConversationsFilter.All, true, 60, _chats.Count);
                CacheManager.Add(response.Profiles);
                CacheManager.Add(response.Groups);

                foreach (var conv in response.Items) {
                    ChatViewModel chat = CacheManager.GetChat(session.Id, conv.Conversation.Peer.Id);
                    if (chat == null) {
                        chat = new ChatViewModel(session, conv.Conversation, conv.LastMessage);
                        CacheManager.Add(session.Id, chat);
                    }
                    _chats.AddOrUpdate(chat);
                }
                
            } catch (Exception ex) {
                if (_chats.Count > 0) {
                    if (await ExceptionHelper.ShowErrorDialogAsync(session.Window, ex)) LoadConversations();
                } else {
                    Placeholder = PlaceholderViewModel.GetForException(ex, (o) => LoadConversations());
                }
            }
            IsLoading = false;
        }

        #region Longpoll events

        private async void LongPoll_MessageReceived(LongPoll longPoll, Message message, int flags) {
            await Dispatcher.UIThread.InvokeAsync(() => {
                var lookup = _chats.Lookup(message.PeerId);
                if (!lookup.HasValue) { // В списке нет, и нам надо его (чат) получить.
                    ChatViewModel chat = CacheManager.GetChat(session.Id, message.PeerId);
                    if (chat == null) {
                        Log.Information($"Received message from peer {message.PeerId}, which is not found in cache");
                        chat = new ChatViewModel(session, message.PeerId, message, true);
                        if (!(IsLoading && _chats.Count == 0)) CacheManager.Add(session.Id, chat);
                    }
                    _chats.AddOrUpdate(chat);
                }
            });
        }

        private async void LongPoll_ConversationRemoved(object sender, long peerId) {
            await Dispatcher.UIThread.InvokeAsync(() => {
                var lookup = _chats.Lookup(peerId);
                if (lookup.HasValue) {
                    _chats.Remove(lookup.Value);
                }
            });
        }

        #endregion
    }
}
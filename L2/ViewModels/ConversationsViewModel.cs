using DynamicData;
using DynamicData.Binding;
using ELOR.Laney.Core;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Methods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;

namespace ELOR.Laney.ViewModels {
    public sealed class ConversationsViewModel : CommonViewModel {
        private VKSession session;

        private SourceList<ChatViewModel> _chats = new SourceList<ChatViewModel>();
        private ReadOnlyObservableCollection<ChatViewModel> _sortedChats;
        private bool _isEmpty = true;

        public ReadOnlyObservableCollection<ChatViewModel> SortedChats { get { return _sortedChats; } }
        public bool IsEmpty { get { return _isEmpty; } private set { _isEmpty = value; OnPropertyChanged(); } }

        public ConversationsViewModel(VKSession session) {
            this.session = session;

            var observableChats = _chats.Connect();
            var loader = observableChats
                .AutoRefresh(c => c.SortIndex)
                .Sort(SortExpressionComparer<ChatViewModel>.Descending(c => c.SortIndex))
                .Bind(out _sortedChats)
                .Subscribe(t => {
                    IsEmpty = _chats.Count == 0;
                    session.TriggerPropertyChangedEvent(nameof(VKSession.CurrentOpenedChat)); // чтобы listbox в conversationsview правильно выделял элемент
                    Debug.WriteLine($"Chats count: {_chats.Count}; sorted count: {_sortedChats.Count}");
                });

            GetConversations();
        }

        public async void GetConversations() {
            IsLoading = true;
            Placeholder = null;
            try {
                var response = await session.API.Messages.GetConversationsAsync(session.GroupId, VKAPIHelper.Fields, ConversationsFilter.All, true);
                CacheManager.Add(response.Profiles);
                CacheManager.Add(response.Groups);

                List<ChatViewModel> loadedChats = new List<ChatViewModel>();
                foreach (var conv in response.Items) {
                    ChatViewModel chat = CacheManager.GetChat(session.Id, conv.Conversation.Peer.Id);
                    if (chat == null) {
                        chat = new ChatViewModel(conv.Conversation, conv.LastMessage);
                        CacheManager.Add(session.Id, chat);
                    }
                    loadedChats.Add(chat);
                }
                _chats.AddRange(loadedChats);
            } catch (Exception ex) {
                Placeholder = PlaceholderViewModel.GetForException(ex, () => GetConversations());
            }
            IsLoading = false;
        }

        #region Binded from UI

        #endregion
    }
}
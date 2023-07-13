using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ELOR.Laney.ViewModels.Modals {
    public enum SharingContentType { Messages, Attachments }

    public class SharingViewModel : CommonViewModel {
        private string _query;
        private ObservableCollection<Tuple<long, Uri, string>> _chats = new ObservableCollection<Tuple<long, Uri, string>>();

        public string Query { get { return _query; } set { _query = value; } }
        public ObservableCollection<Tuple<long, Uri, string>> Chats { get { return _chats; } set { _chats = value; } }

        public SharingContentType Type { get; private set; }
        public long GroupId { get; private set; } // необходим для user-сессии при пересылке сообщений из группы в личку.
        public VKSession Session { get; private set; }

        public SharingViewModel(VKSession session, List<MessageViewModel> messages, long groupId) {
            Type = SharingContentType.Messages;
            GroupId = groupId;
            Session = session;
        }

        public void OnDisplayed() {
            SetCurrentLoadedChats();
        }

        private void SetCurrentLoadedChats() {
            Chats.Clear();
            foreach (var c in Session.ImViewModel.SortedChats) {
                Chats.Add(new Tuple<long, Uri, string>(c.PeerId, c.Avatar, c.Title));
            }
        }

        public async void SearchChats() {
            if (IsLoading) return;
            Chats.Clear();
            Placeholder = null;

            if (String.IsNullOrEmpty(Query)) {
                SetCurrentLoadedChats();
                return;
            }

            IsLoading = true;
            try {
                var response = await Session.API.Messages.SearchConversationsAsync(Session.GroupId, Query, 200, true, VKAPIHelper.Fields);
                IsLoading = false;

                if (response.Count == 0) {
                    Placeholder = new PlaceholderViewModel { Text = Localizer.Instance["nothing_found"] };
                    return;
                }

                CacheManager.Add(response.Profiles);
                CacheManager.Add(response.Groups);

                foreach (var chat in response.Items) {
                    if (!chat.CanWrite.Allowed) continue;
                    long id = chat.Peer.Id;
                    string name = $"{chat.Peer.Type} {chat.Peer.LocalId}";
                    Uri avatar = null;

                    var info = CacheManager.GetNameAndAvatar(id);
                    name = String.Join(" ", new string[] { info.Item1, info.Item2 });
                    avatar = info.Item3;

                    Tuple<long, Uri, string> item = new Tuple<long, Uri, string>(id, avatar, name);
                    Chats.Add(item);
                }
            } catch (Exception ex) {
                IsLoading = false;
                if (Chats != null && Chats.Count > 0) {
                    if (await ExceptionHelper.ShowErrorDialogAsync(Session.ModalWindow, ex)) SearchChats();
                } else {
                    Placeholder = PlaceholderViewModel.GetForException(ex, (o) => SearchChats());
                }
            }
        }
    }
}
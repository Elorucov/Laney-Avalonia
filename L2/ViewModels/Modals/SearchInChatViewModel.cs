using ELOR.Laney.Core;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels.Controls;
using System;
using System.Collections.ObjectModel;

namespace ELOR.Laney.ViewModels.Modals {
    public class SearchInChatViewModel : CommonViewModel {
        private VKSession session;
        private long peerId;

        private string _query;
        private ObservableCollection<MessageViewModel> _messages;

        public string Query { get { return _query; } set { _query = value; OnPropertyChanged(); } }
        public ObservableCollection<MessageViewModel> Messages { get { return _messages; } private set { _messages = value; OnPropertyChanged(); } }

        public SearchInChatViewModel(VKSession session, long peerId) {
            this.session = session;
            this.peerId = peerId;
        }

        public async void DoSearch(bool clear = false) {
            if (clear) Messages = null;
            if (String.IsNullOrEmpty(Query)) return;
            if (IsLoading) return;
            Placeholder = null;
            IsLoading = true;
            int offset = Messages != null ? Messages.Count : 0;
            try {
                var response = await session.API.Messages.SearchAsync(session.GroupId, Query, peerId, offset: offset, count: 40, extended: true, fields: VKAPIHelper.Fields);
                IsLoading = false;

                if (response.Count == 0) {
                    Placeholder = new PlaceholderViewModel { Text = Assets.i18n.Resources.nothing_found };
                    return;
                }

                CacheManager.Add(response.Profiles);
                CacheManager.Add(response.Groups);

                var msgs = MessageViewModel.BuildFromAPI(response.Items, session);
                if (Messages == null) {
                    Messages = new ObservableCollection<MessageViewModel>(msgs);
                } else {
                    foreach (var msg in msgs) {
                        Messages.Add(msg);
                    }
                }
            } catch (Exception ex) {
                IsLoading = false;
                if (Messages != null && Messages.Count > 0) {
                    if (await ExceptionHelper.ShowErrorDialogAsync(session.Window, ex)) DoSearch();
                } else {
                    Placeholder = PlaceholderViewModel.GetForException(ex, (o) => DoSearch());
                }
            }
        }
    }
}
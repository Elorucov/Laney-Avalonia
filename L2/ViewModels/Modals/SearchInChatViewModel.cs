using ELOR.Laney.Core;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ELOR.Laney.ViewModels.Modals {
    public class SearchInChatViewModel : CommonViewModel {
        private VKSession session;
        private long peerId;

        private string _query;
        private ObservableCollection<Message> _messages;

        public string Query { get { return _query; } set { _query = value; OnPropertyChanged(); } }
        public ObservableCollection<Message> Messages { get { return _messages; } private set { _messages = value; OnPropertyChanged(); } }

        public SearchInChatViewModel(VKSession session, long peerId) {
            this.session = session;
            this.peerId = peerId;
        }

        public async Task DoSearchAsync(bool clear = false) {
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

                if (Messages == null) {
                    Messages = new ObservableCollection<Message>(response.Items);
                } else {
                    foreach (var msg in response.Items) {
                        Messages.Add(msg);
                    }
                }
            } catch (Exception ex) {
                IsLoading = false;
                if (Messages != null && Messages.Count > 0) {
                    if (await ExceptionHelper.ShowErrorDialogAsync(session.Window, ex)) await DoSearchAsync();
                } else {
                    Placeholder = PlaceholderViewModel.GetForException(ex, async (o) => await DoSearchAsync());
                }
            }
        }
    }
}
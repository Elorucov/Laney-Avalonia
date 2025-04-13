using ELOR.Laney.Core;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels.Controls;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ELOR.Laney.ViewModels.Modals {
    public class ImportantMessagesViewModel : CommonViewModel {
        private VKSession session;

        private ObservableCollection<Message> _messages = new ObservableCollection<Message>();
        private int _count = 0;

        public ObservableCollection<Message> Messages { get { return _messages; } private set { _messages = value; OnPropertyChanged(); } }
        public int Count { get { return _count; } private set { _count = value; OnPropertyChanged(); } }

        public int CustomOffset { get; private set; }

        public ImportantMessagesViewModel(VKSession session) {
            this.session = session;
        }

        public async Task LoadAsync(int offset = -1) {
            if (IsLoading) return;
            if (offset < 0) {
                offset = Messages.Count + CustomOffset;
            } else {
                CustomOffset = offset;
            }
            try {
                Placeholder = null;
                IsLoading = true;

                var response = await session.API.Messages.GetImportantMessagesAsync(session.GroupId, offset, 10, 0, 0, true, VKAPIHelper.Fields);
                CacheManager.Add(response.Profiles);
                CacheManager.Add(response.Groups);
                Count = response.Messages.Count;
                foreach (var message in response.Messages.Items) {
                    Messages.Add(message);
                }
            } catch (Exception ex) {
                if (Messages.Count > 0) {
                    if (await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex)) await LoadAsync(offset);
                } else {
                    Placeholder = PlaceholderViewModel.GetForException(ex, async (o) => await LoadAsync(offset));
                }
            }
            IsLoading = false;
        }

        public void RemoveMessageFromLoaded(Message message) {
            Messages.Remove(message);
            Count--;
        }
    }
}
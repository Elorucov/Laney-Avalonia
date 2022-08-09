using ELOR.Laney.ViewModels.Controls;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.Laney.Collections {
    public class MessagesCollection2 {
        public ObservableCollection<object> DisplayableElements { get; private set; } = new ObservableCollection<object>();
        public List<MessageViewModel> Messages => 
            DisplayableElements.Where(de => de.GetType() == typeof(MessageViewModel)).Cast<MessageViewModel>().ToList();

        public int Count => DisplayableElements.Count;
        public int MessagesCount => Messages.Count;
        public MessageViewModel First => Messages?.FirstOrDefault();
        public MessageViewModel Last => Messages?.LastOrDefault();

        public MessagesCollection2(List<MessageViewModel> messages) {
            for (int i = 0; i < messages.Count; i++) {
                MessageViewModel message = messages[i];

                bool isPrevFromSameSender = false;
                if (i == 0) {
                    DisplayableElements.Add(message.SentTime.Date);
                } else {
                    var prev = messages[i - 1];
                    isPrevFromSameSender = prev.SenderId == message.SenderId && prev.SentTime.Date == message.SentTime.Date;
                    if (prev.SentTime.Date != message.SentTime.Date) DisplayableElements.Add(message.SentTime.Date);
                }


                bool isNextFromSameSender = false;
                if (i == messages.Count - 1) {
                    isNextFromSameSender = false;
                } else {
                    var next = messages[i + 1];
                    isNextFromSameSender = next.SenderId == message.SenderId && next.SentTime.Date == message.SentTime.Date;
                }

                message.UpdateSenderInfoView(isPrevFromSameSender, isNextFromSameSender);
                DisplayableElements.Add(message);
            }
        }

        public void Insert(MessageViewModel message) {
            int idx = 0;
            var q = DisplayableElements.Where(obj => obj is MessageViewModel msg && msg.Id == message.Id).FirstOrDefault();
            if (q != null && q is MessageViewModel old) {
                idx = DisplayableElements.IndexOf(old);
                Remove(old);
            } else {
                idx = DisplayableElements.ToList().BinarySearch(message);
                if (idx < 0) idx = ~idx;
            }

            if (idx == 1) {
                //
            } else {
                var prev = DisplayableElements[idx - 1];
                if (prev is MessageViewModel prevmsg) {
                    bool isPrevFromSameSender = prevmsg.SenderId == message.SenderId && prevmsg.SentTime.Date == message.SentTime.Date;
                    prevmsg.UpdateSenderInfoView(null, isPrevFromSameSender);
                }
            }

            if (idx == DisplayableElements.Count - 1) {
                //
            } else {
                var next = DisplayableElements[idx + 1];
                if (next is MessageViewModel nextmsg) {
                    bool isNextFromSameSender = nextmsg.SenderId == message.SenderId && nextmsg.SentTime.Date == message.SentTime.Date;
                    nextmsg.UpdateSenderInfoView(isNextFromSameSender, null);
                }
            }
        }

        public void InsertRange(List<MessageViewModel> messages) {
            foreach (var message in CollectionsMarshal.AsSpan<MessageViewModel>(messages)) {
                Insert(message);
            }
        }

        public void Remove(MessageViewModel message) {
            int index = DisplayableElements.IndexOf(message);
            if (index == -1) return;
            DisplayableElements.Remove(message);
            
            if (index == DisplayableElements.Count) {
                var prev = DisplayableElements[index - 1];
                if (prev is MessageViewModel prevmsg) prevmsg.UpdateSenderInfoView(null, false);
            } else if (index == 1) {
                var next = DisplayableElements[1];
                if (next is MessageViewModel nextmsg) nextmsg.UpdateSenderInfoView(false, null);
            } else if (index > 1 && index < DisplayableElements.Count) {
                var prev = DisplayableElements[index - 1];
                var repl = DisplayableElements[index];
                if (prev is DateTime) prev = DisplayableElements[index - 2];
                if (repl is DateTime) prev = DisplayableElements[index + 1];

                MessageViewModel prevmsg = prev as MessageViewModel;
                MessageViewModel replmsg = repl as MessageViewModel;
                bool isMessagesFromSameSender = prevmsg.SenderId == replmsg.SenderId && prevmsg.SentTime.Date == replmsg.SentTime.Date;
                prevmsg.UpdateSenderInfoView(null, isMessagesFromSameSender);
                replmsg.UpdateSenderInfoView(isMessagesFromSameSender, null);
            }
        }

        public MessageViewModel GetById(int messageId) {
            return Messages.Where(m => m.Id == messageId).FirstOrDefault();
        }

        public void RemoveById(int messageId) {
            var message = GetById(messageId);
            if (message != null) Remove(message);
        }

        public void Clear() {
            DisplayableElements.Clear();
        }
    }
}

using DynamicData;
using ELOR.Laney.ViewModels.Controls;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace ELOR.Laney.Collections {
    public class MessagesCollection : ObservableCollection<MessageViewModel> {
        public MessageViewModel First => this.FirstOrDefault();
        public MessageViewModel Last => this.LastOrDefault();

        public MessagesCollection(List<MessageViewModel> messages) {
            for (int i = 0; i < messages.Count; i++) {
                MessageViewModel message = messages[i];

                bool isPrevFromSameSender = false;
                bool isDateBetweenVisible = false;
                if (i == 0) {
                    isPrevFromSameSender = false;
                    isDateBetweenVisible = true;
                } else {
                    var prev = messages[i - 1];
                    isPrevFromSameSender = prev.SenderId == message.SenderId && prev.SentTime.Date == message.SentTime.Date;
                    isDateBetweenVisible = prev.SentTime.Date != message.SentTime.Date;
                }


                bool isNextFromSameSender = false;
                if (i == messages.Count - 1) {
                    isNextFromSameSender = false;
                } else {
                    var next = messages[i + 1];
                    isNextFromSameSender = next.SenderId == message.SenderId && next.SentTime.Date == message.SentTime.Date;
                }

                message.UpdateSenderInfoView(isPrevFromSameSender, isNextFromSameSender);
                message.UpdateDateBetweenVisibility(isDateBetweenVisible);
                Items.Add(message);
            }
        }

        public void Insert(MessageViewModel message) {
            int idx = 0;

            var q = Items.Where(obj => obj is MessageViewModel msg && msg.ConversationMessageId == message.ConversationMessageId).FirstOrDefault();
            if (q != null && q is MessageViewModel old) {
                idx = Items.IndexOf(old);
                RemoveAt(idx);
            } else {
                idx = Items.ToList().BinarySearch(message);
                if (idx < 0) idx = ~idx;
            }

            Insert(idx, message);
            UpdateSenderInfoView(message);
        }

        public void InsertRange(List<MessageViewModel> messages) {
            foreach (var message in CollectionsMarshal.AsSpan<MessageViewModel>(messages)) {
                Insert(message);
            }
        }

        public void Remove(MessageViewModel message) {
            int index = Items.IndexOf(message);
            if (index == -1) return;
            RemoveAt(index);
            if (Count == 0) return;
            if (index > 0) UpdateSenderInfoView(this.ElementAt(index - 1));
            if (Count > index) UpdateSenderInfoView(this.ElementAt(index));
        }

        private void UpdateSenderInfoView(MessageViewModel msg) {
            int index = IndexOf(msg);

            if (Count == 1) {
                msg.UpdateSenderInfoView(false, false);
                msg.UpdateDateBetweenVisibility(true);
            } else if (Count > 1) {
                bool isPrevFromSameSender = false;
                bool isNextFromSameSender = false;

                if (index > 0) {
                    var prev = this[index - 1];
                    isPrevFromSameSender = msg.SenderId == prev.SenderId && msg.SentTime.Date == prev.SentTime.Date;
                    prev.UpdateSenderInfoView(null, isPrevFromSameSender);
                    msg.UpdateDateBetweenVisibility(prev.SentTime.Date != msg.SentTime.Date);
                }
                if (index < Count - 1) {
                    var next = this[index + 1];
                    isNextFromSameSender = msg.SenderId == next.SenderId && msg.SentTime.Date == next.SentTime.Date;
                    next.UpdateSenderInfoView(isNextFromSameSender, null);
                    next.UpdateDateBetweenVisibility(next.SentTime.Date != msg.SentTime.Date);
                }
                msg.UpdateSenderInfoView(isPrevFromSameSender, isNextFromSameSender);
            }
        }

        public MessageViewModel GetById(int messageId) {
            return this.Where(m => m.ConversationMessageId == messageId).FirstOrDefault();
        }

        public void RemoveById(int messageId) {
            var message = GetById(messageId);
            if (message != null) Remove(message);
        }
    }
}

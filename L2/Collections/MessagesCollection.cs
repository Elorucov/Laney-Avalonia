using ELOR.Laney.ViewModels.Controls;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace ELOR.Laney.Collections {
    public class GroupedMessagesCollection : ObservableCollection<MessagesCollectionGroupItem> {
        public GroupedMessagesCollection(MessagesCollection messages) {
            if (messages != null && messages.Count > 0) {
                var groups = messages.GroupBy(m => m.SentTime.Date);
                foreach (var g in groups) {
                    MessagesCollectionGroupItem mc = new MessagesCollectionGroupItem(g.Key, g.ToList());
                    Add(mc);
                    Debug.WriteLine($"GroupedMessagesCollection: Key = {g.Key}, Count = {mc.Count}");
                }
            }
        }

        // Получает MessagesCollectionGroupItem, в котором содержится сообщение с id-ом messageId
        public MessagesCollectionGroupItem GetGroupThatHasContainsMessage(int messageId, out int indexInGroup) {
            foreach (var group in Items) {
                for (int i = 0; i < group.Count; i++) {
                    var message = group[i];
                    if (message.Id == messageId) {
                        indexInGroup = i;
                        return group;
                    }
                }
            }
            indexInGroup = -1;
            return null;
        }

        public void Insert(MessageViewModel message) {
            var q = from g in Items where g.Key == message.SentTime.Date select g;
            if (q.Count() == 1) {
                q.First().Insert(message);
                Debug.WriteLine($"GroupedMessagesCollection: Message {message.Id} inserted in group {q.First().Key}");
            } else {
                MessagesCollectionGroupItem group = new MessagesCollectionGroupItem(message.SentTime.Date, new List<MessageViewModel> { message });
                int idx = this.ToList().BinarySearch(group);
                if (idx < 0) idx = ~idx;
                Insert(idx, group);
                Debug.WriteLine($"GroupedMessagesCollection: Message {message.Id} inserted in new group (idx: {idx})");
            }
        }

        public void Remove(MessageViewModel message) {
            var q = from g in Items where g.Key == message.SentTime.Date select g;
            if (q.Count() == 1) {
                MessagesCollectionGroupItem i = q.FirstOrDefault();
                if (i != null) {
                    i.Remove(message);
                    if (i.Count == 0) Remove(i);
                    Debug.WriteLine($"GroupedMessagesCollection: Message {message.Id} removed from group {i.Key}");
                }
            }
        }
    }

    public class MessagesCollection : ObservableCollection<MessageViewModel> {
        public GroupedMessagesCollection GroupedMessages { get; set; }

        public MessagesCollection() { }

        public MessagesCollection(List<MessageViewModel> messages, bool doNotGrouping = false) {
            for (int i = 0; i < messages.Count; i++) {
                MessageViewModel message = messages[i];

                bool isPrevFromSameSender = false;
                if (i == 0) {
                    isPrevFromSameSender = false;
                } else {
                    var prev = messages[i - 1];
                    isPrevFromSameSender = prev.SenderId == message.SenderId && prev.SentTime.Date == message.SentTime.Date;
                }


                bool isNextFromSameSender = false;
                if (i == messages.Count - 1) {
                    isNextFromSameSender = false;
                } else {
                    var next = messages[i + 1];
                    isNextFromSameSender = next.SenderId == message.SenderId && next.SentTime.Date == message.SentTime.Date;
                }

                message.UpdateSenderInfoView(isPrevFromSameSender, isNextFromSameSender);
                Add(message);
            }
            if (!doNotGrouping) CreateGroup();
        }

        public MessagesCollection(List<Message> messages, bool doNotGrouping = false) {
            foreach (Message msg in messages) {
                Add(new MessageViewModel(msg));
            }
            if (!doNotGrouping) CreateGroup();
        }

        public void Insert(MessageViewModel message) {
            int idx = 0;
            var q = from m in Items where m.Id == message.Id select m;
            if (q.Count() == 1) {
                MessageViewModel old = q.First();
                idx = IndexOf(old);
                Remove(old);
                if (GroupedMessages != null) GroupedMessages.Remove(old);
                Insert(idx, message);
            } else {
                idx = this.ToList().BinarySearch(message);
                if (idx < 0) idx = ~idx;
                Insert(idx, message);
            }

            bool isPrevFromSameSender = false;
            if (idx == 0) {
                isPrevFromSameSender = false;
            } else {
                var prev = Items[idx - 1];
                isPrevFromSameSender = prev.SenderId == message.SenderId && prev.SentTime.Date == message.SentTime.Date;
                prev.UpdateSenderInfoView(null, isPrevFromSameSender);
            }


            bool isNextFromSameSender = false;
            if (idx == Items.Count - 1) {
                isNextFromSameSender = false;
            } else {
                var next = Items[idx + 1];
                isNextFromSameSender = next.SenderId == message.SenderId && next.SentTime.Date == message.SentTime.Date;
                next.UpdateSenderInfoView(isNextFromSameSender, null);
            }

            message.UpdateSenderInfoView(isPrevFromSameSender, isNextFromSameSender);
            if (GroupedMessages != null) GroupedMessages.Insert(message);
        }

        public void InsertRange(List<MessageViewModel> messages) {
            foreach (var message in CollectionsMarshal.AsSpan<MessageViewModel>(messages)) {
                Insert(message);
            }
        }

        public new void Remove(MessageViewModel message) {
            int index = IndexOf(message);
            if (index == -1) return;
            base.Remove(message);

            if (Count == 0) {
                if (GroupedMessages != null) GroupedMessages.Remove(message);
                return;
            }
            if (index == Count) {
                var prev = Items[index - 1];
                prev.UpdateSenderInfoView(null, false);
            } else if (index == 0) {
                var next = Items[0];
                next.UpdateSenderInfoView(false, null);
            } else if (index > 0 && index < Count) {
                var prev = Items[index - 1];
                var repl = Items[index];
                bool isMessagesFromSameSender = prev.SenderId == repl.SenderId && prev.SentTime.Date == repl.SentTime.Date;
                prev.UpdateSenderInfoView(null, isMessagesFromSameSender);
                repl.UpdateSenderInfoView(isMessagesFromSameSender, null);
            }

            if (GroupedMessages != null) GroupedMessages.Remove(message);
        }

        public MessageViewModel GetById(int messageId) {
            return Items.Where(m => m.Id == messageId).FirstOrDefault();
        }

        public void RemoveById(int messageId) {
            var message = Items.Where(m => m.Id == messageId).FirstOrDefault();
            if (message != null) Remove(message);
        }

        //public void Insert(Message message) {
        //    Insert(new MessageViewModel(message));
        //}

        private void CreateGroup() {
            GroupedMessages = new GroupedMessagesCollection(this);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            base.OnCollectionChanged(e);
            if (e.Action == NotifyCollectionChangedAction.Reset) {
                if (GroupedMessages != null) GroupedMessages.Clear();
            }
        }
    }

    public class MessagesCollection<TKey> : MessagesCollection, IComparable {
        public MessagesCollection(TKey key, List<MessageViewModel> messages) : base(messages, true) {
            Key = key;
        }

        public TKey Key { get; private set; }

        public int CompareTo(object obj) {
            if (obj is MessagesCollection<DateTime> msgd && Key is DateTime dt) {
                return dt.CompareTo(msgd.Key);
            }
            throw new InvalidOperationException("No comparable TKey.");
        }
    }

    public class MessagesCollectionGroupItem : MessagesCollection<DateTime> {
        public MessagesCollectionGroupItem(DateTime key, List<MessageViewModel> messages) : base(key, messages) { }
    }
}
using ELOR.Laney.ViewModels;
using NeoSmart.Unicode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ELOR.Laney.DataModels {
    public class TwoStringTuple : Tuple<string, string> {
        public TwoStringTuple(string item1, string item2) : base(item1, item2) { }
    }

    public class TwoStringBindable : ViewModelBase {
        private bool _enabled = true;
        private string _item1;
        private string _item2;

        public bool Enabled { get { return _enabled; } set { _enabled = value; OnPropertyChanged(nameof(Enabled)); } }
        public string Item1 { get { return _item1; } set { _item1 = value; OnPropertyChanged(nameof(Item1)); } }
        public string Item2 { get { return _item2; } set { _item2 = value; OnPropertyChanged(nameof(Item2)); } }
    }

    public record Entity {
        public long Id { get; private set; }
        public Uri ImageUri { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public List<string> IconIds { get; private set; }
        public Command Command { get; private set; }

        public Entity(long id, Uri imageUri, string name, string description, Command command) {
            Id = id;
            ImageUri = imageUri;
            Name = name;
            Description = description;
            Command = command;
            IconIds = new List<string>();
        }
    }

    public class ReactionGroup : Tuple<int, int, List<Entity>> {
        public ReactionGroup(int item1, int item2, List<Entity> item3) : base(item1, item2, item3) { }
    }

    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement> {
        readonly List<TElement> elements;

        public Grouping(IGrouping<TKey, TElement> grouping) {
            if (grouping == null) throw new ArgumentException(nameof(grouping));
            Key = grouping.Key;
            elements = grouping.ToList();
        }

        public TKey Key { get; private set; }

        public IEnumerator<TElement> GetEnumerator() {
            return elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }

    public class EmojiGroup : Grouping<string, SingleEmoji> {
        public EmojiGroup(IGrouping<string, SingleEmoji> grouping) : base(grouping) { }
    }
}
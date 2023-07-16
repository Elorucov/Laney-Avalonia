using Avalonia.Controls.Selection;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ELOR.Laney.DataModels {
    public class GroupWithSelection<TKey, TElement> {
        public ObservableCollection<TElement> Items { get; private set; }
        public SelectionModel<TElement> Selected { get; private set; }
        public TKey Key { get; private set; }

        public GroupWithSelection(IGrouping<TKey, TElement> grouping, EventHandler<SelectionModelSelectionChangedEventArgs<TElement>> selectionChanged) {
            if (grouping == null) throw new ArgumentException(nameof(grouping));
            Key = grouping.Key;
            Items = new ObservableCollection<TElement>(grouping.ToList());
            Selected = new SelectionModel<TElement>() {
                SingleSelect = false,
                Source = Items
            };
            Selected.SelectionChanged += (a, b) => selectionChanged?.Invoke(a, b);
        }
    }

    public class AlphabeticalUsers : GroupWithSelection<string, User> {
        public AlphabeticalUsers(IGrouping<string, User> grouping, EventHandler<SelectionModelSelectionChangedEventArgs<User>> selectionChanged) : base(grouping, selectionChanged) {}
    }
}

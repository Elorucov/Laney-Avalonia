using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ELOR.Laney.ViewModels {
    public class ItemsViewModel<T> : CommonViewModel {
        private ObservableCollection<T> _items;

        public ObservableCollection<T> Items => _items;

        public ItemsViewModel() {
            _items = new ObservableCollection<T>();
        }

        // Allowing the class owner to modify items in collection that sent to "items" param.
        public ItemsViewModel(ObservableCollection<T> items) {
            _items = items;
        }

        public ItemsViewModel(IEnumerable<T> items) {
            _items = new ObservableCollection<T>(items);
        }
    }
}
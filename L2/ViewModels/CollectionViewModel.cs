using System.Collections.ObjectModel;

namespace ELOR.Laney.ViewModels {
    public class CollectionViewModel<T> : CommonViewModel {
        private ObservableCollection<T> _items = new ObservableCollection<T>();

        public ObservableCollection<T> Items { get { return _items; } }
    }
}
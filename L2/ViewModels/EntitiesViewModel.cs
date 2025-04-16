using ELOR.Laney.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.Laney.ViewModels {
    public sealed class EntitiesViewModel : ItemsViewModel<Entity> {
        public EntitiesViewModel(IEnumerable<Entity> items) : base(items) { }

        public EntitiesViewModel(ObservableCollection<Entity> items) : base(items) { }
    }
}

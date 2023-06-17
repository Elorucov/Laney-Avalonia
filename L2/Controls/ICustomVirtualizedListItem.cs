using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.Laney.Controls {
    public interface ICustomVirtalizedListItem {
        void OnAppearedOnScreen();

        void OnDisappearedFromScreen();
    }
}

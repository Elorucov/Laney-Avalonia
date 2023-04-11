using Avalonia.Controls;
using VKUI.Controls;

namespace ELOR.Laney.Views {
    public partial class SearchView : Page {
        public SearchView() {
            InitializeComponent();

            BackButton.Click += (a, b) => NavigationRouter.BackAsync();
        }
    }
}
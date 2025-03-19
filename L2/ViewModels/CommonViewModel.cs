namespace ELOR.Laney.ViewModels {
    public class CommonViewModel : ViewModelBase {
        private bool _isLoading;
        private PlaceholderViewModel _placeholder;

        public bool IsLoading { get { return _isLoading; } set { _isLoading = value; OnPropertyChanged(); } }
        public PlaceholderViewModel Placeholder { get { return _placeholder; } set { _placeholder = value; OnPropertyChanged(); } }
    }
}
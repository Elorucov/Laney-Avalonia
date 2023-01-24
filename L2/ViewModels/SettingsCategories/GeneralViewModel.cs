namespace ELOR.Laney.ViewModels.SettingsCategories {
    public class GeneralViewModel : CommonViewModel {
        string _test = "Test";

        public string Test { get => _test; private set { _test = value; OnPropertyChanged(); } }

        public GeneralViewModel() { }
    }
}
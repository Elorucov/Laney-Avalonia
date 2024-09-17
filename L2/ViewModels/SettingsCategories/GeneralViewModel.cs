using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.DataModels;
using System.Collections.ObjectModel;
using System.Linq;

namespace ELOR.Laney.ViewModels.SettingsCategories {
    public class GeneralViewModel : CommonViewModel {
        public ObservableCollection<TwoStringTuple> Languages { get; private set; } = Localizer.SupportedLanguages;

        public TwoStringTuple CurrentLanguage { get { return GetLang(); } set { ChangeLang(value); OnPropertyChanged(); } }
        public bool SentViaEnter { get { return Settings.SentViaEnter; } set { Settings.SentViaEnter = value; OnPropertyChanged(); } }
        public bool DontParseLinks { get { return Settings.DontParseLinks; } set { Settings.DontParseLinks = value; OnPropertyChanged(); } }
        public bool DisableMentions { get { return Settings.DisableMentions; } set { Settings.DisableMentions = value; OnPropertyChanged(); } }
        
        public bool SuggestStickers { get { return Settings.SuggestStickers; } set { Settings.SuggestStickers = value; OnPropertyChanged(); } }
        public bool AnimateStickers { get { return Settings.AnimateStickers; } set { Settings.AnimateStickers = value; OnPropertyChanged(); } }

        public GeneralViewModel() { 
            
        }

        private TwoStringTuple GetLang() {
            var id = Settings.Get(Settings.LANGUAGE, Constants.DefaultLang);
            return Languages.Where(l => l.Item1 == id).FirstOrDefault();
        }

        private void ChangeLang(TwoStringTuple value) {
            Settings.Set(Settings.LANGUAGE, value.Item1);
            Localizer.LoadLanguage(value.Item1);
            OnPropertyChanged(nameof(CurrentLanguage));
        }
    }
}
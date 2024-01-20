using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.DataModels;
using ELOR.Laney.Views.Modals;

namespace ELOR.Laney.Views.SettingsCategories {
    public partial class General : UserControl {
        string oldLangId;
        public General() {
            InitializeComponent();
            oldLangId = Settings.Get(Settings.LANGUAGE, Constants.DefaultLang);
        }

        private async void ComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
            Window window = TopLevel.GetTopLevel(this) as Window;
            if (window == null) return;

            if (e.AddedItems.Count == 1) {
                TwoStringTuple value = e.AddedItems[0] as TwoStringTuple;
                if (value.Item1 == oldLangId) return;

                VKUIDialog alert = new VKUIDialog(Localizer.Instance["restart_required"], Localizer.Instance["restart_required_ext"]);
                await alert.ShowDialog(window);
            }
        }
    }
}
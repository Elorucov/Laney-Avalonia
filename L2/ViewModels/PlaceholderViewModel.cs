using Avalonia.Controls;
using Avalonia.Media;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Helpers;
using VKUI.Controls;
using System;

namespace ELOR.Laney.ViewModels {
    public sealed class PlaceholderViewModel : ViewModelBase {
        private Control _icon;
        private string _header;
        private string _text;
        private string _actionButton;
        private RelayCommand _actionButtonFunc;
        private object _data;

        public Control Icon { get { return _icon; } private set { _icon = value; OnPropertyChanged(); } }
        public string Header { get { return _header; } private set { _header = value; OnPropertyChanged(); } }
        public string Text { get { return _text; } private set { _text = value; OnPropertyChanged(); } }
        public string ActionButton { get { return _actionButton; } private set { _actionButton = value; OnPropertyChanged(); } }
        public RelayCommand ActionButtonFunc { get { return _actionButtonFunc; } private set { _actionButtonFunc = value; OnPropertyChanged(); } }
        public object Data { get { return _data; } private set { _data = value; OnPropertyChanged(); } }

        public static PlaceholderViewModel GetForException(Exception ex, Action<object> function = null) {
            var err = ExceptionHelper.GetDefaultErrorInfo(ex);
            return new PlaceholderViewModel() {
                Data = ex,
                Icon = new VKIcon { Id = VKIconNames.Icon56ErrorOutline },
                Header = err.Item1,
                Text = err.Item2,
                ActionButton = Localizer.Instance["retry"],
                ActionButtonFunc = new RelayCommand(function)
            };
        }
    }
}
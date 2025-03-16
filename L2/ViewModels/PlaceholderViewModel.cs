using Avalonia.Controls;
using ELOR.Laney.Helpers;
using System;
using VKUI.Controls;

namespace ELOR.Laney.ViewModels {
    public sealed class PlaceholderViewModel : ViewModelBase {
        private Control _icon;
        private string _header;
        private string _text;
        private string _actionButton;
        private RelayCommand _actionButtonFunc;
        private object _data;

        public Control Icon { get { return _icon; } set { _icon = value; OnPropertyChanged(); } }
        public string Header { get { return _header; } set { _header = value; OnPropertyChanged(); } }
        public string Text { get { return _text; } set { _text = value; OnPropertyChanged(); } }
        public string ActionButton { get { return _actionButton; } set { _actionButton = value; OnPropertyChanged(); } }
        public RelayCommand ActionButtonFunc { get { return _actionButtonFunc; } set { _actionButtonFunc = value; OnPropertyChanged(); } }
        public object Data { get { return _data; } set { _data = value; OnPropertyChanged(); } }

        public static PlaceholderViewModel GetForException(Exception ex, Action<object> function = null) {
            var err = ExceptionHelper.GetDefaultErrorInfo(ex);
            return new PlaceholderViewModel() {
                Data = ex,
                Icon = new VKIcon { Id = VKIconNames.Icon56ErrorOutline },
                Header = err.Item1,
                Text = err.Item2,
                ActionButton = Assets.i18n.Resources.retry,
                ActionButtonFunc = new RelayCommand(function)
            };
        }
    }
}
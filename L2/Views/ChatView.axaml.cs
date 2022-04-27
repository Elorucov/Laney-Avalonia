using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using VKUI.Controls;
using System;

namespace ELOR.Laney.Views {
    public sealed partial class ChatView : UserControl, IMainWindowRightView {
        public ChatView() {
            InitializeComponent();
            BackButton.Click += (a, b) => BackButtonClick?.Invoke(this, null);
        }

        public event EventHandler BackButtonClick;
        public void ChangeBackButtonVisibility(bool isVisible) {
            BackButton.IsVisible = isVisible;
        }
    }
}
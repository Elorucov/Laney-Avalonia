using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using System;

namespace VKUI.Windows {
    public class DialogWindow : Window {
        public DialogWindow() {
            Classes.Add("Dialog");
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Activated += DialogWindow_Activated;
        }

        private void DialogWindow_Activated(object sender, EventArgs e) {
            Activated -= DialogWindow_Activated;
            TryFocus();
        }

        // Autofocus to window
        private void TryFocus() {
            var focusable = this.FindDescendantOfType<InputElement>();
            if (focusable != null) focusable.Focus();
        }
    }
}
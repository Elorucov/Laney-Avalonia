using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.Extensions;
using System;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals {
    public sealed partial class VKUIDialog : DialogWindow {
        string header, text;
        string[] buttons;
        int primaryButton = 0;

        public static StyledProperty<object> DialogContentProperty = StyledProperty<object>.Register<VKUIDialog, object>(nameof(DialogContent));

        public object DialogContent { get { return GetValue(DialogContentProperty); } set { SetValue(DialogContentProperty, value); CheckContent(); } }

        public VKUIDialog() {
            InitializeComponent();
        }

        public VKUIDialog(string header, string text) {
            InitializeComponent();
            this.FixDialogWindows(TitleBar, ContentRoot);
            Setup(header, text, new string[] { "OK" });
        }

        public VKUIDialog(string header, string text, string[] buttons, int primaryButton = 0) {
            InitializeComponent();
            this.FixDialogWindows(TitleBar, ContentRoot);
            Setup(header, text, buttons, primaryButton);
        }

        private void Setup(string header, string text, string[] buttons, int primaryButton = 0) {
            TitleBar.CanShowTitle = false;
            if (string.IsNullOrEmpty(header)) throw new ArgumentException("Parameter must not be null!", nameof(header));
            if (buttons == null) throw new ArgumentException("Buttons must not be null!", nameof(buttons));
            if (buttons.Length == 0 || buttons.Length > 3)
                throw new ArgumentException("Buttons count must be > 0 and <= 3!", nameof(buttons));

            Title = header;
            this.header = header;
            this.text = text;
            this.buttons = buttons;
            this.primaryButton = primaryButton;

            DlgHeader.Text = header;
            DlgText.IsVisible = !String.IsNullOrEmpty(text);
            DlgText.Text = text;

            for (int i = 1; i <= buttons.Length; i++) {
                Button button = new Button { 
                    Tag = i,
                    Content = buttons[i - 1],
                    Margin = new Thickness(8, 0, 0, 0)
                };
                button.Classes.Add("Medium");
                if (i == primaryButton) button.Classes.Add("Primary");
                button.Click += Button_Click;
                Buttons.Children.Add(button);
            }

            CheckContent();
        }

        private void CheckContent() {
            ContentArea.Content = DialogContent;
            ContentArea.IsVisible = DialogContent != null;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Button button = sender as Button;
            int id = (int)button.Tag;
            Close(id);
        }
    }
}
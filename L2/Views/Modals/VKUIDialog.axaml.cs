using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;
using ELOR.Laney.Extensions;
using System;
using System.Collections.Generic;

namespace ELOR.Laney.Views.Modals {
    public sealed partial class VKUIDialog : Window {
        string header, text;
        Dictionary<short, string> buttons;
        short primaryButtonId;

        public static StyledProperty<object> DialogContentProperty = StyledProperty<object>.Register<VKUIDialog, object>(nameof(DialogContent));

        public object DialogContent { get { return GetValue<object>(DialogContentProperty); } set { SetValue(DialogContentProperty, value); CheckContent(); } }

        public VKUIDialog() {
            InitializeComponent();
        }

        public VKUIDialog(string header, string text) {
            InitializeComponent();
            this.FixDecoration();
            Setup(header, text, new Dictionary<short, string> { { 0, "OK" } }, 0);
        }

        public VKUIDialog(string header, string text, Dictionary<short, string> buttons, short primaryButtonId = 0) {
            InitializeComponent();
            this.FixDecoration();
            Setup(header, text, buttons, primaryButtonId);
        }

        private void Setup(string header, string text, Dictionary<short, string> buttons, short primaryButtonId = 0) {
            if (string.IsNullOrEmpty(header)) throw new ArgumentException("Parameter must not be null!", nameof(header));
            if (buttons == null) throw new ArgumentException("Buttons must not be null!", nameof(buttons));
            if (buttons.Count == 0 || buttons.Count > 3)
                throw new ArgumentException("Buttons count must be > 0 and <= 3!", nameof(buttons));

            this.header = header;
            this.text = text;
            this.buttons = buttons;
            this.primaryButtonId = primaryButtonId;

#if DEBUG
            this.AttachDevTools();
#endif

            DlgHeader.Text = header;
            DlgText.IsVisible = !String.IsNullOrEmpty(text);
            DlgText.Text = text;

            foreach (var b in buttons) {
                Button button = new Button { 
                    Tag = b.Key,
                    Content = b.Value,
                    Margin = new Thickness(8, 0, 0, 0)
                };
                if (b.Key == primaryButtonId) button.Classes.Add("Primary");
                button.Click += Button_Click;
                Buttons.Children.Add(button);
            }

            CheckContent();
        }

        private void CheckContent() {
            ContentArea.Content = DialogContent;
            ContentArea.IsVisible = DialogContent != null;
        }

        private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Button button = sender as Button;
            short id = (short)button.Tag;
            Close(id);
        }

        #region Static

        public static Dictionary<short, string> OkButton {
            get {
                return new Dictionary<short, string> {
                    { 1, "Ok" }
                };
            }
        }

        #endregion
    }
}
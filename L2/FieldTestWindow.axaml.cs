using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using VKUI;

namespace ELOR.Laney {
    public partial class FieldTestWindow : Window {
        public FieldTestWindow() {
            InitializeComponent();
            test = this.FindControl<TextBlock>("test");
            buildInfo = this.FindControl<TextBlock>("buildInfo");
            setResult = this.FindControl<TextBlock>("setResult");
            settingDemo = this.FindControl<TextBox>("settingDemo");

            EffectiveViewportChanged += MainWindow_EffectiveViewportChanged;

            this.FindControl<Button>("ben").Click += ben_Click;
            this.FindControl<Button>("bru").Click += bru_Click;
            this.FindControl<Button>("buk").Click += buk_Click;

            this.FindControl<Button>("btl").Click += btl_Click;
            this.FindControl<Button>("btd").Click += btd_Click;

            this.FindControl<Button>("getBtn").Click += getBtn_Click;
            this.FindControl<Button>("setBtn").Click += setBtn_Click;

            this.FindControl<Button>("w1").Click += w1_Click;

            buildInfo.Text = $"Build tag: {App.BuildInfoFull}";
            setResult.Text += $"\n\nSettings file location:\n{Settings.FilePath}";
        }

        private void ben_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Localizer.Instance.LoadLanguage("en-US");
        }

        private void bru_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Localizer.Instance.LoadLanguage("ru-RU");
        }

        private void buk_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Localizer.Instance.LoadLanguage("uk-UA");
        }

        private void btl_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            App.SwitchTheme(VKUIScheme.BrightLight);
        }

        private void btd_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            App.SwitchTheme(VKUIScheme.SpaceGray);
        }

        private void MainWindow_EffectiveViewportChanged(object? sender, Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            var t = e.EffectiveViewport;
            test.Text = $"Size: {t.Size.Width}x{t.Size.Height}";
        }

        private void getBtn_Click(object? sender, RoutedEventArgs e) {
            string test = Settings.Get<string>(Settings.TEST_STRING);
            setResult.Text = test;
        }

        private void setBtn_Click(object? sender, RoutedEventArgs e) {
            Settings.Set(Settings.TEST_STRING, settingDemo.Text);
        }

        private void w1_Click(object? sender, RoutedEventArgs e) {
            this.Width = 360;
            this.Height = 640;
        }
    }
}
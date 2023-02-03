using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using System;

namespace ELOR.Laney {
    public partial class FieldTestWindow : Window {
        public FieldTestWindow() {
            InitializeComponent();

            EffectiveViewportChanged += MainWindow_EffectiveViewportChanged;

            getBtn.Click += getBtn_Click;
            setBtn.Click += setBtn_Click;

            checkLinkBtn.Click += CheckLinkBtn_Click;

            w1.Click += w1_Click;

            buildInfo.Text = $"Build tag: {App.BuildInfoFull}";
            setResult.Text += $"\n\nSettings file location:\n{Settings.FilePath}";
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

        private void CheckLinkBtn_Click(object sender, RoutedEventArgs e) {
            string url = linkBox.Text;
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) {
                routerResult.Text = "Invalid URL!";
                return;
            }

            var result = Router.LaunchLink(VKSession.Main, url);
            routerResult.Text = $"Type: {result.Item1}";
        }

        private void w1_Click(object? sender, RoutedEventArgs e) {
            this.Width = 360;
            this.Height = 640;
        }
    }
}
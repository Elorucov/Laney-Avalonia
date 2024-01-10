using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.Helpers;
using ELOR.Laney.Views.Modals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

            string test = "Testing VK links (ColorTextBlock lib): [id172894294|user], [club171015120|group], https://elor.top and [https://vk.com/wall-171015120_363|wall post], hee-hee!";
            TextParser.SetText(test, aeTest, async (link) => {
                VKUIDialog dlg = new VKUIDialog("Link test", link);
                await dlg.ShowDialog(this);
            });

            TestAnimatedSticker();
        }

        private async void TestAnimatedSticker() {
            await Task.Delay(1000);
            var file = await CacheManager.GetFileFromCacheAsync(new Uri("https://vk.com/sticker/3-8476.json"));
            if (file) {
                lottieSticker.Stretch = Avalonia.Media.Stretch.Uniform;
                lottieSticker.StretchDirection = Avalonia.Media.StretchDirection.Both;
                lottieSticker.RepeatCount = 10;
                lottieSticker.Path = $"file://{Path.Combine(App.LocalDataPath, "cache", "3-8476.json").Replace("\\", "/")}";
            }
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
            var bb2 = AssetsManager.OpenAsset(new Uri("avares://laney/Assets/Audio/bb2.mp3"));
            AudioPlayer.SFX?.Play(bb2);
        }
    }
}
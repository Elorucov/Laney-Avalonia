using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ELOR.Laney.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Network;
using ELOR.Laney.Helpers;
using ELOR.Laney.Views.Modals;
using System;
using System.Threading.Tasks;
using VKUI.Controls;
using VKUI.Popups;

namespace ELOR.Laney {
    public partial class FieldTestWindow : Window {
        public FieldTestWindow() {
            InitializeComponent();
            EffectiveViewportChanged += MainWindow_EffectiveViewportChanged;

            getBtn.Click += getBtn_Click;
            setBtn.Click += setBtn_Click;

            checkLinkBtn.Click += CheckLinkBtn_Click;

            w1.Click += w1_Click;
            w2.Click += w2_Click;
            w3.Click += w3_Click;

            sr1.Click += sr1_Click;

            buildInfo.Text = $"The infos listed below is changed when building on CI/CD\nBuild tag: {App.BuildInfoFull}\nRepository: {App.RepoInfo}\nBuilder username and hostname: {App.BuildHost}";
            setResult.Text += $"\n\nSettings file location:\n{Settings.FilePath}";

            string test = "Testing VK links (ColorTextBlock lib): [id172894294|user], [club171015120|group], https://elor.top and [https://vk.com/wall-171015120_363|wall post], hee-hee!";
            TextParser.SetText(test, aeTest, async (link) => {
                VKUIDialog dlg = new VKUIDialog("Link test", link);
                await dlg.ShowDialog(this);
            });

            AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
            AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            AddHandler(DragDrop.DropEvent, OnDrop);

            PTarget.Effect = new ImmutableDropShadowEffect(0, 4, 8, Color.FromRgb(0, 0, 0), 0.2);
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

            new System.Action(async () => {
                if (VKSession.Main == null) {
                    await new VKUIDialog("Sorry", "Main session is not available!").ShowDialog(this);
                    return;
                }
                var result = await Router.LaunchLink(VKSession.Main, url);
                routerResult.Text = $"Type: {result.Item1}";
            })();
        }

        private void w1_Click(object? sender, RoutedEventArgs e) {
            var bb2 = AssetsManager.OpenAsset(new Uri("avares://laney/Assets/Audio/bb2.mp3"));
            LMediaPlayer.SFX?.PlayStream(bb2);
        }

        private void w2_Click(object? sender, RoutedEventArgs e) {
            new Action(async () => {
                var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                    AllowMultiple = false
                });

                if (files?.Count > 0) {
                    var file = files[0];
                    using var stream = await file.OpenReadAsync();
                    LMediaPlayer.SFX?.PlayStream(stream);
                }
            })();
        }

        private void w3_Click(object? sender, RoutedEventArgs e) {
            LMediaPlayer.SFX?.PlayURL("https://elor.top/res/audios/sunrise_spring.mp3");
        }

        private void sr1_Click(object sender, RoutedEventArgs e) {
            srt1.Children.Clear();
            new Action(async () => {
                foreach (var url in new string[] { "https://example.com", "https://elor.top", "https://vk.ru/terms" }) {
                    await TestNetworkAsync(url);
                }
                foreach (var url in new string[] { "https://elor.top/res/images/sample_test/sample_3x4_01.jpg", "https://elor.top/res/images/sample_test/sample_1920x1200_01.jpg", "https://elor.top/res/images/sample_test/sample_16x9_02.jpg" }) {
                    TestNetwork2(url);
                }
            })();
        }

        private async Task TestNetworkAsync(string url) {
            var response = await LNet.GetSequentialAsync(new Uri(url));
            var type = response.Content.Headers.ContentType;
            await Dispatcher.UIThread.InvokeAsync(() => {
                srt1.Children.Add(new TextBlock { Text = $"{url}: {type}" });
            });
        }

        private void TestNetwork2(string url) {
            Avalonia.Controls.Image img = new Image() {
                Width = 256,
                Height = 144,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            ImageLoader.SetSource(img, new Uri(url));
            srt1.Children.Add(img);
        }

        private void OnDragEnter(object sender, DragEventArgs e) {
            Title = "Drag enter!";
        }

        private void OnDragLeave(object sender, DragEventArgs e) {
            Title = "Drag leave!";
        }

        private void OnDrop(object sender, DragEventArgs e) {
            Title = "Drop!";
        }

        private void Button_Click(object? sender, RoutedEventArgs e) {
            PTest.IsOpen = true;
        }

        private void ActionSheetTest(object? sender, RoutedEventArgs e) {
            ActionSheet ash = new ActionSheet();
            ash.Items.Add(new ActionSheetItem() {
                Header = "Item 1",
                Before = new VKIcon { Id = VKIconNames.Icon20FavoriteOutline }
            });
            ash.Items.Add(new ActionSheetItem() {
                Header = "Item 2"
            });
            ash.Items.Add(new ActionSheetItem());
            ash.Items.Add(new ActionSheetItem() {
                Header = "Checked",
                Before = new VKIcon { Id = VKIconNames.Icon20Check }
            });
            ash.ShowAt(sender as Button);
        }
    }
}
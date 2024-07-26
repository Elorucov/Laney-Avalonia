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

        private async void vt01_Click(object? sender, RoutedEventArgs e) {
            var formats = await Clipboard.GetFormatsAsync();
            string str = String.Join(", ", formats);
            string export = Path.Combine(App.LocalDataPath, "clipboardtest");
            if (formats.Length > 0) {
                Directory.CreateDirectory(export);

                int result = await new VKUIDialog("Save data from clipboard?", $"Clipboard contains {formats.Length} object(s): {str}.\n\nAll these object saved as binary file in:\n{export}\n\nContinue?", new string[] { "Yes", "No" }, 1).ShowDialog<int>(this);
                if (result == 1) {
                    Dictionary<string, string> nonbinary = new Dictionary<string, string>();
                    foreach (string format in formats) {
                        try {
                            var data = await Clipboard.GetDataAsync(format);
                            if (data == null) {
                                nonbinary.Add(format, "empty");
                                continue;
                            } else if (data is byte[] binary) {
                                string path = Path.Combine(export, $"{format}.bin");
                                var fs = File.Create(path);
                                await fs.WriteAsync(binary);
                                await fs.FlushAsync();
                            } else if (data is string text) {
                                string path = Path.Combine(export, $"{format}.txt");
                                await File.WriteAllTextAsync(path, text);
                            } else {
                                nonbinary.Add(format, data.GetType().ToString());
                                continue;
                            }
                        } catch (Exception ex) {
                            nonbinary.Add(format, $"failed with {ex.GetType()}\n");
                            continue;
                        }
                    }

                    if (nonbinary.Count > 0) {
                        string str2 = "";
                        foreach (var nb in nonbinary) {
                            str2 += $"{nb.Key}: {nb.Value}\n";
                        }
                        await new VKUIDialog(nonbinary.Count == formats.Length ? "Failed to save all objects!" : "Failed to save some objects", str2.Trim()).ShowDialog<int>(this);
                    } else {
                        await new VKUIDialog("All done!", $"Check folder:\n{export}").ShowDialog<int>(this);
                    }
                }
            } else {
                await new VKUIDialog("Failed", "Clipboard is empty.").ShowDialog<int>(this);
            }
        }
    }
}
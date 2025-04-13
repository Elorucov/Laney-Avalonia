using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ELOR.Laney.Core;
using ELOR.Laney.Helpers;
using ELOR.Laney.Views.Modals;
using System;
using System.Collections.Generic;
using System.IO;

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

        private void vt01_Click(object? sender, RoutedEventArgs e) {
            new System.Action(async () => {
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
            })();
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
    }
}
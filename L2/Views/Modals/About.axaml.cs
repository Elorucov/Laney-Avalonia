using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using System;
using System.Runtime.InteropServices;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals {
    public partial class About : DialogWindow {
        public About() {
            InitializeComponent();
            versionCell.After = $"{App.BuildInfo}";
            dotnetVersionCell.After = RuntimeInformation.FrameworkDescription;
            buildTimeCell.After = App.BuildTime;

            string str = String.Empty;
            AssetsManager.Check(out str);
            dev.Text = $"{Localizer.Instance["about_dev"]} {str.Substring(str.Length / 2)}";
#if MAC
            TitleBar.CanShowTitle = true;
#elif LINUX
            TitleBar.IsVisible = false;
#endif

#if RELEASE
            byte i = 2;
#elif BETA
            TextBlock t = new TextBlock { 
                    Text = "BETA",
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeight.Bold
                };
                t.Classes.Add("Caption2");

                Border b = new Border {
                    Width = 36,
                    Height = 14,
                    CornerRadius = new Avalonia.CornerRadius(0, 2, 2, 0),
                    Background = new SolidColorBrush(Color.Parse("#D1C097")),
                    Child = t
                };

                Path p = new Path { 
                    Data = Geometry.Parse("M 0,14 L 10,24 L 10,14 z"),
                    Fill = new SolidColorBrush(Color.Parse("#857250"))
                };

                Canvas c = new Canvas { 
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                    Width = 36,
                    Height = 26,
                    Margin = new Avalonia.Thickness(2, 0, 0, 7)
                };

                c.Children.Add(b);
                c.Children.Add(p);
                Logo.Children.Add(c);
#else
            TextBlock t = new TextBlock {
                Text = "DEV",
                Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeight.Bold
            };
            t.Classes.Add("Caption2");

            Border b = new Border {
                Width = 36,
                Height = 14,
                CornerRadius = new Avalonia.CornerRadius(0, 2, 2, 0),
                Background = new SolidColorBrush(Color.Parse("#D1C097")),
                Child = t
            };

            Path p = new Path {
                Data = Geometry.Parse("M 0,14 L 10,24 L 10,14 z"),
                Fill = new SolidColorBrush(Color.Parse("#857250"))
            };

            Canvas c = new Canvas {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                Width = 36,
                Height = 26,
                Margin = new Avalonia.Thickness(2, 0, 0, 7)
            };

            c.Children.Add(b);
            c.Children.Add(p);
            Logo.Children.Add(c);
#endif
        }

        private void b01_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Launcher.LaunchUrl("https://vk.com/elorlaney");
        }

        private void b02_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Launcher.LaunchUrl("https://vk.com/privacy");
        }

        private void b03_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Launcher.LaunchUrl("https://vk.com/terms");
        }

        private async void b04_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            VKUIDialog dlg = new VKUIDialog(Localizer.Instance["about_libs"], String.Join("\n", App.UsedLibs));
            await dlg.ShowDialog(this);
        }
    }
}
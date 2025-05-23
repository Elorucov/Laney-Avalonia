using Avalonia.Controls;
using ELOR.Laney.Core;
using System;
using System.Runtime.InteropServices;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals {
    public partial class About : DialogWindow {
        public About() {
            InitializeComponent();
#if RELEASE
            versionCell.After = $"{App.BuildInfo}";
#else
            versionCell.After = $"{App.BuildInfo} ({(App.ExpirationDate - DateTime.Now.Date).Days} day(s) to expire)";
#endif
            dotnetVersionCell.After = RuntimeInformation.FrameworkDescription;
            buildTimeCell.After = App.BuildTime.ToString("dd MMM yyyy");
            launchTimeCell.After = TimeSpan.FromMilliseconds(Program.LaunchTime).ToString(@"%s\.fff") + " sec.";

            string str = String.Empty;
            dev.Text = $"{Assets.i18n.Resources.about_dev} {Assets.i18n.Resources.about_dev2}";
#if MAC
            TitleBar.CanShowTitle = true;
#elif LINUX
            TitleBar.IsVisible = false;
#endif

            App.UpdateBranding(Logo.Child as Grid);
        }

        private void b00_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            new Action(async () => await Launcher.LaunchUriAsync(new Uri("https://github.com/Elorucov/Laney-Avalonia")))();
        }

        private void b01_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            new Action(async () => await Launcher.LaunchUriAsync(new Uri("https://vk.com/elorlaney")))();
        }

        private void b02_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            new Action(async () => await Launcher.LaunchUriAsync(new Uri("https://vk.com/privacy")))();
        }

        private void b03_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            new Action(async () => await Launcher.LaunchUriAsync(new Uri("https://vk.com/terms")))();
        }

        private void b04_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            new Action(async () => {
                string libs = string.Join("\n", App.UsedLibs);
                libs += $"\n\nLibVLC version: {LMediaPlayer.LibVersion}";
                VKUIDialog dlg = new VKUIDialog(Assets.i18n.Resources.about_libs, libs);
                await dlg.ShowDialog(this);
            })();
        }
    }
}
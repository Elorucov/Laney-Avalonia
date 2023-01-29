using Avalonia.Controls;
using ELOR.Laney.Core;
using System.IO;

namespace ELOR.Laney.Views.SettingsCategories {
    public partial class DebugPage : UserControl {
        public DebugPage() {
            InitializeComponent();
            InitSettings();
        }

        private void InitSettings() {
            l01.IsChecked = Settings.EnableLogs;
            l01.Click += (a, b) => Settings.EnableLogs = (bool)(a as ToggleSwitch).IsChecked;

            l02.IsChecked = Settings.EnableLongPollLogs;
            l02.Click += (a, b) => Settings.EnableLongPollLogs = (bool)(a as ToggleSwitch).IsChecked;

            p01.IsChecked = Settings.ShowFPS;
            p01.Click += (a, b) => Settings.ShowFPS = (bool)(a as ToggleSwitch).IsChecked;

            p02.IsChecked = Settings.ShowDebugCounters;
            p02.Click += (a, b) => Settings.ShowDebugCounters = (bool)(a as ToggleSwitch).IsChecked;
        }

        private void b01_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Launcher.LaunchFolder(Path.Combine(App.LocalDataPath, "logs"));
        }
    }
}
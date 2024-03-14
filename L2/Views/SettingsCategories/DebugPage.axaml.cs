using Avalonia.Controls;
using ELOR.Laney.Core;
using System.IO;

namespace ELOR.Laney.Views.SettingsCategories {
    public partial class DebugPage : UserControl {
        public DebugPage() {
            InitializeComponent();
            InitSettings();
            lc01.Subtitle = Path.Combine(App.LocalDataPath, "logs");

#if BETA || RELEASE
            LogsCells.Children.Remove(lc05);
			ToolsCells.Children.Remove(c01);
            ToolsCells.Children.Remove(c02);
            ToolsCells.Children.Remove(c04);
            ToolsCells.Children.Remove(c05);
#endif

#if RELEASE
            LogsCells.Children.Remove(lc04);
#endif
        }

        private void InitSettings() {
            l01.IsChecked = Settings.EnableLogs;
            l01.Click += (a, b) => Settings.EnableLogs = (bool)(a as ToggleSwitch).IsChecked;

            l02.IsChecked = Settings.EnableLongPollLogs;
            l02.Click += (a, b) => Settings.EnableLongPollLogs = (bool)(a as ToggleSwitch).IsChecked;

            l03.IsChecked = Settings.BitmapManagerLogs;
            l03.Click += (a, b) => Settings.BitmapManagerLogs = (bool)(a as ToggleSwitch).IsChecked;

#if !RELEASE

            l04.IsChecked = Settings.LNetLogs;
            l04.Click += (a, b) => Settings.LNetLogs = (bool)(a as ToggleSwitch).IsChecked;
			
#endif

            p03.IsChecked = Settings.ShowRAMUsage;
            p03.Click += (a, b) => Settings.ShowRAMUsage = (bool)(a as ToggleSwitch).IsChecked;
			
            p06.IsChecked = Settings.MessagesListVirtualization;
            p06.Click += (a, b) => Settings.MessagesListVirtualization = (bool)(a as ToggleSwitch).IsChecked;

            p07.IsChecked = Settings.ShowDebugInfoInGallery;
            p07.Click += (a, b) => Settings.ShowDebugInfoInGallery = (bool)(a as ToggleSwitch).IsChecked;

#if !BETA && !RELEASE

            l05.IsChecked = Settings.MessageRenderingLogs;
            l05.Click += (a, b) => Settings.MessageRenderingLogs = (bool)(a as ToggleSwitch).IsChecked;

            p01.IsChecked = Settings.ShowFPS;
            p01.Click += (a, b) => Settings.ShowFPS = (bool)(a as ToggleSwitch).IsChecked;

            p02.IsChecked = Settings.ShowDebugCounters;
            p02.Click += (a, b) => Settings.ShowDebugCounters = (bool)(a as ToggleSwitch).IsChecked;

            p04.IsChecked = Settings.ShowDevItemsInContextMenus;
            p04.Click += (a, b) => Settings.ShowDevItemsInContextMenus = (bool)(a as ToggleSwitch).IsChecked;

            p05.IsChecked = Settings.DisableMarkingMessagesAsRead;
            p05.Click += (a, b) => Settings.DisableMarkingMessagesAsRead = (bool)(a as ToggleSwitch).IsChecked;

#endif
        }

        private void b01_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Launcher.LaunchFolder(Path.Combine(App.LocalDataPath, "logs"));
        }
    }
}
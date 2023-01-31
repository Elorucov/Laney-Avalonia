using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using Serilog;
using System;
using System.Diagnostics;

namespace ELOR.Laney.Views {
    public sealed partial class MainWindow : Window {
        public VKSession Session => DataContext as VKSession;

        public MainWindow() {
            InitializeComponent();
            Log.Information($"{nameof(MainWindow)} initialized.");

            Unloaded += MainWindow_Unloaded;
            Activated += MainWindow_Activated;
            EffectiveViewportChanged += MainWindow_EffectiveViewportChanged;
            ChatView.BackButtonClick += ChatView_BackButtonClick;

            // TODO: запомнить и восстановить размер и положение окна.
            Width = 800; Height = 540;
            this.Position = new PixelPoint(128, 64);

            Renderer.DrawFps = Settings.ShowFPS;
            RAMInfoOverlay.IsVisible = Settings.ShowRAMUsage;
            ToggleRAMInfoOverlay();
            Settings.SettingChanged += Settings_SettingChanged;
        }

        private void MainWindow_Unloaded(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Unloaded -= MainWindow_Unloaded;
            Activated -= MainWindow_Activated;
            EffectiveViewportChanged -= MainWindow_EffectiveViewportChanged;
            ChatView.BackButtonClick -= ChatView_BackButtonClick;
            Settings.SettingChanged -= Settings_SettingChanged;
        }

        private void MainWindow_EffectiveViewportChanged(object? sender, Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            CheckAdaptivity(e.EffectiveViewport.Width);
        }

        private void MainWindow_Activated(object? sender, EventArgs e) {
            Program.StopStopwatch();
            Log.Information($"{nameof(MainWindow)} activated. Launch time: {Program.LaunchTime} ms.");
            Activated -= MainWindow_Activated;
            Closing += MainWindow_Closing;
            VKSession.GetByDataContext(this).PropertyChanged += SessionPropertyChanged;
            Title = $"{VKSession.GetByDataContext(this).Name} - Laney";
        }

        private void SessionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            VKSession session = sender as VKSession;
            if (e.PropertyName == nameof(VKSession.Name)) {
                Title = $"{session.Name} - Laney";
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            Hide();
        }

        private void Settings_SettingChanged(string key, object value) {
            if (key == Settings.DEBUG_FPS) {
                Renderer.DrawFps = (bool)value;
            }
            switch (key) {
                case Settings.DEBUG_FPS:
                    Renderer.DrawFps = (bool)value;
                    break;
                case Settings.DEBUG_COUNTERS_RAM:
                    RAMInfoOverlay.IsVisible = (bool)value;
                    ToggleRAMInfoOverlay();
                    break;
            }
        }

        DispatcherTimer ramTimer = null;
        private void ToggleRAMInfoOverlay() {
            if (Settings.ShowRAMUsage) {
                UpdateRAMUsageInfo();
                if (ramTimer == null) {
                    ramTimer = new DispatcherTimer {
                        Interval = TimeSpan.FromMilliseconds(500)
                    };
                    ramTimer.Tick += (a, b) => UpdateRAMUsageInfo();
                }
                ramTimer.Start();
            } else {
                if (ramTimer != null) {
                    ramTimer.Stop();
                }
            }
        }

        private void UpdateRAMUsageInfo() {
            long ram = Process.GetCurrentProcess().PrivateMemorySize64;
            double rammb = (double)ram / 1048576;
            RAMInfo.Text = $"{Math.Round(rammb, 1)} Mb";
        }

        #region Adaptivity and convsview / chatview navigation

        bool isWide = false;
        bool isRightSideDisplaying = false;

        private void CheckAdaptivity(double width) {
            isWide = width >= 720;

            if (!isWide) {
                Grid.SetColumnSpan(ConvsView, 2);
                Grid.SetColumn(ChatViewContainer, 0);
                Grid.SetColumnSpan(ChatViewContainer, 2);
                Separator.IsVisible = false;

                ConvsView.IsVisible = !isRightSideDisplaying;
                ChatViewContainer.IsVisible = isRightSideDisplaying;
            } else {
                Grid.SetColumnSpan(ConvsView, 1);
                Grid.SetColumn(ChatViewContainer, 1);
                Grid.SetColumnSpan(ChatViewContainer, 1);
                Separator.IsVisible = true;

                ConvsView.IsVisible = true;
                ChatViewContainer.IsVisible = true;
            }

            ChatView.ChangeBackButtonVisibility(!isWide);
        }

        public void SwitchToSide(bool toRight) {
            isRightSideDisplaying = toRight;
            CheckAdaptivity(Bounds.Width);
        }

        private void ChatView_BackButtonClick(object? sender, EventArgs e) {
            SwitchToSide(false);
        }

        #endregion
    }
}
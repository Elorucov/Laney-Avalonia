using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using ELOR.Laney.Core;
using Serilog;
using System;
using System.Collections.Generic;
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

            // Window size, position & state
            // На данный момент есть баг со стороны Авалонии,
            // github.com/AvaloniaUI/Avalonia/issues/8869

            bool isMaximized = Settings.Get(Settings.WIN_MAXIMIZED, false);
            if (!isMaximized) WindowState = WindowState.Normal;

            Width = Settings.Get<double>(Settings.WIN_SIZE_W, 800);
            Height = Settings.Get<double>(Settings.WIN_SIZE_H, 600);
            int wx = Settings.Get(Settings.WIN_POS_X, 128);
            int wy = Settings.Get(Settings.WIN_POS_Y, 32);
            Position = new PixelPoint(wx, wy);

            Opened += MainWindow_Opened;

            Renderer.DrawFps = Settings.ShowFPS;
            RAMInfoOverlay.IsVisible = Settings.ShowRAMUsage;
            ToggleRAMInfoOverlay();
            Settings.SettingChanged += Settings_SettingChanged;
        }

        // Bug workaround
        private void MainWindow_Opened(object sender, EventArgs e) {
            Opened -= MainWindow_Opened;
            bool isMaximized = Settings.Get(Settings.WIN_MAXIMIZED, false);
            WindowState = isMaximized ? WindowState.Maximized : WindowState.Normal;
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
            SaveWindowParameters();
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

        private void SaveWindowParameters() {
            if (WindowState == WindowState.Maximized) {
                Settings.Set(Settings.WIN_MAXIMIZED, true);
            } else {
                Settings.SetBatch(new Dictionary<string, object> {
                    { Settings.WIN_SIZE_W, Width },
                    { Settings.WIN_SIZE_H, Height },
                    { Settings.WIN_POS_X, Position.X },
                    { Settings.WIN_POS_Y, Position.Y },
                    { Settings.WIN_MAXIMIZED, false }
                });
            }
        }

        #region RAM info

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

        #endregion

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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering;
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

            bool isMaximized = Settings.Get(Settings.WIN_MAXIMIZED, false);
            if (!isMaximized) WindowState = WindowState.Normal;

            Width = Settings.Get<double>(Settings.WIN_SIZE_W, 800);
            Height = Settings.Get<double>(Settings.WIN_SIZE_H, 600);
            int wx = Settings.Get(Settings.WIN_POS_X, 128);
            int wy = Settings.Get(Settings.WIN_POS_Y, 32);
            if (wx < 0) wx = 128;
            if (wy < 0) wy = 32;
            Position = new PixelPoint(wx, wy);

            RendererDiagnostics.DebugOverlays = Settings.ShowFPS ? RendererDebugOverlays.Fps : RendererDebugOverlays.None;
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

        private async void MainWindow_Activated(object? sender, EventArgs e) {
            Program.StopStopwatch();
            Log.Information($"{nameof(MainWindow)} activated. Launch time: {Program.LaunchTime} ms.");
            Activated -= MainWindow_Activated;
            Closing += MainWindow_Closing;
            VKSession.GetByDataContext(this).PropertyChanged += SessionPropertyChanged;
            SetSessionNameInWindowTitle(VKSession.GetByDataContext(this).Name);

            await LeftNav.NavigationRouter.NavigateToAsync(new ImView());
        }

        private void SessionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            VKSession session = sender as VKSession;
            if (e.PropertyName == nameof(VKSession.Name)) SetSessionNameInWindowTitle(session.Name);
        }

        private void SetSessionNameInWindowTitle(string name) {
#if RELEASE
            Title = $"{name} - Laney";
#elif BETA
            Title = $"{name} - Laney beta";
#else
            Title = $"{name} - Laney dev";
#endif
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            SaveWindowParameters();
            Hide();
        }

        private void Settings_SettingChanged(string key, object value) {
            switch (key) {
                case Settings.DEBUG_FPS:
                    RendererDiagnostics.DebugOverlays = (bool)value ? RendererDebugOverlays.Fps : RendererDebugOverlays.None;
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
                Grid.SetColumnSpan(LeftNav, 2);
                Grid.SetColumn(ChatViewContainer, 0);
                Grid.SetColumnSpan(ChatViewContainer, 2);
                Separator.IsVisible = false;

                LeftNav.IsVisible = !isRightSideDisplaying;
                ChatViewContainer.IsVisible = isRightSideDisplaying;
            } else {
                Grid.SetColumnSpan(LeftNav, 1);
                Grid.SetColumn(ChatViewContainer, 1);
                Grid.SetColumnSpan(ChatViewContainer, 1);
                Separator.IsVisible = true;

                LeftNav.IsVisible = true;
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
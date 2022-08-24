using Avalonia;
using Avalonia.Controls;
using ELOR.Laney.Core;
using Serilog;
using System;

namespace ELOR.Laney.Views {
    public sealed partial class MainWindow : Window {
        public VKSession Session => DataContext as VKSession;

        public MainWindow() {
            InitializeComponent();
            Log.Information($"{nameof(MainWindow)} initialized.");

            Activated += MainWindow_Activated;
            EffectiveViewportChanged += MainWindow_EffectiveViewportChanged;
            ChatView.BackButtonClick += ChatView_BackButtonClick;

            // TODO: запомнить и восстановить размер и положение окна.
            Width = 800; Height = 540;
            this.Position = new PixelPoint(128, 64);
        }

        private void MainWindow_EffectiveViewportChanged(object? sender, Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            CheckAdaptivity(e.EffectiveViewport.Width);
        }

        private void MainWindow_Activated(object? sender, EventArgs e) {
            Log.Information($"{nameof(MainWindow)} activated.");
            this.Activated -= MainWindow_Activated;
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
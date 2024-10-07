using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace ELOR.Laney.Controls;

public partial class MiniAudioPlayer : UserControl {
    public MiniAudioPlayer() {
        InitializeComponent();
    }

    public event EventHandler Click;
    public event EventHandler CloseButtonClick;

    private void OnClick(object? sender, RoutedEventArgs e) {
        Click?.Invoke(this, e);
    }

    private void OnCloseButtonClick(object? sender, RoutedEventArgs e) {
        CloseButtonClick?.Invoke(this, e);
    }
}
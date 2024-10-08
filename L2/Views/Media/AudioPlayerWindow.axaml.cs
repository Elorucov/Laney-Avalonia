using Avalonia;
using Avalonia.Controls;
using ELOR.Laney.DataModels;
using ELOR.Laney.ViewModels;
using VKUI.Windows;

namespace ELOR.Laney.Views;

public partial class AudioPlayerWindow : DialogWindow {
    public AudioPlayerWindow() {
        InitializeComponent();
        ShowInTaskbar = true;

        AudioPlayerViewModel.InstancesChanged += AudioPlayerViewModel_InstancesChanged;
        Unloaded += AudioPlayerWindow_Unloaded;

#if WIN
        TrackName.Margin = new Thickness(0, 0, 36, 0);
#elif MAC
        WTitleBar.CanShowTitle = true;
        TBStub.Height = 27;
#elif LINUX
        WTitleBar.IsVIsible = false;
#endif
    }

    private void AudioPlayerViewModel_InstancesChanged(object sender, System.EventArgs e) {
        if (AudioPlayerViewModel.MainInstance != null) {
            DataContext = AudioPlayerViewModel.MainInstance;
        } else {
            if (currentOpenedWindow == this) currentOpenedWindow = null;
            Close();
        }
    }

    private void AudioPlayerWindow_Unloaded(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
        AudioPlayerViewModel.InstancesChanged -= AudioPlayerViewModel_InstancesChanged;
        Unloaded -= AudioPlayerWindow_Unloaded;
    }

    AudioPlayerViewModel ViewModel => DataContext as AudioPlayerViewModel;

    private void MediaSlider_PositionChanged(object sender, System.TimeSpan e) {
        ViewModel?.SetPosition(e);
    }

    static AudioPlayerWindow currentOpenedWindow;
    public static void ShowForMainInstance() {
        if (currentOpenedWindow != null) {
            currentOpenedWindow.Show();
            currentOpenedWindow.Activate();
            return;
        }

        currentOpenedWindow = new AudioPlayerWindow {
            DataContext = AudioPlayerViewModel.MainInstance,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Position = new PixelPoint(64, 64)
        };
        currentOpenedWindow.Show();
    }

    private void AudioItemSizeChanged(object? sender, SizeChangedEventArgs e) {
        Grid g = sender as Grid;
        TextBlock tb = g.Tag as TextBlock;
        var available = g.Bounds.Width - g.ColumnDefinitions[0].Width.Value - g.ColumnDefinitions[2].Width.Value;
        tb.MaxWidth = available / 10 * 6;
    }

    private void UpdateOrdinalNumber(object? sender, System.EventArgs e) {
        TextBlock tb = sender as TextBlock;
        AudioPlayerItem audio = tb.DataContext as AudioPlayerItem;
        if (audio == null || ViewModel == null) {
            tb.Text = "—";
            return;
        }

        int ordinal = ViewModel.Songs.IndexOf(audio) + 1;
        tb.Text = ordinal.ToString();
    }

    private void SwitchSong(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        Button btn = sender as Button;
        AudioPlayerItem audio = btn.DataContext as AudioPlayerItem;
        if (audio == null || ViewModel == null) return;

        if (ViewModel.Songs.Contains(audio)) ViewModel.CurrentSong = audio;
    }
}
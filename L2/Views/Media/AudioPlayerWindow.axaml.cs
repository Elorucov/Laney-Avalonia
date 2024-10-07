using Avalonia;
using Avalonia.Controls;
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
}
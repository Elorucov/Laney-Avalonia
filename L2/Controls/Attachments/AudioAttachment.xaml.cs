using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels;
using ELOR.VKAPILib.Objects;
using System;
using VKUI.Controls;

namespace ELOR.Laney.Controls.Attachments;

public class AudioAttachment : TemplatedControl {
    #region Properties

    public static readonly StyledProperty<Audio> AudioProperty =
        AvaloniaProperty.Register<AudioAttachment, Audio>(nameof(Audio));

    public Audio Audio {
        get => GetValue(AudioProperty);
        set => SetValue(AudioProperty, value);
    }

    #endregion

    private AudioPlayerViewModel Instance => AudioPlayerViewModel.MainInstance;
    private bool IsThisAudioSelected => Instance != null && Instance.CurrentSong?.Id == Audio?.Id;
    private bool IsThisAudioPlaying => IsThisAudioSelected && Instance.IsPlaying;

    #region Events

    public event EventHandler PlayAudioRequested;

    #endregion

    #region Template elements

    Button PlayButton;
    VKIcon ButtonIcon;
    TextBlock TrackName;
    TextBlock Performer;
    TextBlock Duration;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        PlayButton = e.NameScope.Find<Button>(nameof(PlayButton));
        ButtonIcon = e.NameScope.Find<VKIcon>(nameof(ButtonIcon));
        TrackName = e.NameScope.Find<TextBlock>(nameof(TrackName));
        Performer = e.NameScope.Find<TextBlock>(nameof(Performer));
        Duration = e.NameScope.Find<TextBlock>(nameof(Duration));

        Setup();

        PlayButton.Click += PlayButton_Click;
        if (AudioPlayerViewModel.MainInstance != null) Instance.StateChanged += Instance_StateChanged;
        AudioPlayerViewModel.InstancesChanged += AudioPlayerViewModel_InstancesChanged;
        Unloaded += AudioAttachment_Unloaded;
    }

    #endregion

    private void Setup() {
        if (Audio != null) {
            TrackName.Text = Audio.Title;
            Performer.Text = Audio.Artist;
            Duration.Text = Audio.Duration.ToTimeWithHourIfNeeded();
            if (Audio.Uri != null) {
                PlayButton.IsEnabled = true;
            } else {
                IsHitTestVisible = false;
                Opacity = 0.5;
            }
        } else {
            TrackName.Text = String.Empty;
            Performer.Text = String.Empty;
            Duration.Text = "-:--";
            PlayButton.IsEnabled = false;
            IsHitTestVisible = true;
            Opacity = 1;
        }
    }

    private void CheckCurrentPlayingAudio() {
        ButtonIcon.Id = IsThisAudioPlaying ? VKIconNames.Icon24Pause : VKIconNames.Icon24Play;
        var t = Instance;
        var u = t?.CurrentSong;
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e) {
        if (IsThisAudioSelected) {
            if (Instance.IsPlaying) {
                Instance.Pause();
            } else {
                Instance.Play();
            }
        } else {
            PlayAudioRequested?.Invoke(this, null);
        }
    }

    private void Instance_StateChanged(object sender, bool e) {
        CheckCurrentPlayingAudio();
    }

    private void AudioPlayerViewModel_InstancesChanged(object sender, EventArgs e) {
        if (Instance != null) Instance.StateChanged += Instance_StateChanged;
        CheckCurrentPlayingAudio();
    }

    private void AudioAttachment_Unloaded(object sender, RoutedEventArgs e) {
        PlayButton.Click -= PlayButton_Click;
        if (Instance != null) Instance.StateChanged -= Instance_StateChanged;
        AudioPlayerViewModel.InstancesChanged -= AudioPlayerViewModel_InstancesChanged;
        Unloaded -= AudioAttachment_Unloaded;
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using VKUI.Controls;

namespace ELOR.Laney;

public class AudioMessageAttachment : TemplatedControl {
    #region Properties

    public static readonly StyledProperty<AudioMessage> AudioMessageProperty =
        AvaloniaProperty.Register<AudioMessageAttachment, AudioMessage>(nameof(AudioMessage));

    public AudioMessage AudioMessage {
        get => GetValue(AudioMessageProperty);
        set => SetValue(AudioMessageProperty, value);
    }

    #endregion

    private AudioPlayerViewModel Instance => AudioPlayerViewModel.VoiceMessageInstance;
    private bool IsThisAudioSelected => Instance != null && Instance.CurrentSong?.Id == AudioMessage?.Id;
    private bool IsThisAudioPlaying => IsThisAudioSelected && Instance.IsPlaying;

    #region Events

    public event EventHandler PlayAudioRequested;

    #endregion

    #region Template elements

    Button PlayButton;
    VKIcon ButtonIcon;
    Grid WaveContainer;
    Canvas BackgroundSoundWave;
    Canvas ForegroundSoundWave;
    Border Seeker;
    TextBlock Duration;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        PlayButton = e.NameScope.Find<Button>(nameof(PlayButton));
        ButtonIcon = e.NameScope.Find<VKIcon>(nameof(ButtonIcon));
        WaveContainer = e.NameScope.Find<Grid>(nameof(WaveContainer));
        BackgroundSoundWave = e.NameScope.Find<Canvas>(nameof(BackgroundSoundWave));
        ForegroundSoundWave = e.NameScope.Find<Canvas>(nameof(ForegroundSoundWave));
        Seeker = e.NameScope.Find<Border>(nameof(Seeker));
        Duration = e.NameScope.Find<TextBlock>(nameof(Duration));

        Seeker.PointerPressed += Seeker_PointerPressed;
        Setup();

        PlayButton.Click += PlayButton_Click;
        if (AudioPlayerViewModel.VoiceMessageInstance != null) {
            Instance.StateChanged += Instance_StateChanged;
            Instance.PositionChanged += Instance_PositionChanged;
        }
        AudioPlayerViewModel.InstancesChanged += AudioPlayerViewModel_InstancesChanged;
        Unloaded += AudioMessageAttachment_Unloaded;
    }

    #endregion

    private void Setup() {
        if (AudioMessage != null) {
            wmax = AudioMessage.WaveForm.Max();
            DrawSoundWaveLines(BackgroundSoundWave);
            DrawSoundWaveLines(ForegroundSoundWave);
            Duration.Text = TimeSpan.FromSeconds(AudioMessage.Duration).ToTimeWithHourIfNeeded();
            PlayButton.IsEnabled = true;
        } else {
            BackgroundSoundWave.Children.Clear();
            ForegroundSoundWave.Children.Clear();
            Duration.Text = "-:--";
            PlayButton.IsEnabled = false;
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

    private void Instance_PositionChanged(object sender, TimeSpan e) {
        if (!IsThisAudioSelected) return;
        ChangeWaveClip();
    }

    private void AudioPlayerViewModel_InstancesChanged(object sender, EventArgs e) {
        if (Instance != null) {
            Instance.StateChanged += Instance_StateChanged;
            Instance.PositionChanged += Instance_PositionChanged;
        }
        CheckCurrentPlayingAudio();
    }

    private void AudioMessageAttachment_Unloaded(object sender, RoutedEventArgs e) {
        PlayButton.Click -= PlayButton_Click;
        Seeker.PointerPressed -= Seeker_PointerPressed;
        if (Instance != null) {
            Instance.StateChanged -= Instance_StateChanged;
            Instance.PositionChanged -= Instance_PositionChanged;
        }
        AudioPlayerViewModel.InstancesChanged -= AudioPlayerViewModel_InstancesChanged;
        Unloaded -= AudioMessageAttachment_Unloaded;
    }

    #region Render methods

    int wmax = 0;

    // To be optimized — we do not call GetWaveform twice for each canvas.
    private void DrawSoundWaveLines(Canvas c) {
        c.Children.Clear();

        double x = WaveContainer.Bounds.Width;
        double y = WaveContainer.Bounds.Height;

        if (x > 0 && y > 0) {
            List<int> wave = GetWaveForm(AudioMessage.WaveForm);
            if (wave.Count > 0) {
                for (int i = 0; i < wave.Count; i++) {
                    int num = wave[i];
                    int left = i * 3;
                    double top = (y / 2) - (double)num / 2.0;
                    c.Children.Add(GetWaveformItem(num, left, top));
                }
            }
        } else {
            c.LayoutUpdated += OnCanvasSizeChanged;
        }
    }

    private void OnCanvasSizeChanged(object sender, EventArgs e) {
        Canvas c = sender as Canvas;
        c.LayoutUpdated -= OnCanvasSizeChanged;
        DrawSoundWaveLines(c);
    }

    // Code taken from decompiled version of VK for Windows Phone and used in Laney v1.
    public static List<int> Resample(List<int> source, int targetLength) {
        if (source == null || source.Count == 0 || source.Count == targetLength) {
            return source;
        }
        int[] array = new int[targetLength];
        if (source.Count < targetLength) {
            double num = (double)source.Count / (double)targetLength;
            for (int i = 0; i < targetLength; i++) {
                array[i] = source[(int)((double)i * num)];
            }
        } else {
            double num2 = (double)source.Count / (double)targetLength;
            double num3 = 0.0;
            double num4 = 0.0;
            int i = 0;

            foreach (int current in source) {
                double num5 = Math.Min(num4 + 1.0, num2) - num4;
                num3 += (double)current * num5;
                num4 += num5;
                if (num4 >= num2 - 0.001) {
                    array[i++] = (int)Math.Round(num3 / num2);
                    if (num5 < 1.0) {
                        num4 = 1.0 - num5;
                        num3 = (double)current * num4;
                    } else {
                        num4 = 0.0;
                        num3 = 0.0;
                    }
                }
            }

            if (num3 > 0.0 && i < targetLength) {
                array[i] = (int)Math.Round(num3 / num2);
            }
        }
        return array.ToList();
    }

    private List<int> GetWaveForm(int[] waveform) {
        List<int> list2 = new List<int>();
        List<int> WaveList = waveform.ToList();
        bool isAllEmpty = WaveList.All(l => l == 0);
        if (waveform != null && waveform.Length > 0 && !isAllEmpty) {
            int targetLength = (int)(WaveContainer.Bounds.Width / 3.0);
            double ch = WaveContainer.Bounds.Height;
            List<int> list = Resample(WaveList, targetLength);
            int num = list.Max();
            foreach (int t in list) {
                int num2 = (int)Math.Round(ch * ((double)t * 1.0 / (double)wmax));
                if (num2 < 2) {
                    num2 = 2;
                }
                if (num2 % 2 != 0) {
                    num2++;
                }
                list2.Add(num2);
            }
        }
        return list2;
    }

    private Rectangle GetWaveformItem(int waveformItem, int left, double top) {
        Rectangle rect = new Rectangle {
            Width = 2,
            Height = waveformItem,
            Margin = new Thickness(0, 0, 1, 0)
        };
        Canvas.SetLeft(rect, left);
        Canvas.SetTop(rect, top);
        return rect;
    }

    private void ChangeWaveClip() {
        double w = WaveContainer.Bounds.Width / Instance.CurrentSong.Duration.TotalMilliseconds * Instance.Position.TotalMilliseconds;
        ForegroundSoundWave.Clip = new RectangleGeometry { Rect = new Rect(0, 0, w, WaveContainer.Bounds.Height) };
        Duration.Text = Instance.Position.ToString(@"m\:ss");
    }

    private void Seeker_PointerPressed(object sender, PointerPressedEventArgs e) {
        if (Instance == null || !IsThisAudioSelected) return;
        Border seeker = sender as Border;

        e.Handled = true;
        double x = e.GetCurrentPoint(Seeker).Position.X;
        double w = seeker.Bounds.Width;
        double t = Instance.CurrentSong.Duration.TotalMilliseconds / w * x;
        Instance.SetPosition(TimeSpan.FromMilliseconds(t));
        ChangeWaveClip();
    }

    #endregion
}
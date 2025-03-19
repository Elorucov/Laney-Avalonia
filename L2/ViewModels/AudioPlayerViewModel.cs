using ELOR.Laney.Core;
using ELOR.Laney.DataModels;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ELOR.Laney.ViewModels {
    public class AudioPlayerViewModel : ViewModelBase {
        private string _name;
        private ObservableCollection<AudioPlayerItem> _songs;
        private AudioPlayerItem _currentSong;
        private int _currentSongIndex;
        private TimeSpan _position;
        private bool _repeatOneSong;
        private bool _isPlaying;
        private bool _isTracklistDisplaying;

        private RelayCommand _playPauseCommand;
        private RelayCommand _getPreviousCommand;
        private RelayCommand _getNextCommand;
        private RelayCommand _repeatCommand;
        private RelayCommand _shareCommand;
        private RelayCommand _openTracklistCommand;

        private LMediaPlayer Instance { get; set; }

        public string Name { get { return _name; } set { _name = value; OnPropertyChanged(); } }
        public ObservableCollection<AudioPlayerItem> Songs { get { return _songs; } private set { _songs = value; OnPropertyChanged(); } }
        public AudioPlayerItem CurrentSong { get { return _currentSong; } set { _currentSong = value; OnPropertyChanged(); } }
        public int CurrentSongIndex { get { return _currentSongIndex; } private set { _currentSongIndex = value; OnPropertyChanged(); } }
        public TimeSpan Position { get { return _position; } private set { _position = value; OnPropertyChanged(); PositionChanged?.Invoke(this, value); } }
        public bool RepeatOneSong { get { return _repeatOneSong; } set { _repeatOneSong = value; OnPropertyChanged(); } }
        public bool IsPlaying { get { return _isPlaying; } private set { _isPlaying = value; OnPropertyChanged(); } }
        public bool IsTracklistDisplaying { get { return _isTracklistDisplaying; } private set { _isTracklistDisplaying = value; OnPropertyChanged(); } }

        public RelayCommand PlayPauseCommand { get { return _playPauseCommand; } private set { _playPauseCommand = value; OnPropertyChanged(); } }
        public RelayCommand GetPreviousCommand { get { return _getPreviousCommand; } private set { _getPreviousCommand = value; OnPropertyChanged(); } }
        public RelayCommand GetNextCommand { get { return _getNextCommand; } private set { _getNextCommand = value; OnPropertyChanged(); } }
        public RelayCommand RepeatCommand { get { return _repeatCommand; } private set { _repeatCommand = value; OnPropertyChanged(); } }
        public RelayCommand ShareCommand { get { return _shareCommand; } private set { _shareCommand = value; OnPropertyChanged(); } }
        public RelayCommand OpenTracklistCommand { get { return _openTracklistCommand; } private set { _openTracklistCommand = value; OnPropertyChanged(); } }

        public event EventHandler<TimeSpan> PositionChanged;
        public event EventHandler<bool> StateChanged;
        AudioType Type;

        private AudioPlayerViewModel(List<Audio> songs, Audio currentSong, string name) {
            Log.Information($"APVM type=audio, count={songs.Count}, current={currentSong.Id}");
            Type = AudioType.Audio;
            Songs = new ObservableCollection<AudioPlayerItem>();
            Name = name;
            foreach (var song in songs) {
                if (song.Uri == null) continue;
                AudioPlayerItem api = new AudioPlayerItem(song);
                Songs.Add(api);
                if (song.Id == currentSong.Id) CurrentSong = api;
            }
            Initialize();

            SwitchSong(true);
            PropertyChanged += (a, b) => {
                if (b.PropertyName == nameof(CurrentSong)) SwitchSong();
            };
        }

        private AudioPlayerViewModel(List<Podcast> podcasts, Podcast currentPodcast, string name) {
            Log.Information($"APVM type=podcast, count={podcasts.Count}, current={currentPodcast.Id}");
            Type = AudioType.Podcast;
            Songs = new ObservableCollection<AudioPlayerItem>();
            Name = name;
            podcasts.ForEach(podcast => {
                AudioPlayerItem api = new AudioPlayerItem(podcast);
                Songs.Add(api);
                if (podcast.Id == currentPodcast.Id) CurrentSong = api;
            });
            Initialize();

            SwitchSong();
            PropertyChanged += (a, b) => {
                if (b.PropertyName == nameof(CurrentSong)) SwitchSong();
                if (b.PropertyName == nameof(RepeatOneSong)) Settings.AudioPlayerLoop = RepeatOneSong;
            };
        }

        private AudioPlayerViewModel(List<AudioMessage> messages, AudioMessage currentMessage, string ownerName) {
            Log.Information($"APVM type=voicemessage, count={messages.Count}, current={currentMessage.Id}");
            Type = AudioType.VoiceMessage;
            Songs = new ObservableCollection<AudioPlayerItem>();
            Name = ownerName;
            messages.ForEach(message => {
                AudioPlayerItem api = new AudioPlayerItem(message, ownerName);
                Songs.Add(api);
                if (message == currentMessage) CurrentSong = api;
            });
            Initialize();

            SwitchSong();
            PropertyChanged += (a, b) => {
                if (b.PropertyName == nameof(CurrentSong)) SwitchSong();
                if (b.PropertyName == nameof(RepeatOneSong)) Settings.AudioPlayerLoop = RepeatOneSong;
            };
        }

        private void Initialize() {
            Instance = new LMediaPlayer($"Audioplayer type: {Type}");
            RepeatOneSong = Type != AudioType.VoiceMessage ? Settings.AudioPlayerLoop : false;
            Log.Information($"APVM initialized. Repeat={RepeatOneSong}");

            Instance.MediaEnded += Player_MediaEnded;
            Instance.PositionChanged += Instance_PositionChanged;
            Instance.StateChanged += Instance_StateChanged;
            PlayPauseCommand = new RelayCommand(o => {
                if (Instance.IsPlaying) {
                    Pause();
                } else {
                    Play();
                }
            });
            GetPreviousCommand = new RelayCommand(o => PlayPrevious());
            GetNextCommand = new RelayCommand(o => PlayNext());
            RepeatCommand = new RelayCommand(o => {
                RepeatOneSong = !RepeatOneSong;
                Settings.AudioPlayerLoop = RepeatOneSong;
            });
            ShareCommand = new RelayCommand(o => { });
            OpenTracklistCommand = new RelayCommand(o => {
                IsTracklistDisplaying = !IsTracklistDisplaying;
            });
        }

        private void Instance_StateChanged(object sender, bool e) {
            IsPlaying = Instance.IsPlaying;
            StateChanged?.Invoke(this, e);
        }

        private void Instance_PositionChanged(object sender, float e) {
            if (CurrentSong == null) return;
            var millis = (float)CurrentSong.Duration.TotalMilliseconds / 1f * e;
            Position = TimeSpan.FromMilliseconds(millis);
        }

        private void Player_MediaEnded(object sender, EventArgs e) {
            if (Type == AudioType.Audio && !RepeatOneSong) PlayNext();
        }

        private void Uninitialize() {
            Log.Information($"APVM uninitialized. Type={Type}");
            Instance.MediaEnded -= Player_MediaEnded;
            Instance.PositionChanged -= Instance_PositionChanged;
            Instance.StateChanged -= Instance_StateChanged;
            Instance.Dispose();
        }

        private void SwitchSong(bool timeout = false) {
            if (CurrentSong == null) return;

            // Pause();
            Position = TimeSpan.FromMilliseconds(0);
            CurrentSongIndex = Songs.IndexOf(CurrentSong) + 1;
            Log.Information($"APVM changing audio to {CurrentSongIndex}.");

            if (CurrentSong.Source != null) Instance.PlayURL(CurrentSong.Source);
        }

        #region Controls

        public void SetPosition(TimeSpan position) {
            if (CurrentSong == null) return;
            Instance.SetPosition(position);
            Position = position;
        }

        public void Play() {
            if (Type != AudioType.VoiceMessage && VoiceMessageInstance != null) CloseVoiceMessageInstance();
            Instance.Play();
        }

        public void Pause() {
            Instance.Pause();
        }

        public void PlayNext() {
            int i = Songs.IndexOf(CurrentSong);
            if (i >= Songs.Count - 1) {
                CurrentSong = Songs[0];
            } else {
                CurrentSong = Songs[i + 1];
            }
        }

        public void PlayPrevious() {
            int i = Songs.IndexOf(CurrentSong);
            if (i <= 0) {
                CurrentSong = Songs[Songs.Count - 1];
            } else {
                CurrentSong = Songs[i - 1];
            }
        }

        #endregion

        #region Static members

        public static AudioPlayerViewModel MainInstance { get; private set; }
        public static AudioPlayerViewModel VoiceMessageInstance { get; private set; }

        public static event EventHandler InstancesChanged;

        public static void PlaySong(List<Audio> songs, Audio selectedSong, string name) {
            if (selectedSong.Uri == null || !LMediaPlayer.IsInitialized) return;

            CloseVoiceMessageInstance();
            MainInstance?.Uninitialize();
            MainInstance = new AudioPlayerViewModel(songs, selectedSong, name);
            InstancesChanged?.Invoke(null, null);
        }

        public static void PlayPodcast(List<Podcast> podcasts, Podcast selectedPodcast, string name) {
            if (selectedPodcast.Uri == null || !LMediaPlayer.IsInitialized) return;

            CloseVoiceMessageInstance();
            MainInstance?.Uninitialize();
            MainInstance = new AudioPlayerViewModel(podcasts, selectedPodcast, name);
            InstancesChanged?.Invoke(null, null);
        }

        public static void PlayVoiceMessage(List<AudioMessage> messages, AudioMessage selectedMessage, string ownerName) {
            if (selectedMessage.Uri == null || !LMediaPlayer.IsInitialized) return;

            VoiceMessageInstance?.Uninitialize();
            if (MainInstance != null) {
                MainInstance.Pause();
            }
            VoiceMessageInstance = new AudioPlayerViewModel(messages, selectedMessage, ownerName);
            InstancesChanged?.Invoke(null, null);
        }

        public static void CloseMainInstance() {
            if (MainInstance != null) {
                MainInstance.Uninitialize();
                MainInstance = null;
                InstancesChanged?.Invoke(null, null);
            }
        }

        public static void CloseVoiceMessageInstance() {
            if (VoiceMessageInstance != null) {
                VoiceMessageInstance.Uninitialize();
                VoiceMessageInstance = null;
                InstancesChanged?.Invoke(null, null);
            }
        }
        #endregion
    }
}
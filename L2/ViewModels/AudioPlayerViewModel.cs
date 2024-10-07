using Avalonia.Threading;
using ELOR.Laney.Core;
using ELOR.Laney.DataModels;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;
using ManagedBass;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ELOR.Laney.ViewModels {
    public class AudioPlayerViewModel : ViewModelBase {
        private string _name;
        private ObservableCollection<AudioPlayerItem> _songs;
        private AudioPlayerItem _currentSong;
        private int _currentSongIndex;
        private TimeSpan _position;
        private PlaybackState _playbackState;
        private bool _isPlaying;

        private RelayCommand _playPauseCommand;
        private RelayCommand _getPreviousCommand;
        private RelayCommand _getNextCommand;
        private RelayCommand _repeatCommand;
        private RelayCommand _shareCommand;

        public string Name { get { return _name; } set { _name = value; OnPropertyChanged(); } }
        public ObservableCollection<AudioPlayerItem> Songs { get { return _songs; } private set { _songs = value; OnPropertyChanged(); } }
        public AudioPlayerItem CurrentSong { get { return _currentSong; } set { _currentSong = value; OnPropertyChanged(); } }
        public int CurrentSongIndex { get { return _currentSongIndex; } private set { _currentSongIndex = value; OnPropertyChanged(); } }
        public TimeSpan Position { get { return _position; } private set { _position = value; OnPropertyChanged(); } }
        public PlaybackState PlaybackState { get { return _playbackState; } private set { _playbackState = value; OnPropertyChanged(); } }
        public bool RepeatOneSong { get { return Player.Loop; } set { Player.Loop = value; OnPropertyChanged(); } }
        public bool IsPlaying { get { return _isPlaying; } private set { _isPlaying = value; OnPropertyChanged(); } }

        public RelayCommand PlayPauseCommand { get { return _playPauseCommand; } private set { _playPauseCommand = value; OnPropertyChanged(); } }
        public RelayCommand GetPreviousCommand { get { return _getPreviousCommand; } private set { _getPreviousCommand = value; OnPropertyChanged(); } }
        public RelayCommand GetNextCommand { get { return _getNextCommand; } private set { _getNextCommand = value; OnPropertyChanged(); } }
        public RelayCommand RepeatCommand { get { return _repeatCommand; } private set { _repeatCommand = value; OnPropertyChanged(); } }
        public RelayCommand ShareCommand { get { return _shareCommand; } private set { _shareCommand = value; OnPropertyChanged(); } }

        public event EventHandler<PlaybackState> PlaybackStateChanged;
        AudioType Type;

        public Guid Id { get; private set; }

        #region Private fields

        private UrlMediaPlayer Player;

        #endregion

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
                if (b.PropertyName == nameof(PlaybackState)) {
                    PlaybackStateChanged?.Invoke(this, PlaybackState);
                };
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
                if (b.PropertyName == nameof(PlaybackState)) {
                    PlaybackStateChanged?.Invoke(this, PlaybackState);
                };
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

        DispatcherTimer positionTimer;

        private void Initialize() {
            Player = new UrlMediaPlayer();
            Player.PropertyChanged += Player_PropertyChanged;
            Player.MediaLoaded += Player_MediaLoaded;
            Player.MediaEnded += Player_MediaEnded;
            RepeatOneSong = Type != AudioType.VoiceMessage ? Settings.AudioPlayerLoop : false;
            Log.Information($"APVM initialized. Repeat={RepeatOneSong}");

            if (positionTimer != null) {
                positionTimer.Stop();
                positionTimer = null;
            }
            positionTimer = new DispatcherTimer { 
                Interval = TimeSpan.FromMilliseconds(125)
            };
            positionTimer.Tick += PositionTimer_Tick;

            PlayPauseCommand = new RelayCommand(o => {
                switch (PlaybackState) {
                    case PlaybackState.Playing: Pause(); break;
                    case PlaybackState.Paused: Play(); break;
                }
            });
            GetPreviousCommand = new RelayCommand(o => PlayPrevious());
            GetNextCommand = new RelayCommand(o => PlayNext());
            RepeatCommand = new RelayCommand(o => {
                RepeatOneSong = !RepeatOneSong;
                Settings.AudioPlayerLoop = RepeatOneSong;
            });
            ShareCommand = new RelayCommand(o => { });
        }

        private void PositionTimer_Tick(object sender, EventArgs e) {
            if (Player != null) Position = Player.Position;
        }

        private void Player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            Debug.WriteLine($"APVM: Player's \"{e.PropertyName}\" prop changed.");
            if (e.PropertyName == nameof(MediaPlayer.State)) {
                CheckPlaybackState();
            //} else if (e.PropertyName == nameof(MediaPlayer.Position)) {
            //    Debug.WriteLine($"APVM: Player's \"{e.PropertyName}\" prop changed to {Player.Position}");
            //    Position = Player.Position;
            }
        }

        private void Player_MediaLoaded(int obj) {
            Debug.WriteLine($"APVM: Media loaded.");
            CheckPlaybackState();
        }

        private void Player_MediaEnded(object sender, EventArgs e) {
            if (Type == AudioType.Audio && !RepeatOneSong) PlayNext();
        }

        private void Uninitialize() {
            Log.Information($"APVM uninitialized. Type={Type}");
            Player.PropertyChanged -= Player_PropertyChanged;
            Player.MediaEnded -= Player_MediaEnded;
            Player.Dispose();
            Player = null;
        }

        private void CheckPlaybackState() {
            PlaybackState = Player.State;
            Debug.WriteLine($"APVM: State = {PlaybackState}");
            switch (PlaybackState) {
                case PlaybackState.Playing: IsPlaying = true; break;
                case PlaybackState.Stopped:
                case PlaybackState.Paused: IsPlaying = false; break;
                case PlaybackState.Stalled: Player.Pause(); Player.Play(); break; // because after first init; BASS starts play audio and incorrectly reports "Stalled" state instead "Playing".
            }
        }

        private async void SwitchSong(bool timeout = false) {
            if (CurrentSong == null) return;

            Pause();
            Position = TimeSpan.FromMilliseconds(0);
            CurrentSongIndex = Songs.IndexOf(CurrentSong) + 1;
            Log.Information($"APVM changing audio to {CurrentSongIndex}.");

            bool result = await Player.LoadAsync(CurrentSong.Source.AbsoluteUri);
            if (!result) {
                var error = Bass.LastError;
                Log.Error($"APVM error! Type={Type}, error={error}");
                return;
            }
            if (timeout) await Task.Delay(100);
            Play();
        }

        #region Controls

        public void SetPosition(TimeSpan position) {
            Player.Position = position;
            Position = position;
        }

        public void Play() {
            if (Type != AudioType.VoiceMessage && VoiceMessageInstance != null) CloseVoiceMessageInstance();
            Player.Play();
            positionTimer?.Start();
        }

        public void Pause() {
            positionTimer.Stop();
            Player.Pause();
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
            CloseVoiceMessageInstance();
            MainInstance?.Uninitialize();
            MainInstance = new AudioPlayerViewModel(songs, selectedSong, name);
            InstancesChanged?.Invoke(null, null);
        }

        public static void PlayPodcast(List<Podcast> podcasts, Podcast currentPodcast, string name) {
            CloseVoiceMessageInstance();
            MainInstance?.Uninitialize();
            MainInstance = new AudioPlayerViewModel(podcasts, currentPodcast, name);
            InstancesChanged?.Invoke(null, null);
        }

        public static void PlayVoiceMessage(List<AudioMessage> messages, AudioMessage selectedMessage, string ownerName) {
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
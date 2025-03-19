using Avalonia.Threading;
using LibVLCSharp.Shared;
using Serilog;
using System;
using System.IO;

namespace ELOR.Laney.Core {
    public class LMediaPlayer {
        #region Static instances

        public static bool IsInitialized { get; private set; }
        private static LibVLC VLC;

        public static string LibVersion => VLC?.Version;
        public static string InitializationErrorReason { get; private set; }
        public static LMediaPlayer SFX { get; private set; }

        public static void InitStaticInstances() {
            try {
                SFX = new LMediaPlayer(nameof(SFX));
            } catch (VLCException vlcex) {
                SFX = null;
                InitializationErrorReason = vlcex.ToString();
                Log.Error(vlcex, $"LMediaPlayer could not be initialized! This is an error from libVLC side.");
            } catch (Exception ex) {
                SFX = null;
                Log.Error(ex, $"LMediaPlayer could not be initialized!");
            }
        }

        #endregion

        private Media _media;
        private MediaPlayer _player;

        public string InstanceName { get; private set; }
        public float Position { get; private set; }
        public bool IsPlaying { get; private set; }

        #region Events

        public event EventHandler MediaEnded;
        public event EventHandler<float> PositionChanged;
        public event EventHandler<bool> StateChanged;

        #endregion

        private static void VLC_Log(object sender, LogEventArgs e) {
            switch (e.Level) {
                case LogLevel.Error:
                    Log.Error($"libVLC: {e.Message}");
                    break;
                case LogLevel.Warning:
                    Log.Warning($"libVLC: {e.Message}");
                    break;
                case LogLevel.Notice:
                    Log.Information($"libVLC: {e.Message}");
                    break;
                case LogLevel.Debug:
                    Log.Verbose($"libVLC: {e.Message}");
                    break;
            }
        }

        public LMediaPlayer(string name) {
            InstanceName = name;
            if (IsInitialized) return;

            if (VLC == null) {
                VLC = new LibVLC(); // init VLC first time
                VLC.Log += VLC_Log;
            }

            IsInitialized = true;
        }

        public void PlayStream(Stream stream) {
            Media old = _player?.Media;

            _media = new Media(VLC, new StreamMediaInput(stream));
            if (_player == null) {
                _player = new MediaPlayer(_media);

                _player.PositionChanged += Player_PositionChanged;
                _player.Playing += Player_Playing;
                _player.Paused += Player_Paused;
                _player.EndReached += Player_EndReached;
            } else {
                _player.Media = _media;
            }

            if (old != null) _media.Dispose();

            _player.Play();
        }

        public void PlayURL(string url) {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) return;
            PlayURL(new Uri(url));
        }

        public void PlayURL(Uri uri) {
            Media old = _player?.Media;

            _media = new Media(VLC, uri);
            if (_player == null) {
                _player = new MediaPlayer(_media);

                _player.PositionChanged += Player_PositionChanged;
                _player.Playing += Player_Playing;
                _player.Paused += Player_Paused;
                _player.EndReached += Player_EndReached;
            } else {
                _player.Media = _media;
            }

            if (old != null) _media.Dispose();

            _player.Play();
        }

        public void Play() {
            if (_player != null && !_player.IsPlaying) _player.Play();
        }

        public void Pause() {
            if (_player != null && _player.IsPlaying) _player.Pause();
        }

        public void SetPosition(TimeSpan position) {
            _player.SeekTo(position);
        }

        public void Dispose() {
            _media?.Dispose();
            _player?.Dispose();
        }

        #region Library Events

        private void Player_PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e) {
            new Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    Position = e.Position;
                    PositionChanged?.Invoke(this, e.Position);
                });
            })();
        }

        private void Player_Playing(object sender, EventArgs e) {
            new Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    IsPlaying = true;
                    StateChanged?.Invoke(this, true);
                });
            })();
        }

        private void Player_Paused(object sender, EventArgs e) {
            new Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    IsPlaying = false;
                    StateChanged?.Invoke(this, false);
                });
            })();
        }

        private void Player_EndReached(object sender, EventArgs e) {
            new Action(async () => {
                await Dispatcher.UIThread.InvokeAsync(() => {
                    MediaEnded?.Invoke(this, e);
                });
            })();
        }

        #endregion
    }
}
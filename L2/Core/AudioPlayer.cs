using ManagedBass;
using Serilog;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ELOR.Laney.Core {
    public class AudioPlayer {
        #region Static instances

        public static bool IsInitialized { get; private set; }
        public static AudioPlayer SFX { get; private set; }

        public static void InitInstances() {
            try {
                SFX = new AudioPlayer("SFX");
            } catch (Exception ex) {
                SFX = null;
                Log.Error(ex, $"AudioPlayer could not be initialized!");
            }
        }

        #endregion

        public string InstanceName { get; private set; }

        public AudioPlayer(string name) {
            InstanceName = name;
            if (IsInitialized) return;
            if (!Bass.Init()) {
                string forName = String.IsNullOrEmpty(name) ? String.Empty : $" (instance: {name})";
                Log.Error($"AudioPlayer: BASS could not be initialized!" + forName);
                return;
            }
            IsInitialized = true;
        }

        public void Play(Stream stream) {
            var length = stream.Length;
            byte[] b = new byte[stream.Length];
            stream.Read(b);
            stream.Dispose();

            var handle = GCHandle.Alloc(b, GCHandleType.Pinned);
            var bstream = Bass.CreateStream(handle.AddrOfPinnedObject(), 0, length, BassFlags.Float | BassFlags.AutoFree);
            if (bstream == 0 || !Bass.ChannelPlay(bstream, false)) {
                Log.Error($"AudioPlayer.Play: Cannot play a stream! Last error code from BASS: {Bass.LastError}");
            }
        }

        public void PlayURL(string url) {
            var bstream = Bass.CreateStream(url, 0, BassFlags.Float | BassFlags.AutoFree, null);
            if (bstream == 0 || !Bass.ChannelPlay(bstream, false)) {
                Log.Error($"AudioPlayer.Play: Cannot play a stream! Last error code from BASS: {Bass.LastError}");
            }
        }
    }
}
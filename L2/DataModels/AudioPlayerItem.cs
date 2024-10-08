using ELOR.VKAPILib.Objects;
using System;
using VKUI.Controls;

namespace ELOR.Laney.DataModels {
    public enum AudioType {
        Audio, Podcast, VoiceMessage
    }

    public class AudioPlayerItem {
        public AudioType Type { get; private set; }
        public long Id { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Performer { get; private set; }
        public TimeSpan Duration { get; private set; }
        public Uri Source { get; private set; }
        public string CoverPlaceholderIconId { get; private set; }
        public Uri CoverUrl { get; private set; }
        public AttachmentBase Attachment { get; private set; }

        public AudioPlayerItem(Audio audio) {
            Attachment = audio;
            Type = AudioType.Audio;
            Id = audio.Id;
            Title = audio.Title;
            Subtitle = audio.Subtitle;
            // if (!String.IsNullOrEmpty(audio.Subtitle)) Title += $" ({audio.Subtitle})";
            Performer = audio.Artist;
            Duration = TimeSpan.FromSeconds(audio.Duration);
            Source = audio.Uri;
            CoverPlaceholderIconId = VKIconNames.Icon28SongOutline;
        }

        public AudioPlayerItem(Podcast podcast) {
            Attachment = podcast;
            Type = AudioType.Podcast;
            Id = podcast.Id;
            Title = podcast.Title;
            Performer = podcast.Artist;
            Duration = TimeSpan.FromSeconds(podcast.Duration);
            Source = podcast.Uri;
            CoverPlaceholderIconId = VKIconNames.Icon28PodcastOutline;
            CoverUrl = podcast.Info.Cover?.Sizes[0].Uri;
        }

        public AudioPlayerItem(AudioMessage audioMessage, string ownerName) {
            Attachment = audioMessage;
            Type = AudioType.VoiceMessage;
            Id = audioMessage.Id;
            Title = Assets.i18n.Resources.audio_message;
            Performer = ownerName;
            Duration = TimeSpan.FromSeconds(audioMessage.Duration);
            Source = audioMessage.Uri;
            CoverPlaceholderIconId = VKIconNames.Icon28VoiceOutline;
        }

        public override string ToString() {
            return Type == AudioType.VoiceMessage ? Performer : $"{Performer} — {Title}";
        }
    }
}

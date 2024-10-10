using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels;
using ELOR.Laney.Views.Media;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VKUI.Controls;
using VKUI.Utils;

namespace ELOR.Laney.Controls.Attachments {
    public class AttachmentsContainer : TemplatedControl {
        #region Properties

        public static readonly StyledProperty<List<Attachment>> AttachmentsProperty =
            AvaloniaProperty.Register<AttachmentsContainer, List<Attachment>>(nameof(Attachments));

        public List<Attachment> Attachments {
            get => GetValue(AttachmentsProperty);
            set => SetValue(AttachmentsProperty, value);
        }

        public static readonly StyledProperty<Gift> GiftProperty =
            AvaloniaProperty.Register<AttachmentsContainer, Gift>(nameof(Gift));

        public Gift Gift {
            get => GetValue(GiftProperty);
            set => SetValue(GiftProperty, value);
        }

        public bool NoMargins { get; set; } = false; // отступы по бокам. true нужно для PostUI.
        public bool? IsOutgoing { get; set; } = null; // нужно для MessageBubble. В других случаях null.
        public string Owner { get; set; } = String.Empty; // имя владельца контента. Обычно это отправитель сообщения либо автор записи с данными вложениями. Нужно для аудиопроигрывателя.
        public bool NeedToShowStoryPreview { get; set; } = false;

        #endregion

        #region Template elements

        StackPanel StandartAttachments;

        bool isUILoaded = false;
        VKSession session;
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            StandartAttachments = e.NameScope.Find<StackPanel>(nameof(StandartAttachments));
            isUILoaded = true;
            RenderAttachments();
        }

        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);
            session = VKSession.GetByDataContext(Parent as Control);
        }

        #endregion

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);

            if (change.Property == AttachmentsProperty) {
                RenderAttachments();
            }
        }

        private void RenderAttachments() {
            if (!isUILoaded || Attachments == null) return;
            StandartAttachments.Margin = NoMargins ? new Thickness(0) : new Thickness(8, 0, 8, 0);

            double imageFixedWidth = MessageBubble.BUBBLE_FIXED_WIDTH - 4;
            if (!Double.IsNaN(Width)) imageFixedWidth = Width;

            Sticker sticker = null;
            UGCSticker ugcs = null;
            Gift gift = null;
            Graffiti graffiti = null;
            List<IPreview> previews = new List<IPreview>();
            WallPost wp = null;
            WallReply wr = null;
            List<Link> links = new List<Link>();
            Market market = null;
            Poll poll = null;
            Call call = null;
            GroupCallInProgress gcall = null;
            Event evt = null;
            List<Story> stories = new List<Story>();
            Narrative nr = null;
            Curator cur = null;
            List<Document> docs = new List<Document>();
            List<Audio> audios = new List<Audio>();
            List<Podcast> podcasts = new List<Podcast>();
            List<AudioMessage> ams = new List<AudioMessage>();
            // WikiPage page = null;
            // Note note = null;
            // Album album = null;
            // SituationalTheme sth = null;
            // Textlive tl = null;
            TextpostPublish tpb = null;
            List<Attachment> unknown = new List<Attachment>();

            foreach (Attachment a in CollectionsMarshal.AsSpan(Attachments)) {
                switch (a.Type) {
                    case AttachmentType.Sticker: sticker = a.Sticker; break;
                    case AttachmentType.UGCSticker: ugcs = a.UGCSticker; break;
                    case AttachmentType.Graffiti: graffiti = a.Graffiti; break;
                    case AttachmentType.Gift: gift = a.Gift; break;
                    case AttachmentType.Photo: previews.Add(a.Photo); break;
                    case AttachmentType.Video: previews.Add(a.Video); break;
                    case AttachmentType.Audio: audios.Add(a.Audio); break;
                    case AttachmentType.Podcast: podcasts.Add(a.Podcast); break;
                    case AttachmentType.Curator: cur = a.Curator; break;
                    case AttachmentType.Wall: wp = a.Wall; break;
                    case AttachmentType.WallReply: wr = a.WallReply; break;
                    case AttachmentType.Link: links.Add(a.Link); break;
                    case AttachmentType.Market: market = a.Market; break;
                    case AttachmentType.Poll: poll = a.Poll; break;
                    case AttachmentType.AudioMessage: ams.Add(a.AudioMessage); break;
                    case AttachmentType.Call: call = a.Call; break;
                    case AttachmentType.GroupCallInProgress: gcall = a.GroupCallInProgress; break;
                    case AttachmentType.Event: evt = a.Event; break;
                    case AttachmentType.Story: stories.Add(a.Story); break;
                    case AttachmentType.Narrative: nr = a.Narrative; break;
                    case AttachmentType.Document: if (a.Document.Preview != null) { previews.Add(a.Document); } else { docs.Add(a.Document); }; break;
                    // case AttachmentType.Page: page = a.Page; break;
                    // case AttachmentType.Note: note = a.Note; break;
                    // case AttachmentType.Album: album = a.Album; break;
                    // case AttachmentType.PrettyCards: break; // чтобы сниппет "unknown attachment" не добавлялся
                    // case AttachmentType.SituationalTheme: sth = a.SituationalTheme; break;
                    // case AttachmentType.Textlive: tl = a.Textlive; break;
                    case AttachmentType.TextpostPublish: tpb = a.TextpostPublish; break;
                    default: unknown.Add(a); break;
                }
            }

            StandartAttachments.Children.Clear();

            // Images
            if (previews.Count == 1) {
                var size = previews[0].GetOriginalSize();
                double w = imageFixedWidth;
                double h = size.Height == 0 ? w : Math.Min(w / size.Width * size.Height, w / 9 * 16);

                var preview = previews[0].GetSizeAndUriForThumbnail(w, h);
                var uri = preview.Uri;

                Button imgBtn = new Button {
                    Tag = new Tuple<List<IPreview>, IPreview>(previews, previews[0]),
                    Background = App.GetResource<SolidColorBrush>("VKBackgroundHoverBrush"),
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                    Padding = new Thickness(0),
                    CornerRadius = new CornerRadius(NoMargins ? 4 : 16),
                    Width = w,
                    Height = h,
                    Margin = NoMargins ? new Thickness(0, 0, 0, 2) : new Thickness(-6, 0, -6, 2)
                };
                AddPreviewInfo(previews[0], imgBtn);
                // if (uri != null) _ = imgBtn.SetImageBackgroundAsync(uri, Width, Height);
                if (uri != null) ImageLoader.SetBackgroundSource(imgBtn, uri);
                imgBtn.Click += ImgBtn_Click;
                StandartAttachments.Children.Add(imgBtn);
            } else if (previews.Count > 1) {
                List<Size> sizes = new List<Size>();
                foreach (IPreview preview in CollectionsMarshal.AsSpan(previews)) {
                    var prevsize = preview.GetOriginalSize();
                    sizes.Add(new Size(prevsize.Width, prevsize.Height));
                }

                var layout = PhotoLayout.Create(new Size(imageFixedWidth, imageFixedWidth), sizes, 2);
                List<Rect> thumbRects = layout.Item1;
                Size layoutSize = layout.Item2;
                List<bool[]> corners = layout.Item3; 
                // top left, top right, bottom right, bottom left
                // corners появился из-за того, что в авалонии
                // контент не обрезается под скруглённым родителем.
                // https://github.com/AvaloniaUI/Avalonia/issues/2105

                Canvas canvas = new Canvas {
                    Width = layoutSize.Width,
                    Height = layoutSize.Height,
                    Margin = NoMargins ? new Thickness(0, 0, 0, 2) : new Thickness(-6, 0, -6, 2),
                    ClipToBounds = true,
                };

                int i = 0;
                foreach (IPreview preview in CollectionsMarshal.AsSpan(previews)) {
                    Rect rect = thumbRects[i];
                    var p = preview.GetSizeAndUriForThumbnail(rect.Width, rect.Height);
                    bool[] corner = corners[i];

                    double tl = !NoMargins && corner[0] ? 16 : 4;
                    double tr = !NoMargins && corner[1] ? 16 : 4;
                    double br = !NoMargins && corner[2] ? 16 : 4;
                    double bl = !NoMargins && corner[3] ? 16 : 4;

                    Button imgBtn = new Button {
                        Tag = new Tuple<List<IPreview>, IPreview>(previews, preview),
                        Background = App.GetResource<SolidColorBrush>("VKBackgroundHoverBrush"),
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Stretch,
                        Padding = new Thickness(0),
                        CornerRadius = new CornerRadius(tl, tr, br, bl),
                        Width = rect.Width,
                        Height = rect.Height
                    };
                    Canvas.SetLeft(imgBtn, rect.Left);
                    Canvas.SetTop(imgBtn, rect.Top);
                    AddPreviewInfo(preview, imgBtn);
                    if (p.Uri != null) _ = imgBtn.SetImageBackgroundAsync(p.Uri, rect.Width, rect.Height);
                    imgBtn.Click += ImgBtn_Click;
                    canvas.Children.Add(imgBtn);
                    i++;
                }
                StandartAttachments.Children.Add(canvas);
            }

            // Sticker
            if (sticker != null) {
                StickerPresenter sp = new StickerPresenter() {
                    Width = MessageBubble.STICKER_WIDTH,
                    Height = MessageBubble.STICKER_WIDTH,
                    Margin = new Thickness(0, 0, 0, 8),
                    HorizontalAlignment = IsOutgoing.HasValue && IsOutgoing.Value ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                    Sticker = sticker
                };
                StandartAttachments.Children.Add(sp);
            }

            // UGC Sticker
            if (ugcs != null) {
                UGCStickerPresenter sp = new UGCStickerPresenter() {
                    Width = MessageBubble.STICKER_WIDTH,
                    Height = MessageBubble.STICKER_WIDTH,
                    Margin = new Thickness(0, 0, 0, 8),
                    HorizontalAlignment = IsOutgoing.HasValue && IsOutgoing.Value ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                    Sticker = ugcs
                };
                StandartAttachments.Children.Add(sp);
            }

            // Graffiti
            if (graffiti != null) {
                Rectangle grImage = new Rectangle() {
                    Width = imageFixedWidth,
                    Height = imageFixedWidth / graffiti.Width * graffiti.Height,
                    Margin = NoMargins ? new Thickness(0, 0, 0, 4) : new Thickness(-4, 0, -4, 4),
                    RadiusX = 14, RadiusY = 14,
                    Name = graffiti.ObjectType
                };
                grImage.SetImageFillAsync(graffiti.Uri, grImage.Width, grImage.Height);
                StandartAttachments.Children.Add(grImage);
            }

            // Wall post
            if (wp != null) {
                string def = VKAPIHelper.GetNameOrDefaultString(wp.OwnerOrToId, TextParser.GetParsedText(wp.Text));
                BasicAttachment ba = new BasicAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Icon = VKIconNames.Icon24ArticleOutline,
                    Title = Assets.i18n.Resources.wall,
                    Subtitle = def,
                    Name = wp.ObjectType
                };
                ba.Click += async (a, b) => await Launcher.LaunchUrl($"https://vk.com/wall{wp.OwnerId}_{wp.Id}");
                StandartAttachments.Children.Add(ba);
            }

            // Wall reply
            if (wr != null) {
                string def = VKAPIHelper.GetNameOrDefaultString(wr.OwnerId, TextParser.GetParsedText(wr.Text));
                BasicAttachment ba = new BasicAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Icon = VKIconNames.Icon24CommentOutline,
                    Title = Assets.i18n.Resources.wall_reply,
                    Subtitle = def,
                    Name = "WallReply"
                };
                ba.Click += async (a, b) => await Launcher.LaunchUrl($"https://vk.com/wall{wr.OwnerId}_{wr.PostId}?reply={wr.Id}");
                StandartAttachments.Children.Add(ba);
            }

            // Link
            foreach (Link link in CollectionsMarshal.AsSpan<Link>(links)) {
                if (link.Button != null || link.Photo != null) {
                    ExtendedAttachment ea = new ExtendedAttachment {
                        Margin = new Thickness(0, 0, 0, 8),
                        Title = link.Title,
                        Subtitle = link.Caption,
                        Name = "Link"
                    };
                    if (link.Button != null) {
                        ea.ActionButtonText = link.Button.Title;
                        ea.ActionButtonClick += async (a, b) => await Launcher.LaunchUrl(link.Button.Action.Url);
                    }
                    if (!String.IsNullOrEmpty(link.Description)) ToolTip.SetTip(ea, link.Description);
                    if (link.Photo != null) ea.Preview = link.Photo.GetSizeAndUriForThumbnail(Constants.ExtendedAttachmentPreviewSize, Constants.ExtendedAttachmentPreviewSize).Uri;
                    ea.Click += async (a, b) => await Launcher.LaunchUrl(link.Url);
                    StandartAttachments.Children.Add(ea);
                } else {
                    BasicAttachment ba = new BasicAttachment {
                        Margin = new Thickness(0, 0, 0, 8),
                        Icon = VKIconNames.Icon24Link,
                        Title = link.Title,
                        Subtitle = link.Caption,
                        Name = "Link"
                    };
                    if (!String.IsNullOrEmpty(link.Description)) ToolTip.SetTip(ba, link.Description);
                    ba.Click += async (a, b) => await Launcher.LaunchUrl(link.Url);
                    StandartAttachments.Children.Add(ba);
                }
            }

            // Market
            if (market != null) {
                string link = $"https://vk.com/product{market.OwnerId}_{market.Id}";

                ExtendedAttachment ea = new ExtendedAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Title = market.Title,
                    Subtitle = market.Price.Text,
                    Preview = new Uri(market.ThumbPhoto),
                    ActionButtonText = Assets.i18n.Resources.open,
                    Name = market.ObjectType
                };
                if (!String.IsNullOrEmpty(market.Description)) ToolTip.SetTip(ea, market.Description);
                ea.ActionButtonClick += async (a, b) => await Launcher.LaunchUrl(link);
                ea.Click += async (a, b) => await Launcher.LaunchUrl(link);
                StandartAttachments.Children.Add(ea);
            }

            // Poll
            if (poll != null) {
                string def = VKAPIHelper.GetNameOrDefaultString(poll.AuthorId);
                BasicAttachment ba = new BasicAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Icon = VKIconNames.Icon24Poll,
                    Title = poll.Question,
                    Subtitle = $"{Assets.i18n.Resources.poll} {def}",
                    Name = poll.ObjectType,
                };
                ba.Click += async (a, b) => await Launcher.LaunchUrl($"https://vk.com/poll{poll.OwnerId}_{poll.Id}");
                StandartAttachments.Children.Add(ba);
            }

            // Call
            if (call != null) StandartAttachments.Children.Add(GetCallInfoControl(call));

            // Group call in progress
            if (gcall != null) StandartAttachments.Children.Add(GetCallInfoControl(gcall));

            // Event
            if (evt != null) {
                Group eg = CacheManager.GetGroup(evt.Id);
                string link = $"https://vk.com/club{evt.Id}";

                ExtendedAttachment ea = new ExtendedAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Title = eg.Name,
                    Subtitle = String.IsNullOrEmpty(evt.Address) ? evt.Text : evt.Address, // лучше дату
                    Preview = eg.Photo,
                    ActionButtonText = evt.ButtonText,
                    Name = "Event"
                };
                if (!String.IsNullOrEmpty(evt.Text)) ToolTip.SetTip(ea, evt.Text);
                ea.ActionButtonClick += async (a, b) => await Launcher.LaunchUrl(link);
                ea.Click += async (a, b) => await Launcher.LaunchUrl(link);
                StandartAttachments.Children.Add(ea);
            }

            // Story
            if (NeedToShowStoryPreview && IsOutgoing.HasValue && stories.Count == 1 && (Attachments.Count == 1 || (Attachments.Count == 2 && sticker != null))) {
                StoryPreview prev = new StoryPreview(stories.FirstOrDefault());
                prev.Margin = new Thickness(0, 0, 0, 8);
                prev.HorizontalAlignment = IsOutgoing.Value ? HorizontalAlignment.Right : HorizontalAlignment.Left;

                var lastUI = StandartAttachments.Children.LastOrDefault();
                if (lastUI != null && lastUI is StickerPresenter) {
                    lastUI.Margin = new Thickness(0, -80, 0, 8);
                    lastUI.Width = 128;
                    lastUI.Height = 128;
                }
                StandartAttachments.Children.Insert(0, prev);
            } else {
                foreach (Story st in stories) {
                    string def = VKAPIHelper.GetNameOrDefaultString(st.OwnerId);
                    if (st.IsExpired || st.IsDeleted || st.IsRestricted || st.CanSee == 0) {
                        BasicAttachment ba = new BasicAttachment {
                            Margin = new Thickness(0, 0, 0, 8),
                            Icon = VKIconNames.Icon24Story,
                            Title = Assets.i18n.Resources.story,
                            Subtitle = def,
                            Name = st.ObjectType
                        };
                        ba.Click += (a, b) => ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow);
                        StandartAttachments.Children.Add(ba);
                    } else {
                        ExtendedAttachment ea = new ExtendedAttachment {
                            Margin = new Thickness(0, 0, 0, 8),
                            Title = Assets.i18n.Resources.story,
                            Subtitle = def,
                            Preview = st.Video != null ? st.Video.FirstFrameForStory.Uri : st.Photo.GetSizeAndUriForThumbnail(Constants.ExtendedAttachmentPreviewSize, Constants.ExtendedAttachmentPreviewSize).Uri,
                            ActionButtonText = Assets.i18n.Resources.watch,
                            Name = st.ObjectType
                        };
                        ea.ActionButtonClick += (a, b) => ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow);
                        ea.Click += (a, b) => ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow);
                        StandartAttachments.Children.Add(ea);
                    }
                }
            }

            // Narrative
            if (nr != null) {
                string link = $"https://m.vk.com/narrative{nr.OwnerId}_{nr.Id}";
                ExtendedAttachment ea = new ExtendedAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Title = nr.Title,
                    Subtitle = $"{Assets.i18n.Resources.narrative} {VKAPIHelper.GetNameOrDefaultString(nr.OwnerId)}",
                    Preview = nr.Cover.CroppedSizes.LastOrDefault().Uri,
                    ActionButtonText = Assets.i18n.Resources.watch,
                    Name = nr.ObjectType
                };
                ea.ActionButtonClick += async (a, b) => await Launcher.LaunchUrl(link);
                ea.Click += async (a, b) => await Launcher.LaunchUrl(link);
                StandartAttachments.Children.Add(ea);
            }

            // Curator
            if (cur != null) {
                ExtendedAttachment ea = new ExtendedAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Title = cur.Name,
                    Subtitle = Assets.i18n.Resources.curator,
                    Preview = cur.Photo[0].Uri,
                    ActionButtonText = Assets.i18n.Resources.open,
                    Name = "Curator"
                };
                ea.ActionButtonClick += async (a, b) => await Launcher.LaunchUrl(cur.Url);
                ea.Click += async (a, b) => await Launcher.LaunchUrl(cur.Url);
                StandartAttachments.Children.Add(ea);
            }

            // Audios
            foreach (Audio a in audios) {
                AudioAttachment aa = new AudioAttachment { 
                    Margin = new Thickness(0, 0, 0, 8),
                    Audio = a,
                    Name = a.ObjectType
                };
                aa.PlayAudioRequested += (b, c) => {
                    AudioPlayerViewModel.PlaySong(audios, a, Owner);
                };
                StandartAttachments.Children.Add(aa);
            }

            // Audio message
            foreach (AudioMessage am in ams) {
                // TODO: сделать проигрыватель и только потом отдельный control
                BasicAttachment ba = new BasicAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Icon = VKIconNames.Icon24Song,
                    Title = Assets.i18n.Resources.audio_message,
                    Subtitle = Assets.i18n.Resources.not_implemented,
                    Name = am.ObjectType
                };
                ba.Click += (a, b) => ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow);
                StandartAttachments.Children.Add(ba);
            }

            // Podcasts
            foreach (Podcast p in podcasts) {
                ExtendedAttachment ea = new ExtendedAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Title = p.Title,
                    Subtitle = Assets.i18n.Resources.podcast,
                    Preview = p.Info.Cover.Sizes[0].Uri,
                    ActionButtonText = Assets.i18n.Resources.play,
                    Name = p.ObjectType
                };
                ea.ActionButtonClick += (a, b) => AudioPlayerViewModel.PlayPodcast(podcasts, p, Owner);
                StandartAttachments.Children.Add(ea);
            }

            // Documents
            foreach (Document d in docs) {
                BasicAttachment ba = new BasicAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Icon = VKIconNames.Icon24Document,
                    Title = d.Title,
                    Subtitle = $"{d.Extension} · {d.Size.ToFileSize()}",
                    Name = d.ObjectType
                };

                ba.Click += async (a, b) => await Launcher.LaunchUrl(d.Url);
                StandartAttachments.Children.Add(ba);
            }

            // Textpost publish
            if (tpb != null) {
                BasicAttachment ba = new BasicAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Icon = VKIconNames.Icon24TextLiveOutline,
                    Title = tpb.Title,
                    // Subtitle = Assets.i18n.Resources.textpost_publish,
                    Name = "Textpost"
                };

                if (!String.IsNullOrEmpty(tpb.Url)) ba.Click += async (a, b) => await Launcher.LaunchUrl(tpb.Url);
                StandartAttachments.Children.Add(ba);
            }

            // Unsuported
            foreach (Attachment a in unknown) {
                StandartAttachments.Children.Add(new BasicAttachment {
                    Title = Assets.i18n.Resources.not_supported,
                    Subtitle = a.TypeString,
                    Margin = new Thickness(0, 0, 0, 8),
                    Icon = VKIconNames.Icon24Question
                });
            }

            // Gift 
            if (Gift != null) {
                StandartAttachments.Children.Add(new GiftUI { 
                    Gift = Gift,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 0, 0, 8),
                });
            }

            if (Classes.Contains(Constants.ATCHC_INBUBBLE)) {
                Control last = StandartAttachments.Children.LastOrDefault();
                if (last is BasicAttachment) {
                    last.Classes.Add(Constants.ATCHC_INBUBBLE);
                }
            }

            StandartAttachments.IsVisible = StandartAttachments.Children.Count > 0;
        }

        private void AddPreviewInfo(IPreview preview, Button button) {
            bool isNarrow = button.Width < 128;
            if (preview is Video video) {
                string meta = "►";
                if (!isNarrow) {
                    if (video.Live == 1) {
                        meta += " LIVE";
                    } else {
                        meta += $" {video.DurationTime.ToString(video.DurationTime.Hours > 0 ? "c" : @"m\:ss")}";
                    }
                }
                button.Content = BuildPreviewInfoUI(meta);
            } else if (preview is Document doc) {
                string meta = doc.Extension.ToUpper();
                if (!isNarrow) meta += $" · {doc.Size.ToFileSize()}";
                button.Content = BuildPreviewInfoUI(meta);
            }
        }

        private Border BuildPreviewInfoUI(string content) {
            Border b = new Border { 
                Background = new SolidColorBrush(Color.FromArgb(104, 0, 0, 0)),
                CornerRadius = new CornerRadius(10),
                // Padding = new Thickness(6, 3),
                Padding = new Thickness(6, 4, 6, 2),
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Child = new TextBlock {
                    Text = content,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 11, LineHeight = 14
                }
            };
            return b;
        }

        private void ImgBtn_Click(object sender, RoutedEventArgs e) {
            Button button = sender as Button;
            if (button.Tag != null && button.Tag is Tuple<List<IPreview>, IPreview> data) {
                if (data.Item2 is Video v) {
                    // TODO: video player
                    ExceptionHelper.ShowNotImplementedDialogAsync(VKSession.GetByDataContext(button).ModalWindow);
                } else {
                    List<IPreview> nonVideo = data.Item1.Where(i => i is not Video).ToList();
                    Gallery.Show(nonVideo, data.Item2);
                }
            }
        }

        private BasicAttachment GetCallInfoControl(Call call) {
            bool isCurrentUserInitiator = call.InitiatorId == VKSession.GetByDataContext(this).UserId;
            string title = isCurrentUserInitiator ? Assets.i18n.Resources.disabled : Assets.i18n.Resources.enabled;
            string subtitle = String.Empty;

            if (call.Participants != null) {
                int c = call.Participants.Count;
                subtitle = " " + Localizer.GetDeclensionFormatted2(c, "member");
            }

            switch (call.State) {
                case "reached": subtitle += call.Duration.ToString(call.Duration.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss"); break;
                case "canceled_by_receiver": subtitle += isCurrentUserInitiator ? Assets.i18n.Resources.call_declined : Assets.i18n.Resources.call_cancelled; break;
                case "canceled_by_initiator": subtitle += isCurrentUserInitiator ? Assets.i18n.Resources.disabled : Assets.i18n.Resources.enabled; break;
                default: subtitle += call.State; break;
            }

            return new BasicAttachment {
                Icon = call.Video ? VKIconNames.Icon24Videocam : VKIconNames.Icon24Phone,
                Title = call.ReceiverId.IsChat() ? Assets.i18n.Resources.group_call_in_progress : title,
                Subtitle = subtitle,
                Margin = new Thickness(0, 0, 0, 8),
                Name = "Call"
            };
        }

        private static ExtendedAttachment GetCallInfoControl(GroupCallInProgress call) {
            string subtitle = String.Empty;

            if (call.Participants != null) {
                int c = call.Participants.Count;
                subtitle = " " + Localizer.GetDeclensionFormatted2(c, "member");
            }

            return new ExtendedAttachment {
                Title = Assets.i18n.Resources.group_call_in_progress,
                Subtitle = subtitle,
                Margin = new Thickness(0, 0, 0, 8),
                Name = "GroupCallInProgress"
            };
        }
    }
}
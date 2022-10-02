using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
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

        #endregion

        #region Template elements

        StackPanel StandartAttachments;

        bool isUILoaded = false;
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            StandartAttachments = e.NameScope.Find<StackPanel>(nameof(StandartAttachments));
            isUILoaded = true;
            RenderAttachments();
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

            Sticker sticker = null;
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
            Album album = null;
            // SituationalTheme sth = null;
            // Textlive tl = null;
            TextpostPublish tpb = null;
            List<Attachment> unknown = new List<Attachment>();

            foreach (Attachment a in CollectionsMarshal.AsSpan(Attachments)) {
                switch (a.Type) {
                    case AttachmentType.Sticker: sticker = a.Sticker; break;
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
            double iwidth = MessageBubble.BUBBLE_FIXED_WIDTH - 8;
            if (previews.Count == 1) {
                var preview = previews[0].GetSizeAndUriForThumbnail();
                var size = preview.Size;
                var uri = preview.Uri;

                Rectangle rect = new Rectangle {
                    Fill = new SolidColorBrush(Color.FromArgb(128, 128, 128, 128)),
                    RadiusX = 14, RadiusY = 14,
                    Width = iwidth,
                    Height = size.Width == 0 || size.Height == 0 ? iwidth :
                        Math.Min(iwidth / (double)size.Width * (double)size.Height, iwidth / 9 * 16),
                    Margin = new Thickness(-4, 0, -4, 4)
                };
                if (uri != null) rect.SetImageFillAsync(uri);
                StandartAttachments.Children.Add(rect);
            } else if (previews.Count > 1) {
                List<Size> sizes = new List<Size>();
                foreach (IPreview preview in CollectionsMarshal.AsSpan(previews)) {
                    sizes.Add(preview.GetSizeAndUriForThumbnail().Size.ToAvaloniaSize());
                }

                var layout = PhotoLayout.Create(new Size(iwidth, iwidth), sizes, 4);
                List<Rect> thumbRects = layout.Item1;
                Size layoutSize = layout.Item2;
                List<bool[]> corners = layout.Item3; // top left, top right, bottom right, bottom left
                // corners появился из-за того, что в авалонии
                // контент не обрезается под скруглённым родителем.
                // https://github.com/AvaloniaUI/Avalonia/issues/2105

                Canvas canvas = new Canvas {
                    Width = layoutSize.Width,
                    Height = layoutSize.Height,
                    Margin = new Thickness(-4, 0, -4, 4),
                    ClipToBounds = true,
                };

                int i = 0;
                foreach (IPreview preview in CollectionsMarshal.AsSpan(previews)) {
                    Rect rect = thumbRects[i];
                    var p = preview.GetSizeAndUriForThumbnail();
                    bool[] corner = corners[i];

                    double tl = corner[0] ? 14 : 4;
                    double tr = corner[1] ? 14 : 4;
                    double br = corner[2] ? 14 : 4;
                    double bl = corner[3] ? 14 : 4;

                    Border border = new Border {
                        Background = App.GetResource<SolidColorBrush>("VKBackgroundHoverBrush"),
                        CornerRadius = new CornerRadius(tl, tr, br, bl),
                        Width = rect.Width,
                        Height = rect.Height
                    };
                    Canvas.SetLeft(border, rect.Left);
                    Canvas.SetTop(border, rect.Top);
                    if (p.Uri != null) border.SetImageBackgroundAsync(p.Uri);
                    canvas.Children.Add(border);
                    i++;
                }
                StandartAttachments.Children.Add(canvas);
            }

            // Sticker
            if (sticker != null) {
                Image stickerImage = new Image() {
                    Width = MessageBubble.STICKER_WIDTH,
                    Height = MessageBubble.STICKER_WIDTH,
                    Margin = new Thickness(0, 0, 0, 8),
                    Name = "Sticker"
                };
                stickerImage.SetUriSourceAsync(sticker.Images[sticker.Images.Count - 1].Uri);
                StandartAttachments.Children.Add(stickerImage);
            }

            // Graffiti
            if (graffiti != null) {
                double gwidth = MessageBubble.BUBBLE_FIXED_WIDTH - 8;
                Rectangle grImage = new Rectangle() {
                    Width = gwidth,
                    Height = gwidth / (double)graffiti.Width * (double)graffiti.Height,
                    Margin = new Thickness(-4, 0, -4, 4),
                    RadiusX = 14, RadiusY = 14,
                    Name = graffiti.ObjectType
                };
                grImage.SetImageFillAsync(graffiti.Uri);
                StandartAttachments.Children.Add(grImage);
            }

            // Wall post
            if (wp != null) {
                // string def = GetNameOrDefaultString(wp.OwnerOrToId, VKTextParser.GetParsedText(wp.Text));
                string def = GetNameOrDefaultString(wp.OwnerOrToId, wp.Text);
                BasicAttachment ba = new BasicAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Icon = VKIconNames.Icon24ArticleOutline,
                    Title = Localizer.Instance["wall"],
                    Subtitle = def,
                    Name = wp.ObjectType
                };
                ba.Click += (a, b) => Launcher.LaunchUrl($"https://vk.com/wall{wp.OwnerId}_{wp.Id}");
                StandartAttachments.Children.Add(ba);
            }

            // Wall reply
            if (wr != null) {
                // string def = GetNameOrDefaultString(wr.OwnerId, VKTextParser.GetParsedText(wr.Text));
                string def = GetNameOrDefaultString(wr.OwnerId, wr.Text);
                BasicAttachment ba = new BasicAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Icon = VKIconNames.Icon24CommentOutline,
                    Title = Localizer.Instance["wall_reply"],
                    Subtitle = def,
                    Name = "WallReply"
                };
                ba.Click += (a, b) => Launcher.LaunchUrl($"https://vk.com/wall{wr.OwnerId}_{wr.PostId}?reply={wr.Id}");
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
                        ea.ActionButtonClick += (a, b) => Launcher.LaunchUrl(link.Button.Action.Url);
                    }
                    if (!String.IsNullOrEmpty(link.Description)) ToolTip.SetTip(ea, link.Description);
                    if (link.Photo != null) ea.Preview = link.Photo.GetSizeAndUriForThumbnail().Uri;
                    ea.Click += (a, b) => Launcher.LaunchUrl(link.Url);
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
                    ba.Click += (a, b) => Launcher.LaunchUrl(link.Url);
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
                    ActionButtonText = Localizer.Instance["open"],
                    Name = market.ObjectType
                };
                if (!String.IsNullOrEmpty(market.Description)) ToolTip.SetTip(ea, market.Description);
                ea.ActionButtonClick += (a, b) => Launcher.LaunchUrl(link);
                ea.Click += (a, b) => Launcher.LaunchUrl(link);
                StandartAttachments.Children.Add(ea);
            }

            // Poll
            if (poll != null) {
                string def = GetNameOrDefaultString(poll.AuthorId);
                BasicAttachment ba = new BasicAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Icon = VKIconNames.Icon24Poll,
                    Title = poll.Question,
                    Subtitle = $"{Localizer.Instance["poll"]} {def}",
                    Name = poll.ObjectType,
                };
                ba.Click += (a, b) => Launcher.LaunchUrl($"https://vk.com/poll{poll.OwnerId}_{poll.Id}");
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
                ea.ActionButtonClick += (a, b) => Launcher.LaunchUrl(link);
                ea.Click += (a, b) => Launcher.LaunchUrl(link);
                StandartAttachments.Children.Add(ea);
            }

            // Story
            foreach (Story st in stories) {
                string def = GetNameOrDefaultString(st.OwnerId);
                if (st.IsExpired || st.IsDeleted || st.IsRestricted) {
                    BasicAttachment ba = new BasicAttachment {
                        Margin = new Thickness(0, 0, 0, 8),
                        Icon = VKIconNames.Icon24Story,
                        Title = Localizer.Instance["story"],
                        Subtitle = def,
                        Name = st.ObjectType
                    };
                    // ba.Click += (a, b) => Launcher.LaunchUrl(link);
                    StandartAttachments.Children.Add(ba);
                } else {
                    ExtendedAttachment ea = new ExtendedAttachment {
                        Margin = new Thickness(0, 0, 0, 8),
                        Title = Localizer.Instance["story"],
                        Subtitle = def,
                        Preview = st.Video != null ? st.Video.FirstFrameForStory.Uri : st.Photo.GetSizeAndUriForThumbnail().Uri,
                        ActionButtonText = Localizer.Instance["watch"],
                        Name = st.ObjectType
                    };
                    // ea.ActionButtonClick += (a, b) => Launcher.LaunchUrl(link);
                    // ea.Click += (a, b) => Launcher.LaunchUrl(link);
                    StandartAttachments.Children.Add(ea);
                }
            }

            // Narrative
            if (nr != null) {
                string link = $"https://m.vk.com/narrative{nr.OwnerId}_{nr.Id}";
                ExtendedAttachment ea = new ExtendedAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Title = nr.Title,
                    Subtitle = $"{Localizer.Instance["narrative"]} {GetNameOrDefaultString(nr.OwnerId)}",
                    Preview = nr.Cover.CroppedSizes.LastOrDefault().Uri,
                    ActionButtonText = Localizer.Instance["watch"],
                    Name = nr.ObjectType
                };
                ea.ActionButtonClick += (a, b) => Launcher.LaunchUrl(link);
                ea.Click += (a, b) => Launcher.LaunchUrl(link);
                StandartAttachments.Children.Add(ea);
            }

            // Curator
            if (cur != null) {
                ExtendedAttachment ea = new ExtendedAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Title = cur.Name,
                    Subtitle = Localizer.Instance["curator"],
                    Preview = cur.Photo[0].Uri,
                    ActionButtonText = Localizer.Instance["open"],
                    Name = "Curator"
                };
                ea.ActionButtonClick += (a, b) => Launcher.LaunchUrl(cur.Url);
                ea.Click += (a, b) => Launcher.LaunchUrl(cur.Url);
                StandartAttachments.Children.Add(ea);
            }

            // Audios
            foreach (Audio a in audios) {
                // TODO: сделать проигрыватель и только потом отдельный control с play/pause
                BasicAttachment ba = new BasicAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Icon = VKIconNames.Icon24Song,
                    Title = a.Title,
                    Subtitle = a.Subtitle,
                    Name = a.ObjectType
                };
                StandartAttachments.Children.Add(ba);
            }

            // Audio message
            foreach (AudioMessage am in ams) {
                // TODO: сделать проигрыватель и только потом отдельный control
                BasicAttachment ba = new BasicAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Icon = VKIconNames.Icon24Song,
                    Title = "Audio message",
                    Subtitle = "Not implemented yet",
                    Name = am.ObjectType
                };
                StandartAttachments.Children.Add(ba);
            }

            // Podcasts
            foreach (Podcast p in podcasts) {
                ExtendedAttachment ea = new ExtendedAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Title = p.Title,
                    Subtitle = Localizer.Instance["podcast"],
                    Preview = p.Info.Cover.Sizes[0].Uri,
                    ActionButtonText = Localizer.Instance["play"],
                    Name = p.ObjectType
                };
                //ea.ActionButtonClick += (a, b) => Launcher.LaunchUrl();
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

                ba.Click += (a, b) => Launcher.LaunchUrl(d.Url);
                StandartAttachments.Children.Add(ba);
            }

            // Textpost publish
            if (tpb != null) {
                BasicAttachment ba = new BasicAttachment {
                    Margin = new Thickness(0, 0, 0, 8),
                    Icon = VKIconNames.Icon24TextLiveOutline,
                    Title = tpb.Title,
                    Subtitle = Localizer.Instance["textpost_publish"],
                    Name = "Textpost"
                };

                if (!String.IsNullOrEmpty(tpb.Url)) ba.Click += (a, b) => Launcher.LaunchUrl(tpb.Url);
                StandartAttachments.Children.Add(ba);
            }

            // Unsuported
            foreach (Attachment a in unknown) {
                StandartAttachments.Children.Add(new BasicAttachment {
                    Title = Localizer.Instance["not_supported"],
                    Subtitle = a.TypeString,
                    Margin = new Thickness(0, 0, 0, 8),
                    Icon = VKIconNames.Icon24Question
                });
            }

            StandartAttachments.IsVisible = StandartAttachments.Children.Count > 0;
        }

        private string GetNameOrDefaultString(int ownerId, string defaultStr = null) {
            if (!String.IsNullOrEmpty(defaultStr)) return defaultStr;
            string from = "";
            if (ownerId > 0) {
                User u = CacheManager.GetUser(ownerId);
                from = u != null ? $"{Localizer.Instance["from"]} {u.FirstNameGen} {u.LastNameGen}" : "";
            } else if (ownerId < 0) {
                Group u = CacheManager.GetGroup(ownerId);
                from = u != null ? $"{Localizer.Instance["from"]} \"{u.Name}\"" : "";
            }
            return from;
        }

        private BasicAttachment GetCallInfoControl(Call call) {
            bool isCurrentUserInitiator = call.InitiatorId == VKSession.GetByDataContext(this).UserId;
            string title = Localizer.Instance[isCurrentUserInitiator ? "outgoing_call" : "incoming_call"];
            string subtitle = String.Empty;

            if (call.Participants != null) {
                int c = call.Participants.Count;
                subtitle = " " + Localizer.Instance.GetDeclensionFormatted2(c, "member");
            }

            switch (call.State) {
                case "reached": subtitle += call.Duration.ToString(call.Duration.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss"); break;
                case "canceled_by_receiver": subtitle += Localizer.Instance[isCurrentUserInitiator ? "call_declined" : "call_canceled"]; break;
                case "canceled_by_initiator": subtitle += Localizer.Instance[isCurrentUserInitiator ? "call_canceled" : "call_missed"]; break;
                default: subtitle += call.State; break;
            }

            return new BasicAttachment {
                Icon = call.Video ? VKIconNames.Icon24Videocam : VKIconNames.Icon24Phone,
                Title = call.ReceiverId > 2000000000 ? Localizer.Instance["group_call_in_progress"] : title,
                Subtitle = subtitle,
                Name = "Call"
            };
        }

        private static ExtendedAttachment GetCallInfoControl(GroupCallInProgress call) {
            string subtitle = String.Empty;

            if (call.Participants != null) {
                int c = call.Participants.Count;
                subtitle = " " + Localizer.Instance.GetDeclensionFormatted2(c, "member");
            }

            return new ExtendedAttachment {
                Title = Localizer.Instance["group_call_in_progress"],
                Subtitle = subtitle,
                Name = "GroupCallInProgress"
            };
        }
    }
}
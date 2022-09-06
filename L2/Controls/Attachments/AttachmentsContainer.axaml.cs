using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
            if (!isUILoaded) return;

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
            Story st = null;
            // Narrative nr = null;
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
            // TextpostPublish tpb = null;
            List<Attachment> unknown = new List<Attachment>();

            foreach (Attachment a in Attachments) {
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
                    case AttachmentType.Story: st = a.Story; break;
                    // case AttachmentType.Narrative: nr = a.Narrative; break;
                    case AttachmentType.Document: if (a.Document.Preview != null) { previews.Add(a.Document); } else { docs.Add(a.Document); }; break;
                    // case AttachmentType.Page: page = a.Page; break;
                    // case AttachmentType.Note: note = a.Note; break;
                    // case AttachmentType.Album: album = a.Album; break;
                    // case AttachmentType.PrettyCards: break; // чтобы сниппет "unknown attachment" не добавлялся
                    // case AttachmentType.SituationalTheme: sth = a.SituationalTheme; break;
                    // case AttachmentType.Textlive: tl = a.Textlive; break;
                    // case AttachmentType.TextpostPublish: tpb = a.TextpostPublish; break;
                    default: unknown.Add(a); break;
                }
            }

            StandartAttachments.Children.Clear();

            // Images
            double iwidth = MessageBubble.BUBBLE_FIXED_WIDTH - 8;
            if (previews.Count == 1) {
                var preview = previews[0];
                var size = preview.PreviewImageSize;
                var uri = preview.PreviewImageUri;

                Rectangle rect = new Rectangle {
                    Fill = new SolidColorBrush(Color.FromArgb(128, 128, 128, 128)),
                    RadiusX = 13, RadiusY = 13,
                    Width = iwidth,
                    Height = size.Width == 0 || size.Height == 0 ? iwidth :
                        Math.Min(iwidth / (double)size.Width * (double)size.Height, iwidth / 9 * 16),
                    Margin = new Thickness(-4, 0, -4, 4)
                };
                rect.SetImageFillAsync(uri);
                StandartAttachments.Children.Add(rect);
            } else if (previews.Count > 1) {
                //Border temp = new Border {
                //    Background = new SolidColorBrush(Color.FromArgb(128, 128, 128, 128)),
                //    CornerRadius = new CornerRadius(13),
                //    Width = iwidth,
                //    Height = iwidth,
                //    Margin = new Thickness(-4, 0, -4, 4),
                //    Child = new TextBlock {
                //        Margin = new Thickness(8),
                //        Text = "Photos grid WIP",
                //        FontStyle = FontStyle.Italic
                //    }
                //};
                //StandartAttachments.Children.Add(temp);

                List<Size> sizes = new List<Size>();
                foreach (IPreview preview in CollectionsMarshal.AsSpan(previews)) {
                    sizes.Add(preview.PreviewImageSize.ToAvaloniaSize());
                }

                var layout = PhotoLayout.Create(new Size(iwidth, iwidth), sizes, 4);
                List<Rect> thumbRects = layout.Item1;
                Size layoutSize = layout.Item2;

                Canvas canvas = new Canvas {
                    Width = layoutSize.Width,
                    Height = layoutSize.Height
                };

                int i = 0;
                foreach (IPreview preview in CollectionsMarshal.AsSpan(previews)) {
                    Rect rect = thumbRects[i];
                    Rectangle rectangle = new Rectangle {
                        Fill = new SolidColorBrush(Color.FromArgb(128, 128, 128, 128)),
                        RadiusX = 4, RadiusY = 4,
                        Width = rect.Width,
                        Height = rect.Height
                    };
                    Canvas.SetLeft(rectangle, rect.Left);
                    Canvas.SetTop(rectangle, rect.Top);
                    rectangle.SetImageFillAsync(preview.PreviewImageUri);
                    canvas.Children.Add(rectangle);
                    i++;
                }

                Border layoutRoot = new Border { 
                    CornerRadius = new CornerRadius(13),
                    Width = layoutSize.Width,
                    Height = layoutSize.Height,
                    Margin = new Thickness(-4, 0, -4, 4),
                    Child = canvas
                };
                StandartAttachments.Children.Add(layoutRoot);
            }

            // Sticker
            if (sticker != null) {
                Image stickerImage = new Image() {
                    Width = MessageBubble.STICKER_WIDTH,
                    Height = MessageBubble.STICKER_WIDTH,
                    Margin = new Thickness(0, 0, 0, 8)
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
                    RadiusX = 13, RadiusY = 13,
                };
                grImage.SetImageFillAsync(graffiti.Uri);
                StandartAttachments.Children.Add(grImage);
            }

            // Other placeholder
            foreach (Attachment a in Attachments) {
                if (a.Type == AttachmentType.Photo || a.Type == AttachmentType.Video
                    || a.Type == AttachmentType.Gift || a.Type == AttachmentType.Sticker || a.Type == AttachmentType.Graffiti
                    || (a.Type == AttachmentType.Document && a.Document.Preview != null)) continue;
                StandartAttachments.Children.Add(new BasicAttachment {
                    Title = a.TypeString,
                    Subtitle = a.TypeString,
                    Margin = new Thickness(0, 0, 0, 8),
                    Icon = (StreamGeometry)VKUI.VKUITheme.Icons["Icon24Done"]
                });
            }

            StandartAttachments.IsVisible = StandartAttachments.Children.Count > 0;
        }
    }
}

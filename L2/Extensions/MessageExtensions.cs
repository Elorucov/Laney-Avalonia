using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.DataModels;
using ELOR.Laney.Helpers;
using ELOR.Laney.ViewModels.Controls;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ELOR.Laney.Extensions {
    public static class MessageExtensions {

        // Проверяет, можно ли прикрепить вложение по типу.
        // При изменении списка не забудьте менять и в OutboundAttachmentViewModel.FromAttachmentBase!
        public static bool CanAttachToSend(this AttachmentType type) {
            switch (type) {
                case AttachmentType.Audio:
                case AttachmentType.Graffiti:
                case AttachmentType.Document:
                case AttachmentType.Market:
                case AttachmentType.Narrative:
                case AttachmentType.Photo:
                case AttachmentType.Podcast:
                case AttachmentType.Poll:
                case AttachmentType.Story:
                case AttachmentType.Video:
                case AttachmentType.Wall:
                case AttachmentType.Link: //
                    return true;
                default:
                    return false;
            }
        }

        // Возвращает true, если вложение с типом type отображается как сниппет,
        // т. е. BasicAttachment или ExtendedAttachment.
        // Проверить это можно в файле AttachmentsContainer.xaml.cs > RenderAttachments()
        public static bool IsAttachmentWithSnippetInUI(this Attachment attachment) {
            switch (attachment.Type) {
                case AttachmentType.Wall:
                case AttachmentType.WallReply:
                case AttachmentType.Link:
                case AttachmentType.Market:
                case AttachmentType.Poll:
                case AttachmentType.Call:
                case AttachmentType.Story:
                case AttachmentType.GroupCallInProgress:
                case AttachmentType.Event:
                case AttachmentType.Narrative:
                case AttachmentType.Curator:
                case AttachmentType.Podcast:
                case AttachmentType.TextpostPublish:
                case AttachmentType.Unknown:
                    return true;
                case AttachmentType.Document:
                    return attachment.Document?.Preview == null;
                default:
                    return false;
            }
        }

        public static string ToNormalString(this Message msg) {
            if (msg.Action != null) {
                return new VKActionMessage(msg.Action, msg.FromId).ToString();
            }

            if (!String.IsNullOrEmpty(msg.Text)) return TextParser.GetParsedText(msg.Text);
            if (msg.Attachments?.Count > 0) {
                int count = msg.Attachments.Count;
                if (msg.Attachments.All(a => a.Type == msg.Attachments[0].Type) && msg.Geo == null) {
                    string type = msg.Attachments[0].TypeString;
                    switch (msg.Attachments[0].Type) {
                        case AttachmentType.Audio:
                        case AttachmentType.AudioMessage:
                        case AttachmentType.Document:
                        case AttachmentType.Photo:
                        case AttachmentType.Video: return Localizer.GetDeclensionFormatted2(count, type);
                        case AttachmentType.Call:
                        case AttachmentType.Curator:
                        case AttachmentType.Event:
                        case AttachmentType.Gift:
                        case AttachmentType.Graffiti:
                        case AttachmentType.GroupCallInProgress:
                        case AttachmentType.Link:
                        case AttachmentType.Market:
                        case AttachmentType.Podcast:
                        case AttachmentType.Poll:
                        case AttachmentType.Sticker:
                        case AttachmentType.UGCSticker:
                        case AttachmentType.Story:
                        case AttachmentType.Wall:
                        case AttachmentType.WallReply:
                        case AttachmentType.Narrative: return Localizer.Get(type);
                        case AttachmentType.TextpostPublish: return Localizer.Get(type);
                        default: return Localizer.GetDeclensionFormatted2(count, "attachment");
                    }
                } else {
                    if (msg.Geo != null && count > 0) count++;
                    return Localizer.GetDeclensionFormatted2(count, "attachment");
                }
            }
            if (msg.Geo != null) return Assets.i18n.Resources.geo;
            if (msg.ForwardedMessages?.Count > 0) return Localizer.GetDeclensionFormatted2(msg.ForwardedMessages.Count, "forwarded_message");

            return msg.IsExpired ? Assets.i18n.Resources.msg_expired : Assets.i18n.Resources.empty_message;
        }

        public static bool CanEdit(this MessageViewModel m, long sessionId) {
            return m.SentTime.AddDays(1) > DateTime.Now && m.SenderId == sessionId && m.Action == null
                && m.UIType != MessageUIType.Gift && m.UIType != MessageUIType.Sticker;
        }

        public static Uri GetPreviewImageUri(this List<Attachment> attachments) {
            if (attachments == null || attachments.Count == 0) return null;
            if (attachments.Count == 1) {
                var a = attachments[0];
                if (a.Type == AttachmentType.Sticker) {
                    return a.Sticker.GetSizeAndUriForThumbnail(Constants.CompactMessagePreviewSize).Uri;
                } else if (a.Type == AttachmentType.UGCSticker) {
                    if (a.UGCSticker.Images?.Count > 0) return a.UGCSticker.Images.LastOrDefault().Uri;
                } else if (a.Type == AttachmentType.Graffiti) {
                    return a.Graffiti.Uri;
                } else if (a.Type == AttachmentType.Story) {
                    Story story = a.Story;
                    if (story.Type == StoryType.Photo && story.Photo != null) {
                        return story.Photo.GetSizeAndUriForThumbnail(Constants.CompactMessagePreviewSize, Constants.CompactMessagePreviewSize).Uri;
                    } else if (story.Type == StoryType.Video && story.Video != null) {
                        return story.Video.GetSizeAndUriForThumbnail(Constants.CompactMessagePreviewSize, Constants.CompactMessagePreviewSize).Uri;
                    }
                } else if (a.Type == AttachmentType.Gift) {
                    return a.Gift.ThumbUri;
                }
            }

            var images = attachments
                .Where(a => a.Type == AttachmentType.Photo || a.Type == AttachmentType.Video
                || (a.Type == AttachmentType.Document && a.Document.Preview != null));

            var iCount = images.Count();
            if (iCount > 0) {
                Attachment a = images.FirstOrDefault();
                if (a.Photo != null) {
                    return a.Photo.GetSizeAndUriForThumbnail(Constants.CompactMessagePreviewSize, Constants.CompactMessagePreviewSize).Uri;
                } else if (a.Video != null) {
                    return a.Video.GetSizeAndUriForThumbnail(Constants.CompactMessagePreviewSize, Constants.CompactMessagePreviewSize).Uri;
                } else if (a.Photo != null) {
                    return a.Document.Preview?.Photo?.GetSizeAndUriForThumbnail(Constants.CompactMessagePreviewSize, Constants.CompactMessagePreviewSize).Uri;
                }
            }

            return null;
        }
    }
}
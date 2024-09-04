using ELOR.Laney.ViewModels.Controls;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static bool CanEdit(this MessageViewModel m, long sessionId) {
            return m.SentTime.AddDays(1) > DateTime.Now && m.SenderId == sessionId && m.Action == null
                && m.UIType != MessageUIType.Gift && m.UIType != MessageUIType.Sticker;
        }
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.ViewModels.Controls;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using VKUI.Controls;

namespace ELOR.Laney.Helpers {
    public static class VKAPIHelper {
        public static readonly List<string> Fields = new List<string>() { "photo_200", "photo_100", "photo_50",
            "ban_info", "blacklisted", "blacklisted_by_me", "can_message", "can_write_private_message", "friend_status",
            "is_messages_blocked", "online_info", "domain", "verified", "sex", "activity",
            "first_name_gen", "first_name_dat", "first_name_acc", "first_name_ins", "first_name_abl",
            "last_name_gen", "last_name_dat", "last_name_acc", "last_name_ins", "last_name_abl", "photo_avg_color"
        };

        public static readonly List<string> UserFields = new List<string>() { "photo_200", "photo_100", "photo_50", "sex",
            "blacklisted", "blacklisted_by_me", "can_write_private_message", "friend_status", "online_info", "domain", "verified", "photo_avg_color"
        };

        public static readonly List<string> GroupFields = new List<string>() { "photo_200", "photo_100", "photo_50", "description",
            "ban_info", "can_message", "is_messages_blocked", "domain", "activity", "status", "verified", "activity", "photo_avg_color"
        };

        #region Errors

        public static string GetUnderstandableErrorMessage(int code) {
            string key = $"api_error_{code}";
            string value = Assets.i18n.Resources.ResourceManager.GetString(key, Assets.i18n.Resources.Culture);
            return !string.IsNullOrEmpty(value) ? value : string.Empty;
        }

        public static string GetUnderstandableErrorMessage(APIException ex) {
            string uem = GetUnderstandableErrorMessage(ex.Code);
            return string.IsNullOrEmpty(uem) ? $"{ex.Message} ({ex.Code})" : uem;
        }

        public static string GetUnderstandableErrorMessage(int code, string message) {
            string uem = GetUnderstandableErrorMessage(code);
            return string.IsNullOrEmpty(uem) ? $"{message} ({code})" : uem;
        }

        #endregion

        public static string GetOnlineInfo(UserOnlineInfo info, Sex sex) {
            if (info != null) {
                if (info.Visible) {
                    if (info.IsOnline) {
                        return Assets.i18n.Resources.online;
                    } else {
                        return info.LastSeen.Year >= 2006 ?
                            Localizer.GetFormatted(sex, "offline_last_seen", info.LastSeen.ToHumanizedString()) :
                            Assets.i18n.Resources.offline; // у забаненных/удалённых возвращается 0 в unixtime. 
                    }
                } else {
                    switch (info.Status) {
                        case UserOnlineStatus.Recently: return Localizer.Get("offline_recently", sex);
                        case UserOnlineStatus.LastWeek: return Localizer.Get("offline_last_week", sex);
                        case UserOnlineStatus.LastMonth: return Localizer.Get("offline_last_month", sex);
                        case UserOnlineStatus.LongAgo: return Localizer.Get("offline_long_ago", sex);
                    }
                }
            }
            return Assets.i18n.Resources.offline;
        }

        public static string GetSenderNameShort(MessageViewModel msg) {
            if (msg.Action != null) return string.Empty;
            StringBuilder sender = new StringBuilder();

            if (msg.SenderId == VKSession.Main.UserId && msg.PeerId != VKSession.Main.UserId) {
                sender.Append(Assets.i18n.Resources.you);
            } else if (msg.PeerId.IsChat()) {
                sender.Append(CacheManager.GetNameOnly(msg.SenderId, true));
            }

            if (sender.Length > 0) sender.Append(": ");
            return sender.ToString();
        }

        public static SolidColorBrush GetDocumentIconBackground(DocumentType type) {
            switch (type) {
                case DocumentType.Text: return new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                case DocumentType.Archive: return new SolidColorBrush(Color.FromArgb(255, 118, 185, 121));
                case DocumentType.GIF: return new SolidColorBrush(Color.FromArgb(255, 119, 165, 214));
                case DocumentType.Image: return new SolidColorBrush(Color.FromArgb(255, 119, 165, 214));
                case DocumentType.Audio: return new SolidColorBrush(Color.FromArgb(255, 186, 104, 200));
                case DocumentType.Video: return new SolidColorBrush(Color.FromArgb(255, 229, 115, 155));
                case DocumentType.EBook: return new SolidColorBrush(Color.FromArgb(255, 255, 174, 56));
                default: return new SolidColorBrush(Color.FromArgb(255, 119, 165, 214));
            }
        }

        public static string GetDocumentIcon(DocumentType type) {
            switch (type) {
                case DocumentType.Text: return VKIconNames.Icon28ArticleOutline;
                case DocumentType.Archive: return VKIconNames.Icon28ZipOutline;
                case DocumentType.GIF: return VKIconNames.Icon28PictureOutline;
                case DocumentType.Image: return VKIconNames.Icon28PictureOutline;
                case DocumentType.Audio: return VKIconNames.Icon28MusicOutline;
                case DocumentType.Video: return VKIconNames.Icon28VideoOutline;
                case DocumentType.EBook: return VKIconNames.Icon28ArticleOutline;
                default: return VKIconNames.Icon28DocumentOutline;
            }
        }

        public static string GetNormalizedBirthDate(string bdate) {
            string[] a = bdate.Split('.');
            DateTime dt = a.Length == 3 ? new DateTime(Int32.Parse(a[2]), Int32.Parse(a[1]), Int32.Parse(a[0])) : new DateTime(1604, Int32.Parse(a[1]), Int32.Parse(a[0]));
            return a.Length == 3 ? $"{dt.ToString("M")} {dt.Year}" : dt.ToString("M");
        }

        public static string GetNameOrDefaultString(long ownerId, string defaultStr = null) {
            if (!String.IsNullOrEmpty(defaultStr)) return defaultStr;
            string from = "";
            if (ownerId.IsUser()) {
                User u = CacheManager.GetUser(ownerId);
                from = u != null ? $"{Assets.i18n.Resources.from} {u.FirstNameGen} {u.LastNameGen}" : "";
            } else if (ownerId.IsGroup()) {
                Group u = CacheManager.GetGroup(ownerId);
                from = u != null ? $"{Assets.i18n.Resources.from} \"{u.Name}\"" : "";
            }
            return from;
        }

        #region Execute

        public static string BuildCodeForGetMessagesByCMID(long groupId, List<KeyValuePair<long, int>> peerMessagePair, List<string> fields) {
            StringBuilder forks = new StringBuilder(), waits = new StringBuilder(), 
                items = new StringBuilder(), profiles = new StringBuilder(), groups = new StringBuilder();

            int i = 1;

            foreach (var pm in peerMessagePair) {
                string fork = $"var f{i} = fork(API.messages.getByConversationMessageId({{\"group_id\": {groupId}, \"peer_id\": {pm.Key}, \"conversation_message_ids\": {pm.Value}, \"extended\": 1, \"fields\": \"{String.Join(',', fields)}\"}}));\n";
                forks.Append(fork);

                string wait = $"var w{i} = wait(f{i});\n";
                waits.Append(wait);

                string item = $"response.items.push(w{i}.items[0]);\n";
                items.Append(item);

                string profile = $@"if (w{i}.profiles) {{
  var pi{i} = 0;
  while (pi{i} < w{i}.profiles.length) {{
    response.profiles.push(w{i}.profiles[pi{i}]);
    pi{i} = pi{i} + 1;
  }}
}}";
                profiles.Append(profile);

                string group = $@"if (w{i}.groups) {{
  var gi{i} = 0;
  while (gi{i} < w{i}.groups.length) {{
    response.groups.push(w{i}.groups[gi{i}]);
    gi{i} = gi{i} + 1;
  }}
}}";
                groups.Append(group);

                i++;
            }

            string code = $@"
var response = {{items: [],
  profiles: [],
  groups: []
}};

{forks}

{waits}

{items}

{profiles}

{groups}

return response;
";
            return code;
        }

        #endregion

        // TODO: убрать методы кнопок ботов в отдельный класс.

        internal static void GenerateButtons(StackPanel root, List<List<BotButton>> buttons) {
            bool isFirstRow = true;
            foreach (var row in CollectionsMarshal.AsSpan(buttons)) {
                Grid buttonsRow = new Grid {
                    ColumnDefinitions = new ColumnDefinitions(),
                    Margin = new Thickness(-4, isFirstRow ? 0 : 8, -4, 0)
                };
                for (byte i = 0; i < row.Count; i++) {
                    buttonsRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                    Button button = BuildButton(row[i]);
                    button.HorizontalAlignment = HorizontalAlignment.Stretch;
                    button.Click += (a, b) => {
                        ExceptionHelper.ShowNotImplementedDialogAsync(VKSession.GetByDataContext(root).ModalWindow);
                    };

                    Grid.SetColumn(button, i);
                    buttonsRow.Children.Add(button);
                }
                isFirstRow = false;
                root.Children.Add(buttonsRow);
            }
        }

        internal static void GenerateButtons(StackPanel root, List<BotButton> buttons) {
            bool isFirstRow = true;
            foreach (var row in CollectionsMarshal.AsSpan(buttons)) {
                Button button = BuildButton(row);
                button.Margin = new Thickness(button.Margin.Left, isFirstRow ? 0 : 8, button.Margin.Right, button.Margin.Bottom);
                button.HorizontalAlignment = HorizontalAlignment.Stretch;
                button.Click += (a, b) => {
                    ExceptionHelper.ShowNotImplementedDialogAsync(VKSession.GetByDataContext(root).ModalWindow);
                };

                isFirstRow = false;
                root.Children.Add(button);
            }
        }

        private static Button BuildButton(BotButton button) {
            Button buttonUI = new Button {
                Margin = new Thickness(4, 0, 4, 0),
                Padding = new Thickness(0, 0, 0, 0),
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
            buttonUI.Classes.Add("Medium");

            StackPanel contentIn = new StackPanel {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            VKIcon icon = new VKIcon {
                Margin = new Thickness(0, 8, 0, 8),
                IsVisible = false,
            };

            TextBlock label = new TextBlock {
                FontSize = 15,
                LineHeight = 15,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeight.Medium,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(4, 10, 4, 10)
            };
            if (button.Color != BotButtonColor.Default || button.Action.Type == BotButtonType.VKPay)
                label.Classes.Add("ButtonIn");

            contentIn.Children.Add(icon);
            contentIn.Children.Add(label);

            Grid content = new Grid { Height = 36 };
            content.Children.Add(contentIn);
            buttonUI.Content = content;

            if (button.Action.Type == BotButtonType.VKPay) {
                buttonUI.Classes.Add("Primary");
            } else {
                switch (button.Color) {
                    case BotButtonColor.Primary:
                        buttonUI.Classes.Add("Primary");
                        icon.RegisterThemeResource(VKIcon.ForegroundProperty, "VKButtonPrimaryForegroundBrush");
                        break;
                    case BotButtonColor.Positive:
                        buttonUI.Classes.Add("Commerce");
                        icon.RegisterThemeResource(VKIcon.ForegroundProperty, "VKButtonCommerceForegroundBrush");
                        break;
                    case BotButtonColor.Negative:
                        buttonUI.Classes.Add("Negative");
                        icon.RegisterThemeResource(VKIcon.ForegroundProperty, "VKButtonCommerceForegroundBrush");
                        break;
                    default:
                        icon.RegisterThemeResource(VKIcon.ForegroundProperty, "VKButtonSecondaryForegroundBrush");
                        break;
                }
            }

            switch (button.Action.Type) {
                case BotButtonType.VKPay:
                    label.Text = "Pay via VK Pay";
                    break;
                case BotButtonType.Location:
                    label.Text = Assets.i18n.Resources.geo;
                    icon.Id = VKIconNames.Icon20PlaceOutline;
                    icon.IsVisible = true;
                    break;
                case BotButtonType.OpenApp:
                    label.Text = button.Action.Label;
                    icon.Id = VKIconNames.Icon20ServicesOutline;
                    icon.IsVisible = true;
                    break;
                default:
                    label.Text = button.Action.Label;
                    break;
            }

            if (button.Action.Type == BotButtonType.OpenLink) {
                var arrowIcon = new VKIcon {
                    Width = 12,
                    Height = 12,
                    Margin = new Thickness(3),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Id = VKIconNames.Icon12ArrowUpRight
                };
                arrowIcon.RegisterThemeResource(VKIcon.ForegroundProperty, "VKButtonSecondaryForegroundBrush");
                content.Children.Add(arrowIcon);
            }
            return buttonUI;
        }
    }
}
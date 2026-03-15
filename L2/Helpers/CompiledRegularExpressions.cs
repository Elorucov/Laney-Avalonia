using System.Text.RegularExpressions;

namespace ELOR.Laney.Helpers {
    public partial class CompiledRegularExpressions {

        [GeneratedRegex("(http(s)?://)?(m.)?vk.(com|ru)/id[0-9]+")]
        public static partial Regex User();

        [GeneratedRegex("(http(s)?://)?(m.)?vk.(com|ru)/(club|public|event)[0-9]+")]
        public static partial Regex Group();

        [GeneratedRegex("(http(s)?://)?(m.)?vk.(com|ru)/wall[-0-9]+_[0-9]+")]
        public static partial Regex Wall();

        [GeneratedRegex("(http(s)?://)?(m.)?vk.(com|ru)/poll[-0-9]+_[0-9]+")]
        public static partial Regex Poll();

        [GeneratedRegex(@"(http(s)?://)?vk.me/join/[a-zA-Z0-9_\s\-]+")]
        public static partial Regex ConvoInvite();

        [GeneratedRegex(@"(http(s)?://)?vk.me/(?!app$)[a-zA-Z0-9._\-]+")]
        public static partial Regex VKME();

        [GeneratedRegex(@"(?!(http(s)?://)?vk.(com|ru)/write)[-0-9]+")]
        public static partial Regex Write();

        [GeneratedRegex(@"(http(s)?://)?vk.(com|ru)/stickers/([a-zA-Z0-9_\-]+)")]
        public static partial Regex Stickers();

        [GeneratedRegex(@"(http(s)?://)?vk.(com|ru)/[a-zA-Z0-9_\-]+")]
        public static partial Regex VK();

        [GeneratedRegex(@"(?![A-Za-z]\S)[-0-9]+")]
        public static partial Regex IDs();

        [GeneratedRegex(@"(?<=(http(s)?://)?vk.(com|ru|me)/)([A-Za-z0-9._/-]+)")]
        public static partial Regex ScreenName();

        [GeneratedRegex(@"(?:(?:http|https):\/\/)?([a-z0-9.\-]*\.)?([-a-zA-Z0-9а-яА-Я]{1,256})\.([-a-zA-Z0-9а-яА-Я]{2,8})\b(?:\/[-a-zA-Z0-9а-яА-Я@:%_\+.~#?&\/=]*)?")]
        public static partial Regex URL();

        [GeneratedRegex(@"([\w\d.]+)@([a-zA-Z0-9а-яА-Я.]{2,256}\.[a-zа-я]{2,8})")]
        public static partial Regex Email();

        [GeneratedRegex(@"\[(id)(\d+)\|(.*?)\]")]
        public static partial Regex UserMention();

        [GeneratedRegex(@"\[(club|public|event)(\d+)\|(.*?)\]")]
        public static partial Regex GroupMention();

        [GeneratedRegex(@"\[((?:http|https):\/\/vk.(com|ru)\/[\w\d\W.]*?)\|((.*?)+?)\]")]
        public static partial Regex LinkInText();
    }
}

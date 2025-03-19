﻿using ELOR.Laney.Views.Modals;
using Serilog;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public enum VKLinkType {
        Unknown, User, Group, Wall, Poll, ConversationInvite, WriteVkMe, Write, StickerPack, ScreenName
    }

    public static class Router {
        #region Regex and parsing VK links

        private static readonly Regex userReg = new Regex("(http(s)?://)?(m.)?vk.(com|ru)/id[0-9]+", RegexOptions.Compiled);
        private static readonly Regex groupReg = new Regex("(http(s)?://)?(m.)?vk.(com|ru)/(club|public|event)[0-9]+", RegexOptions.Compiled);
        private static readonly Regex wallReg = new Regex("(http(s)?://)?(m.)?vk.(com|ru)/wall[-0-9]+_[0-9]+", RegexOptions.Compiled);
        private static readonly Regex pollReg = new Regex("(http(s)?://)?(m.)?vk.(com|ru)/poll[-0-9]+_[0-9]+", RegexOptions.Compiled);
        private static readonly Regex convInvReg = new Regex(@"(http(s)?://)?vk.me/join/[a-zA-Z0-9_\s\-]+", RegexOptions.Compiled);
        private static readonly Regex vkmeReg = new Regex(@"(http(s)?://)?vk.me/(?!app$)[a-zA-Z0-9._\-]+", RegexOptions.Compiled);
        private static readonly Regex writeReg = new Regex("(http(s)?://)?vk.(com|ru)/write[-0-9]+", RegexOptions.Compiled);
        private static readonly Regex stickersReg = new Regex(@"(http(s)?://)?vk.(com|ru)/stickers/([a-zA-Z0-9_\-]+)", RegexOptions.Compiled);
        private static readonly Regex vkReg = new Regex(@"(http(s)?://)?vk.(com|ru)/[a-zA-Z0-9_\-]+", RegexOptions.Compiled);

        private static readonly Regex idsReg = new Regex(@"(?![A-Za-z]\S)[-0-9]+", RegexOptions.Compiled);
        private static readonly Regex screenNameReg = new Regex(@"(?<=(http(s)?://)?vk.(com|ru|me)/)([A-Za-z0-9._/-]+)", RegexOptions.Compiled);
        private static readonly Regex writeIdReg = new Regex(@"(?!(http(s)?://)?vk.com/write)[-0-9]+", RegexOptions.Compiled);

        public static VKLinkType GetLinkType(string url) {
            if (userReg.IsMatch(url)) return VKLinkType.User;
            if (groupReg.IsMatch(url)) return VKLinkType.Group;
            if (wallReg.IsMatch(url)) return VKLinkType.Wall;
            if (pollReg.IsMatch(url)) return VKLinkType.Poll;
            if (convInvReg.IsMatch(url)) return VKLinkType.ConversationInvite;
            if (vkmeReg.IsMatch(url)) return VKLinkType.WriteVkMe;
            if (writeReg.IsMatch(url)) return VKLinkType.Write;
            if (stickersReg.IsMatch(url)) return VKLinkType.StickerPack;
            if (vkReg.IsMatch(url)) return VKLinkType.ScreenName;
            return VKLinkType.Unknown;
        }

        public static async Task<Tuple<VKLinkType, string>> LaunchLink(this VKSession session, Uri uri) {
            return await LaunchLink(session, uri.AbsoluteUri);
        }

        public static async Task<Tuple<VKLinkType, string>> LaunchLink(this VKSession session, string url) {
            VKLinkType type = GetLinkType(url);
            string id = null;

            var ids = idsReg.Matches(url);
            var snm = screenNameReg.Matches(url);
            var spm = stickersReg.Matches(url);

            Log.Information($"Trying to launch VK link. Type: {type}, link: {url}, ids: {String.Join(',', ids)}, snm: {String.Join(',', snm)}, ");

            switch (type) {
                case VKLinkType.User:
                    id = ids[0].Value;
                    await OpenPeerProfileAsync(session, Int64.Parse(id));
                    break;
                case VKLinkType.Group:
                    id = ids[0].Value;
                    await OpenPeerProfileAsync(session, Int64.Parse(id) * -1);
                    break;
                case VKLinkType.Wall: // TODO: Wallpost viewer in app
                    id = $"{ids[0].Value}_{ids[1].Value}";
                    await Launcher.LaunchUrl(url); // Remove after implementation
                    break;
                case VKLinkType.Poll:
                    await OpenPollViewerAsync(session, Int64.Parse(ids[0].Value), Int32.Parse(ids[1].Value));
                    await Launcher.LaunchUrl(url); // Remove after implementation
                    break;
                case VKLinkType.ConversationInvite:
                    id = url;
                    if (session.GroupId != 0) break; // TODO: открыть окно превью чата в сессии юзера
                    await OpenChatPreviewAsync(session, url);
                    await Launcher.LaunchUrl(url); // Remove after implementation
                    break;
                case VKLinkType.WriteVkMe:
                    id = snm[0].Value;
                    await TryResolveScreenNameAndOpenConvAsync(session, id, url);
                    await Launcher.LaunchUrl(url); // Remove after implementation
                    break;
                case VKLinkType.Write:
                    var wr = writeIdReg.Match(url);
                    id = wr.Value;
                    session.GoToChat(Int64.Parse(id));
                    break;
                case VKLinkType.StickerPack:
                    string packName = spm[0].Groups[4].Value;
                    await OpenStickerPackPreviewAsync(session, packName);
                    await Launcher.LaunchUrl(url); // Remove after implementation
                    break;
                case VKLinkType.ScreenName:
                    id = snm[0].Value;
                    await TryResolveScreenNameAndOpenProfileAsync(session, id, url);
                    await Launcher.LaunchUrl(url); // Remove after implementation
                    break;
                case VKLinkType.Unknown:
                    id = url;
                    await Launcher.LaunchUrl(url);
                    break;
            }
            return new Tuple<VKLinkType, string>(type, id);
        }

        #endregion

        public static async Task OpenPeerProfileAsync(VKSession session, long peerId) {
            if (DemoMode.IsEnabled) return;
            PeerProfile pp = new PeerProfile(session, peerId);
            await pp.ShowDialog(session.ModalWindow);
        }

        public static async Task OpenPollViewerAsync(VKSession session, long ownerId, int id) {
            VKUIDialog alert = new VKUIDialog(Assets.i18n.Resources.not_implemented, Assets.i18n.Resources.not_implemented_desc + $"\n\nOwner: {ownerId}, poll id: {id}");
            await alert.ShowDialog(session.ModalWindow);
        }

        public static async Task OpenChatPreviewAsync(VKSession session, string url) {
            VKUIDialog alert = new VKUIDialog(Assets.i18n.Resources.not_implemented, Assets.i18n.Resources.not_implemented_desc + $"\n\nChat url: {url}");
            await alert.ShowDialog(session.ModalWindow);
        }

        public static async Task OpenStickerPackPreviewAsync(VKSession session, string packName) {
            VKUIDialog alert = new VKUIDialog(Assets.i18n.Resources.not_implemented, Assets.i18n.Resources.not_implemented_desc + $"\n\nStickerpack name: {packName}");
            await alert.ShowDialog(session.ModalWindow);
        }

        public static async Task TryResolveScreenNameAndOpenProfileAsync(VKSession session, string name, string fallbackUrl) {
            VKUIDialog alert = new VKUIDialog(Assets.i18n.Resources.not_implemented, Assets.i18n.Resources.not_implemented_desc + $"\n\nName: {name}\nFallback: {fallbackUrl}");
            await alert.ShowDialog(session.ModalWindow);
        }

        public static async Task TryResolveScreenNameAndOpenConvAsync(VKSession session, string name, string fallbackUrl) {
            VKUIDialog alert = new VKUIDialog(Assets.i18n.Resources.not_implemented, Assets.i18n.Resources.not_implemented_desc + $"\n\nName: {name}\nFallback: {fallbackUrl}");
            await alert.ShowDialog(session.ModalWindow);
        }
    }
}
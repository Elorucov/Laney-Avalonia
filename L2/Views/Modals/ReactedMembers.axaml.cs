using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using ELOR.Laney.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.DataModels;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VKUI.Windows;

namespace ELOR.Laney;

public partial class ReactedMembers : DialogWindow {
    long peerId;
    int cmid;
    VKSession session;

    public ReactedMembers() {
        InitializeComponent();
        if (!Design.IsDesignMode) throw new ArgumentException();
    }

    public ReactedMembers(VKSession session, long peerId, int cmid) {
        InitializeComponent();
        this.FixDialogWindows(TitleBar, Tabs);

        this.peerId = peerId;
        this.cmid = cmid;
        this.session = session;
        Tag = session;
        Activated += ReactedMembers_Activated;
    }

    private void ReactedMembers_Activated(object sender, EventArgs e) {
        Activated -= ReactedMembers_Activated;
        new System.Action(async () => await GetReactedPeersAsync())();
    }

    private async Task GetReactedPeersAsync() {
        try {
            var response = await session.API.Messages.GetReactedPeersAsync(session.GroupId, peerId, cmid);
            List<ReactionGroup> tabs = new List<ReactionGroup> {
                new ReactionGroup(0, response.Count, GetEntities(response.Items, response.Profiles, response.Groups))
            };
            var groups = response.Items.GroupBy(rp => rp.ReactionId).ToList();
            if (groups.Count > 1) {
                foreach (var group in groups) {
                    int count = response.Counters.Where(r => r.ReactionId == group.Key).FirstOrDefault().Count;
                    tabs.Add(new ReactionGroup(group.Key, count, GetEntities(group.ToList(), response.Profiles, response.Groups, true)));
                }
            }

            Tabs.ItemsSource = tabs;
            LoadingIndicator.IsVisible = false;
        } catch (Exception ex) {
            await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex, true);
            Close();
        }
    }

    private List<Entity> GetEntities(List<ReactedMember> reactedPeers, List<User> users, List<Group> groups, bool dontAddReactionIcon = false) {
        List<Entity> entities = new List<Entity>();

        foreach (var rp in reactedPeers) {
            string link = dontAddReactionIcon ? null : CacheManager.GetStaticReactionUrl(rp.ReactionId);
            if (rp.UserId.IsUser()) {
                User user = users?.Where(u => u.Id == rp.UserId).FirstOrDefault();
                if (user != null) {
                    entities.Add(new Entity(user.Id, user.Photo, user.FullName, link, null));
                }
            } else if (rp.UserId.IsGroup()) {
                Group group = groups?.Where(g => g.Id == rp.UserId * -1).FirstOrDefault();
                if (group != null) {
                    entities.Add(new Entity(group.Id, group.Photo, group.Name, link, null));
                }
            }
        }

        return entities;
    }

    private void ContentPresenter_DataContextChanged(object? sender, EventArgs e) {
        ContentPresenter c = sender as ContentPresenter;
        c.Content = null;
        ReactionGroup rg = c.DataContext as ReactionGroup;
        if (rg == null) return;

        if (rg.Item1 == 0) {
            c.Content = new TextBlock { Text = Assets.i18n.Resources.all, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
        } else {
            Image img = new Image { Width = 26, Height = 26 };
            ImageLoader.SetSvgSource(img, new Uri(CacheManager.GetStaticReactionUrl(rg.Item1)));
            c.Content = img;
        }
    }

    private void Image_DataContextChanged(object? sender, EventArgs e) {
        Image img = sender as Image;
        Entity en = img.DataContext as Entity;
        img.Source = null;
        if (en == null || String.IsNullOrEmpty(en.Description)) return;

        ImageLoader.SetSvgSource(img, new Uri(en.Description));
    }

    private void OpenProfile(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        Entity entity = (sender as Control).DataContext as Entity;
        if (entity == null) return;
        Close();
        new System.Action(async () => await Router.OpenPeerProfileAsync(session, entity.Id))();
    }
}
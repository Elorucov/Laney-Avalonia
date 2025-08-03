using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.DataModels;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals;

public partial class WhoReadMessage : DialogWindow {
    long peerId;
    int cmid;
    VKSession session;
    ObservableCollection<Entity> members = new ObservableCollection<Entity>();

    public WhoReadMessage() {
        InitializeComponent();
        if (!Design.IsDesignMode) throw new ArgumentException();
    }

    public WhoReadMessage(VKSession session, long peerId, int cmid) {
        InitializeComponent();
        this.FixDialogWindows(TitleBar, ScrollViewer);

        this.peerId = peerId;
        this.cmid = cmid;
        this.session = session;
        Tag = session;
        MembersList.ItemsSource = members;
        Activated += WhoReadMessage_Activated;
    }

    private void WhoReadMessage_Activated(object sender, EventArgs e) {
        Activated -= WhoReadMessage_Activated;
        new System.Action(async () => await GetMembersWhoReadMessageAsync())();
    }

    private async Task GetMembersWhoReadMessageAsync() {
        try {
            LoadingIndicator.IsVisible = true;
            var response = await session.API.Messages.GetMessageReadPeersAsync(session.GroupId, peerId, cmid, 0, 50, VKAPIHelper.Fields);

            foreach (var id in response.Items) {
                if (id.IsUser()) {
                    User user = response.Profiles.Where(u => u.Id == id).FirstOrDefault();
                    if (user != null) {
                        members.Add(new Entity(user.Id, user.Photo, user.FullName, null, null));
                    }
                } else if (id.IsGroup()) {
                    Group group = response.Groups.Where(g => g.Id == id * -1).FirstOrDefault();
                    if (group != null) {
                        members.Add(new Entity(group.Id, group.Photo, group.Name, null, null));
                    }
                }
            }

            LoadingIndicator.IsVisible = false;
        } catch (Exception ex) {
            await ExceptionHelper.ShowErrorDialogAsync(session.ModalWindow, ex, true);
            Close();
        }
    }

    private void OpenProfile(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        Entity entity = (sender as Control).DataContext as Entity;
        if (entity == null) return;
        Close();
        new System.Action(async () => await Router.OpenPeerProfileAsync(session, entity.Id))();
    }
}
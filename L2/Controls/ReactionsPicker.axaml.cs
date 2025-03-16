using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ELOR.Laney.Core;
using ELOR.Laney.DataModels;
using ELOR.Laney.Helpers;
using Serilog;
using System;
using System.Linq;

namespace ELOR.Laney;

public partial class ReactionsPicker : UserControl {
    Control popupTarget;
    PopupFlyoutBase parentPopup;
    long peerId;
    int cmid;
    int selectedReactionId = 0;

    public ReactionsPicker() {
        InitializeComponent();
    }

    public ReactionsPicker(long peerId, int cmid, int pickedReactionId, Control target, PopupFlyoutBase parent) {
        InitializeComponent();
        this.peerId = peerId;
        this.cmid = cmid;
        selectedReactionId = pickedReactionId;
        popupTarget = target;
        parentPopup = parent;

        Command command = new Command(null, null, false, OnReactionClick);
        var entities = CacheManager.AvailableReactions.Select(r => new Entity(r, new Uri(CacheManager.GetStaticReactionUrl(r)), null, null, command)).ToList();
        ReactionsList.ItemsSource = entities;
    }

    private async void OnReactionClick(object obj) {
        parentPopup?.Hide();

        if (DemoMode.IsEnabled) return;
        if (obj == null || obj is not long) return;

        var session = VKSession.GetByDataContext(popupTarget);
        int picked = Convert.ToInt32(obj);
        bool remove = selectedReactionId == picked;
        try {
            bool response = remove
                ? await session.API.Messages.DeleteReactionAsync(session.GroupId, peerId, cmid)
                : await session.API.Messages.SendReactionAsync(session.GroupId, peerId, cmid, picked);
        } catch (Exception ex) {
            string str = remove ? "remove" : "send";
            Log.Error(ex, $"Failed to {str} reaction to message {peerId}_{cmid}!");
            await ExceptionHelper.ShowErrorDialogAsync(session?.Window, ex, true);
        }
    }

    private void Button_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        Button b = sender as Button;
        Entity entity = b.DataContext as Entity;
        if (entity == null) return;

        if (Convert.ToInt64(selectedReactionId) == entity.Item1) b.Classes.Remove("Tertiary");
    }
}
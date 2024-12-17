using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Svg.Skia;
using ELOR.Laney.Core;
using ELOR.Laney.DataModels;
using ELOR.Laney.Helpers;
using ExCSS;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ELOR.Laney;

public partial class ReactionsPicker : UserControl {
    Control popupTarget;
    PopupFlyoutBase parentPopup;
    long peerId;
    int cmid;
    int selectedReactionId = 0;

    private static List<IImage> _fullyCachedReactions;

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
        var entities = CacheManager.AvailableReactions.Select(r => new ReactionEntity(r, null, null, null, command)).ToList();
        ReactionsList.ItemsSource = entities;

        LoadReactionsAsync(entities);
    }

    private async void LoadReactionsAsync(List<ReactionEntity> entities)
    {
        // apply cached if exist
        if (_fullyCachedReactions != null)
        {
            for (int i = 0; i < CacheManager.AvailableReactions.Count; ++i)
            {
                entities[i].Item2 = _fullyCachedReactions[i];
            }
            return;
        }

        // cache
        _fullyCachedReactions = new List<IImage>();
        // load up reactions async
        for (int i = 0; i < CacheManager.AvailableReactions.Count; ++i)
        {
            var uri = new Uri(CacheManager.GetStaticReactionUrl(CacheManager.AvailableReactions[i]));

            string data = await CacheManager.GetStaticReactionImageAsync(uri);
            if (String.IsNullOrEmpty(data)) return;

            var ent = entities[i];
            var svg = new SvgImage
            {
                Source = SvgSource.LoadFromSvg(data)
            };
            ent.Item2 = svg;
            _fullyCachedReactions.Add(svg);
        }
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
using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using System;
using System.Threading.Tasks;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals;

public partial class ChatLinkViewer : DialogWindow {
    public ChatLinkViewer() {
        InitializeComponent();
        if (!Design.IsDesignMode) throw new ArgumentException();
    }

    private readonly VKSession _session;
    private readonly long _peerId;

    public ChatLinkViewer(VKSession session, long peerId) {
        InitializeComponent();
        this.FixDialogWindows(TitleBar, ContentRoot);
        _session = session;
        _peerId = peerId;

        ShowMessagesCheck.Content = string.Format(Assets.i18n.Resources.chatinvite_show_n_messages, Constants.DefaultVisibleMessagesCount);
    }

    private void SetIsLoading(bool isLoading) {
        ShowMessagesCheck.IsEnabled = !isLoading;
        LinkBox.IsEnabled = !isLoading;
        // CopyButton.IsEnabled = !isLoading;
        RefreshButton.IsEnabled = !isLoading;
        LoadingSpinner.IsVisible = isLoading;
    }

    private async Task GetInviteLinkAsync(bool reset = false) {
        SetIsLoading(true);

        try {
            var response = await _session.API.Messages.GetInviteLinkAsync(_session.GroupId, _peerId,
                ShowMessagesCheck.IsChecked == true ? Constants.DefaultVisibleMessagesCount : 0, reset);

            LinkBox.Text = response.Link;
        } catch (Exception ex) {
            if (await ExceptionHelper.ShowErrorDialogAsync(this, ex, true)) Close();
        } finally {
            SetIsLoading(false);
        }

    }

    private void GetInviteLinkEvent(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        new Action(async () => await GetInviteLinkAsync())();
    }

    private void RefreshLink(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        new Action(async () => await GetInviteLinkAsync(true))();
    }
}
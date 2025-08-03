using Avalonia;
using Avalonia.Controls;
using ELOR.Laney.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.VKAPILib.Objects;
using System;
using System.Collections.Generic;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals;

public partial class StandaloneMessageViewer : DialogWindow {
    public StandaloneMessageViewer() {
        InitializeComponent();
        if (!Design.IsDesignMode) throw new ArgumentException();
    }

    public StandaloneMessageViewer(VKSession session, Message message) {
        InitializeComponent();
        if (message == null) throw new ArgumentNullException("message", "Message not passed!");
#if LINUX      
        TitleBar.IsVisible = false;
#endif

        var ui = new PostUI() {
            Post = message,
            Width = Width - 24, // из-за странного бага с текстами в авалонии, родительский элемент становится шире.
            Margin = new Thickness(0, 0, 0, 24)
        };
        ui.SizeChanged += Ui_SizeChanged;
        ScrollRoot.Content = ui;
    }

    public StandaloneMessageViewer(VKSession session, List<Message> messages) {
        InitializeComponent();
        if (messages == null || messages.Count == 0) throw new ArgumentNullException("messages", "Messages not passed!");
        Title = Localizer.GetDeclensionFormatted(messages.Count, "message");
#if LINUX
        TitleBar.IsVisible = false;
#endif

        foreach (var message in messages) {
            var ui = new PostUI() {
                Post = message,
                Width = Width - 24, // из-за странного бага с текстами в авалонии, родительский элемент становится шире.
            };
            MessagesStack.Children.Add(ui);
        }
    }

    private void Ui_SizeChanged(object sender, SizeChangedEventArgs e) {
        var ui = sender as PostUI;
        if (ui == null) return;

        ui.SizeChanged -= Ui_SizeChanged;
#if LINUX
        if (ui.DesiredSize.Height < MaxHeight) {
            Height = ui.DesiredSize.Height;
#else
        if (ui.DesiredSize.Height < MaxHeight - TitleBar.DesiredSize.Height) {
            Height = ui.DesiredSize.Height + TitleBar.DesiredSize.Height;
#endif
        } else {
            ScrollRoot.Height = MaxHeight - TitleBar.DesiredSize.Height + 12;
        }
    }
}
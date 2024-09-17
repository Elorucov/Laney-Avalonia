using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects;
using ELOR.VKAPILib.Objects.Messages;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ELOR.Laney;

public partial class GroupMessageTemplates : UserControl {
    VKSession session;

    public GroupMessageTemplates(VKSession session, User user, User admin, string groupName) {
        InitializeComponent();
        this.session = session;

        List<MessageTemplate> templates = new List<MessageTemplate>();
        foreach (MessageTemplate template in CollectionsMarshal.AsSpan(session.MessageTemplates)) {
            MessageTemplate t = (MessageTemplate)(template.Clone());
            t.Text = NormalizeTemplate(template.Text, user, admin, groupName);
            templates.Add(t);
        }

        TemplatesList.ItemsSource = templates;
    }

    public event EventHandler<string> TemplateSelected;

    private void OnTemplateClicked(object sender, RoutedEventArgs e) {
        Button button = sender as Button;
        if (button.DataContext != null && button.DataContext is MessageTemplate template) {
            TemplateSelected?.Invoke(this, template.Text);
        }
    }

    private void OpenTemplatesEditorModal(object? sender, RoutedEventArgs e) {
        ExceptionHelper.ShowNotImplementedDialogAsync(session.ModalWindow);
    }

    private string NormalizeTemplate(string template, User user, User admin, string groupName) {
        string uf = user.FirstName;
        string ul = user.LastName;
        string af = admin.FirstName;
        string al = admin.LastName;
        return template.Replace("{user name}", uf).Replace("{user surname}", ul)
            .Replace("{admin name}", af).Replace("{admin surname}", al)
            .Replace("{community}", groupName).Replace("{greeting}", Assets.i18n.Resources.commtemplate_greeting);
    }
}
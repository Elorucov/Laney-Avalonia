using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects.Messages;
using System;

namespace ELOR.Laney;

public partial class GroupMessageTemplates : UserControl {
    VKSession session;

    public GroupMessageTemplates(VKSession session) {
        InitializeComponent();
        this.session = session;
        TemplatesList.ItemsSource = session.MessageTemplates;
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
}
using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.ViewModels.Modals;
using System;
using System.Linq;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals;

public partial class GroupsPicker : DialogWindow {
    public GroupsPicker() {
        InitializeComponent();
        if (!Design.IsDesignMode) throw new ArgumentException();
    }

    public GroupsPicker(VKSession session) {
        InitializeComponent();
#if LINUX
            TitleBar.IsVisible = false;
#endif

        DataContext = new GroupsPickerViewModel(session);
    }

    private GroupsPickerViewModel ViewModel { get { return DataContext as GroupsPickerViewModel; } }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        var selectedIds = ViewModel.SelectedGroups.SelectedItems.Select(g => g.Id).ToList();
        Close(selectedIds);
    }
}
using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.DataModels;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib;
using Serilog;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using VKUI.Controls;
using VKUI.Popups;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals;

public partial class ChatEditor : DialogWindow {
    enum ChatEditorMode {
        ChatEditor, PermissionEditorForNewChat
    }

    public ChatEditor() {
        InitializeComponent();
        if (!Design.IsDesignMode) throw new ArgumentException();
    }

    private readonly ChatEditorMode _mode;
    private readonly VKSession _session;
    private readonly int _chatId;
    private readonly string _name;
    private readonly string _description;
    private readonly string _photo;
    private readonly Dictionary<string, string> _permissions;

    // Нужно, т. к. фото меняется сразу, без закрытия окна нажатием на "save", 
    // а тот кто открыл это окно должен узнать актуальное фото, даже если юзер сменил фото и закрыл окно нажатием на крестик
    public string Photo => _photo;

    public ChatEditor(VKSession session, int chatId, string name, string description, string photo, Dictionary<string, string> permissions) {
        InitializeComponent();
        _mode = ChatEditorMode.ChatEditor;
        _session = session;
        _chatId = chatId;
        _name = name;
        _description = description;
        _photo = photo;
        _permissions = permissions;

        Setup();
    }

    private void Setup() {
        if (_mode == ChatEditorMode.ChatEditor) {
            ChatName.Text = _name;
            ChatDescription.Text = _description;

            if (!string.IsNullOrEmpty(_photo) && Uri.IsWellFormedUriString(_photo, UriKind.Absolute))
                ChatAvatar.SetImage(new Uri(_photo));
        }

        SetupPermissions(_permissions);
    }

    private void SetupPermissions(Dictionary<string, string> permissions) {
        if (permissions == null) return;
        PermissionsList.IsVisible = true;

        foreach (var permission in permissions) {
            List<string> availableValues = ["owner", "owner_and_admins"];
            if (permission.Key != "change_admins") availableValues.Add("all");
            PermissionsList.Children.Add(CreatePermissionButton(permission.Key, availableValues));
        }
    }

    private Button CreatePermissionButton(string key, List<string> availableValues) {
        string currentValue = _permissions[key];

        Cell cell = new Cell {
            After = new VKIcon { Id = VKIconNames.Icon20Dropdown },
            Header = Localizer.Get($"chat_permission_key_{key}"),
            Subtitle = Localizer.Get($"chat_permission_value_{currentValue}")
        };

        ActionSheet ash = new ActionSheet {
            Tag = key
        };

        foreach (var value in CollectionsMarshal.AsSpan(availableValues)) {
            ActionSheetItem item = new ActionSheetItem {
                Header = Localizer.Get($"chat_permission_value_{value}"),
                Tag = value
            };

            item.Click += (a, b) => {
                _permissions[key] = value;
                cell.Subtitle = Localizer.Get($"chat_permission_value_{value}");
            };

            ash.Items.Add(item);
        }

        var button = new Button {
            Content = cell,
            Tag = ash
        };

        button.Classes.Add("Tertiary");
        button.Click += OnPermissionButtonClick;

        return button;
    }

    private void OnPermissionButtonClick(object sender, RoutedEventArgs e) {
        Button button = sender as Button;
        ActionSheet ash = button.Tag as ActionSheet;

        string key = ash.Tag.ToString();
        string currentValue = _permissions[key];

        foreach (var item in CollectionsMarshal.AsSpan(ash.Items)) {
            string value = item.Tag.ToString();
            item.Before = new VKIcon {
                Id = value == currentValue ? VKIconNames.Icon20Check : null,
                Width = value == currentValue ? double.NaN : 20
            };
        }

        ash.ShowAt(button);
    }

    private void OnAvatarClick(object? sender, RoutedEventArgs e) {
        ActionSheet ash = new ActionSheet();

        ActionSheetItem change = new ActionSheetItem {
            Before = new VKIcon { Id = VKIconNames.Icon20WriteOutline },
            Header = Assets.i18n.Resources.chat_editor_change_photo
        };
        change.Click += ShowUCModal;

        ActionSheetItem generate = new ActionSheetItem {
            Before = new VKIcon { Id = VKIconNames.Icon20Stars },
            Header = Assets.i18n.Resources.chat_editor_generate_photo
        };
        generate.Click += ShowUCModal;

        ActionSheetItem delete = new ActionSheetItem {
            Before = new VKIcon { Id = VKIconNames.Icon20DeleteOutline },
            Header = Assets.i18n.Resources.chat_editor_delete_photo
        };
        delete.Classes.Add("Destructive");
        delete.Click += ShowUCModal;

        ash.Items.Add(change);
        ash.Items.Add(generate);
        ash.Items.Add(delete);

        ash.ShowAt(sender as Button);
    }

    private void ShowUCModal(object sender, RoutedEventArgs e) {
        ExceptionHelper.ShowNotImplementedDialog(this);
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e) {
        if (string.IsNullOrEmpty(ChatName.Text)) {
            DataValidationErrors.SetError(ChatName, new ApplicationException("Required"));
            ChatName.Focus();
            return;
        }

        new Action(async () => {
            Button button = sender as Button;
            string newName = ChatName.Text;
            string newDesc = ChatDescription.Text;

            string permissions = JsonSerializer.Serialize(_permissions, new JsonSerializerOptions {
                TypeInfoResolver = BuildInJsonContext.Default
            });

            if (_mode == ChatEditorMode.ChatEditor) {
                bool nameChanged = _name != ChatName.Text;
                bool descChanged = _description != ChatDescription.Text;

                Log.Information("{0}: mode={1}, nameChanged={2}, descChanged={3}, permissions={4}", nameof(ChatEditor), _mode, nameChanged, descChanged, permissions);

                try {
                    button.IsEnabled = false;
                    var response = await _session.API.Messages.EditChatAsync(_chatId, nameChanged ? newName : null, descChanged ? newDesc : null, permissions);
                    Close(new ChatEditorResult(_chatId, ChatName.Text, ChatDescription.Text, _permissions));
                } catch (Exception ex) {
                    button.IsEnabled = true;
                    if (await ExceptionHelper.ShowErrorDialogAsync(this, ex)) OnSaveClick(sender, e);
                }
            } else {
                Log.Information("{0}: mode={1}, permissions={2}", nameof(ChatEditor), _mode, permissions);
                Close(new ChatEditorResult(_chatId, null, null, _permissions));
            }
        })();
    }
}
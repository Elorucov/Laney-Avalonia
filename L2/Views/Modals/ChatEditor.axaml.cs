using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Core.Network;
using ELOR.Laney.DataModels;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib;
using ELOR.VKAPILib.Objects;
using ELOR.VKAPILib.Objects.Messages;
using Serilog;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using VKUI.Controls;
using VKUI.Popups;
using VKUI.Windows;

namespace ELOR.Laney.Views.Modals;

public partial class ChatEditor : DialogWindow {
    enum ChatEditorMode {
        ChatEditor, PermissionEditor
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
    private string _photo;
    private readonly Dictionary<string, string> _permissions;
    private readonly ChatACL _acl;
    private readonly bool _serviceMessagesDisabled;

    private Exception _uploaderException;

    // Нужно, т. к. фото меняется сразу, без закрытия окна нажатием на "save", 
    // а тот кто открыл это окно должен узнать актуальное фото, даже если юзер сменил фото и закрыл окно нажатием на крестик
    public string Photo => _photo;

    public ChatEditor(VKSession session, int chatId, string name, string description, string photo, Dictionary<string, string> permissions, ChatACL acl, bool serviceMessagesDisabled) {
        InitializeComponent();
        _mode = ChatEditorMode.ChatEditor;
        _session = session;
        _chatId = chatId;
        _name = name;
        _description = description;
        _photo = photo;
        _permissions = permissions;
        _acl = acl;
        _serviceMessagesDisabled = serviceMessagesDisabled;

        ChatSettingsList.MaxHeight = 236;
        Setup();
    }

    public ChatEditor(VKSession session, Dictionary<string, string> permissions) {
        InitializeComponent();
        _mode = ChatEditorMode.PermissionEditor;
        _session = session;
        _permissions = permissions;

        Setup();
    }

    private void Setup() {
        if (_mode == ChatEditorMode.ChatEditor) {
            ChatMainInfos.IsVisible = true;
            ChatName.Text = _name;
            ChatDescription.Text = _description;

            if (!string.IsNullOrEmpty(_photo) && Uri.IsWellFormedUriString(_photo, UriKind.Absolute))
                ChatAvatar.SetImage(new Uri(_photo));
        }

        SetupPermissions(_permissions);
    }

    private void SetupPermissions(Dictionary<string, string> permissions) {
        if (permissions == null) return;
        ChatSettingsList.IsVisible = true;

        foreach (var permission in permissions) {
            List<string> availableValues = ["owner", "owner_and_admins"];
            if (permission.Key != "change_admins") availableValues.Add("all");
            PermissionsListStack.Children.Add(CreatePermissionButton(permission.Key, availableValues));
        }

        if (_acl != null) {
            if (_acl.CanDisableForwardMessages) {
                ForwardTC.IsVisible = true;
                ForwardTS.IsChecked = !_acl.CanForwardMessages;
            }
            if (_acl.CanDisableServiceMessages) {
                ServiceMsgsTC.IsVisible = true;
                ServiceMsgsTS.IsChecked = !_serviceMessagesDisabled;
            }
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
        change.Click += ShowFilePicker;

        //ActionSheetItem generate = new ActionSheetItem {
        //    Before = new VKIcon { Id = VKIconNames.Icon20Stars },
        //    Header = Assets.i18n.Resources.chat_editor_generate_photo
        //};
        //generate.Click += ShowUCModal;

        ActionSheetItem delete = new ActionSheetItem {
            Before = new VKIcon { Id = VKIconNames.Icon20DeleteOutline },
            Header = Assets.i18n.Resources.chat_editor_delete_photo
        };
        delete.Classes.Add("Destructive");
        delete.Click += DeleteChatPhoto;

        // // сообщество может обновить фото только с помощью токена сообщества, у метода photos.getChatUploadServer нет параметра group_id.
        if (!_session.IsGroup) {
            ash.Items.Add(change);
            // ash.Items.Add(generate);
        }
        ash.Items.Add(delete);

        ash.ShowAt(sender as Button);
    }

    private void ShowFilePicker(object sender, RoutedEventArgs e) {
        new System.Action(async () => {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.ImageAll }
            });

            if (files.Count > 0) {
                await ChangePhotoAsync(files[0]);
            }
        })();
    }

    private async Task ChangePhotoAsync(IStorageFile file) {
        VKUIWaitDialog<SetChatPhotoResponse> wd = new VKUIWaitDialog<SetChatPhotoResponse>();
        try {
            var response = await wd.ShowAsync(this, ChangePhotoInternalAsync(file));
            _photo = response.Chat.Photo;
            if (!string.IsNullOrEmpty(_photo) && Uri.IsWellFormedUriString(_photo, UriKind.Absolute))
                ChatAvatar.SetImage(new Uri(_photo));
        } catch (Exception ex) {
            if (await ExceptionHelper.ShowErrorDialogAsync(this, ex)) await ChangePhotoAsync(file);
        }
    }

    private async Task<SetChatPhotoResponse> ChangePhotoInternalAsync(IStorageFile file) {
        var server = await _session.API.Photos.GetChatUploadServerAsync(_chatId);
        Log.Information("{0}: got upload server: {1}", nameof(ChatEditor), server.Uri);

        var uploader = new VKHttpClientFileUploader("file", server.Uri, file);
        uploader.UploadFailed += Uploader_UploadFailed;

        var uploaderResponse = await uploader.UploadAsync();
        Log.Information("{0}: response from upload server: {1}", nameof(ChatEditor), uploaderResponse);
        if (uploaderResponse == null) throw _uploaderException ?? new ArgumentNullException("Uploaded without errors, but server returns no response!");

        JsonNode uploaderResponseJson = JsonNode.Parse(uploaderResponse);
        if (uploaderResponseJson["response"] == null) throw new ApplicationException("Server doesn't return success result!");

        return await _session.API.Messages.SetChatPhotoAsync(uploaderResponseJson["response"].GetValue<string>());
    }

    private void Uploader_UploadFailed(object sender, Exception e) {
        _uploaderException = e;
        Log.Error(e, "{0}: an error occurred while uploading a new chat photo for chat {1}", nameof(ChatEditor), _chatId);
    }

    private void DeleteChatPhoto(object sender, RoutedEventArgs e) {
        new System.Action(async () => {
            VKUIWaitDialog<SetChatPhotoResponse> wd = new VKUIWaitDialog<SetChatPhotoResponse>();
            try {
                var response = await wd.ShowAsync(this, _session.API.Messages.DeleteChatPhotoAsync(_session.GroupId, _chatId));
                _photo = null;
                ChatAvatar.Image = null;
            } catch (Exception ex) {
                if (await ExceptionHelper.ShowErrorDialogAsync(this, ex)) DeleteChatPhoto(sender, e);
            }
        })();
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e) {
        if (_mode == ChatEditorMode.ChatEditor && string.IsNullOrEmpty(ChatName.Text)) {
            DataValidationErrors.SetError(ChatName, new ApplicationException("Required"));
            ChatName.Focus();
            return;
        }

        new System.Action(async () => {
            Button button = sender as Button;
            string newName = ChatName.Text;
            string newDesc = ChatDescription.Text;

            string permissions = JsonSerializer.Serialize(_permissions, new JsonSerializerOptions {
                TypeInfoResolver = BuildInJsonContext.Default
            });

            if (_mode == ChatEditorMode.ChatEditor) {
                bool nameChanged = _name != ChatName.Text;
                bool descChanged = _description != ChatDescription.Text;
                bool msgForwardChanged = _acl != null && _acl.CanDisableForwardMessages && (_acl.CanForwardMessages == ForwardTS.IsChecked.Value);
                bool serviceMsgsChanged = _acl != null && _acl.CanDisableServiceMessages && (_serviceMessagesDisabled == ServiceMsgsTS.IsChecked.Value);

                Log.Information("{0}: mode={1}, nameChanged={2}, descChanged={3}, permissions={4}, disableForwarding={5}", 
                    nameof(ChatEditor), _mode, nameChanged, descChanged, permissions, ForwardTS.IsChecked);

                try {
                    button.IsEnabled = false;
                    var response = await _session.API.Messages.EditChatAsync(_chatId, nameChanged ? newName : null, descChanged ? newDesc : null, 
                        permissions, msgForwardChanged ? ForwardTS.IsChecked : null, serviceMsgsChanged ? !ServiceMsgsTS.IsChecked : null);

                    if (msgForwardChanged) _acl.CanForwardMessages = !ForwardTS.IsChecked.Value;
                    Close(new ChatEditorResult(_chatId, ChatName.Text, ChatDescription.Text, _permissions, _acl, !ServiceMsgsTS.IsChecked.Value));
                } catch (Exception ex) {
                    button.IsEnabled = true;
                    if (await ExceptionHelper.ShowErrorDialogAsync(this, ex)) OnSaveClick(sender, e);
                }
            } else {
                Log.Information("{0}: mode={1}, permissions={2}", nameof(ChatEditor), _mode, permissions);
                Close(new ChatEditorResult(_chatId, null, null, _permissions, _acl, false));
            }
        })();
    }
}
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.ViewModels.Controls;
using ELOR.Laney.Views.Modals;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ELOR.Laney.Controls {
    public partial class Composer : UserControl {
        private ComposerViewModel ViewModel { get { return DataContext as ComposerViewModel; } }
        private Tuple<int, int> OldSelectionRange = null;

        public Composer() {
            InitializeComponent();
        }

        // Костыль для сохранения выделения в тексте сообщения после потери фокуса.
        private async void MessageText_PropertyChanged(object sender, Avalonia.AvaloniaPropertyChangedEventArgs e) {
            if (e.Property == TextBox.SelectionStartProperty || e.Property == TextBox.SelectionEndProperty) {
                await Task.Delay(10);
                OldSelectionRange = new Tuple<int, int>(MessageText.SelectionStart, MessageText.SelectionEnd);
            }
        }

        // Костыль для сохранения выделения в тексте сообщения после потери фокуса.
        private void MessageText_LostFocus(object sender, RoutedEventArgs e) {
            if (OldSelectionRange != null && OldSelectionRange.Item1 != OldSelectionRange.Item2) {
                ViewModel.TextSelectionStart = OldSelectionRange.Item1;
                ViewModel.TextSelectionEnd = OldSelectionRange.Item2;
            }

            MessageText.SelectionStart = ViewModel.TextSelectionStart;
            MessageText.SelectionEnd = ViewModel.TextSelectionEnd;
        }

        private async void MessageText_KeyUp(object? sender, KeyEventArgs e) {
            Debug.WriteLine($"KeyUp: {e.Key}; Modifiers: {e.KeyModifiers}");
            if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.V) {
                var window = VKSession.GetByDataContext(this).ModalWindow;

                var formats = await window.Clipboard.GetFormatsAsync();
                await new VKUIDialog("Clipboard", String.Join(", ", formats)).ShowDialog<int>(window);
                e.Handled = true;
            }
        }

        private void MessageText_KeyDown(object sender, KeyEventArgs e) {
            Debug.WriteLine($"KeyDown: {e.Key}; Modifiers: {e.KeyModifiers}");

            if (e.Key == Key.Enter) {
                if (!Settings.SentViaEnter) {
                    if (e.KeyModifiers == KeyModifiers.Control && ViewModel.CanSendMessage && !ViewModel.IsLoading) {
                        e.Handled = true;
                        ViewModel.SendMessage();
                    } else {
                        InsertNewLine();
                    }
                } else {
                    if (e.KeyModifiers != KeyModifiers.Shift && ViewModel.CanSendMessage && !ViewModel.IsLoading) {
                        e.Handled = true;
                        ViewModel.SendMessage();
                    } else {
                        InsertNewLine();
                    }
                }
            }
        }

        // Костыль для ручного ввода символа новой строки,
        // ибо при AcceptsReturn = true не срабатывает KeyDown при нажатии на Enter.
        private void InsertNewLine() {
            try {
                if (MessageText.SelectionStart == MessageText.SelectionEnd) {
                    if (!String.IsNullOrEmpty(MessageText.Text)) {
                        MessageText.Text = MessageText.Text.Insert(MessageText.SelectionEnd, "\n");
                    } else {
                        MessageText.Text = "\n";
                    }
                    MessageText.SelectionStart += 1;
                    MessageText.SelectionEnd += 1;
                } else {
                    int start = Math.Min(MessageText.SelectionStart, MessageText.SelectionEnd);
                    int end = Math.Max(MessageText.SelectionStart, MessageText.SelectionEnd);
                    string newText = MessageText.Text.Remove(start, end - start);
                    MessageText.Text = newText.Insert(start, "\n");
                    start += 1;
                    MessageText.SelectionStart = start;
                    MessageText.SelectionEnd = start;
                }
            } catch (Exception ex) {
                Log.Error($"WTF with Composer.InsertNewLine... 0x{ex.HResult.ToString("x8")}");
            }
        }

        private void BotKbdButton_Click(object sender, RoutedEventArgs e) {
            BotKeyboardToggle.IsChecked = !BotKeyboardToggle.IsChecked;
        }

        private void ShowTemplatesFlyout(object sender, RoutedEventArgs e) {
            ViewModel.ShowGroupTemplates(sender as Button);
        }

        private void ShowStickersFlyout(object sender, RoutedEventArgs e) {
            ViewModel.ShowEmojiStickerPicker(sender as Button);
        }

        private void ShowAttachmentPickerContextMenu(object sender, RoutedEventArgs e) {
            ViewModel.ShowAttachmentPickerContextMenu(sender as Button);
        }

        private void RemoveAttachment(object sender, RoutedEventArgs e) {
            OutboundAttachmentViewModel oavm = (sender as Button).DataContext as OutboundAttachmentViewModel;
            if (oavm != null) {
                oavm.CancelUpload();
                ViewModel.Attachments.Remove(oavm);
            }
        }

        private void ShowAttachmentErrorInfo(object sender, RoutedEventArgs e) {
            OutboundAttachmentViewModel oavm = (sender as Button).DataContext as OutboundAttachmentViewModel;
            if (oavm != null) {
                Exception ex = oavm.UploadException;

                // Проблема в том, что проект не компилируется, если метод,
                // название которого прописано в XAML-е в свойстве Click, асинхронный
                // т. е. работает только private void ShowAttachmentErrorInfo,
                // а не private async void ShowAttachmentErrorInfo
                ShowAttachmentErrorInfo(oavm, ex);
            }
        }

        private async void ShowAttachmentErrorInfo(OutboundAttachmentViewModel oavm, Exception ex) {
            string[] buttons = new string[] { Assets.i18n.Resources.retry, Assets.i18n.Resources.delete };

            VKUIDialog dlg = new VKUIDialog(Assets.i18n.Resources.upload_error, ex.Message, buttons, 1);
            int id = await dlg.ShowDialog<int>(VKSession.GetByDataContext(this).Window);
            switch (id) {
                case 1:
                    oavm.DoUploadFile();
                    break;
                case 2:
                    ViewModel.Attachments.Remove(oavm);
                    break;
            }
        }
    }
}
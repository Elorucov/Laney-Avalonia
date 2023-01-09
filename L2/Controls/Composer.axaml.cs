using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.ViewModels.Controls;
using ELOR.Laney.Views.Modals;
using System;
using System.Collections.Generic;

namespace ELOR.Laney.Controls {
    public partial class Composer : UserControl {
        private ComposerViewModel ViewModel { get { return DataContext as ComposerViewModel; } }

        public Composer() {
            InitializeComponent();
        }

        private void BotKbdButton_Click(object sender, RoutedEventArgs e) {
            BotKeyboardToggle.IsChecked = !BotKeyboardToggle.IsChecked;
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
            string[] buttons = new string[] { Localizer.Instance["retry"], Localizer.Instance["delete"] };

            VKUIDialog dlg = new VKUIDialog(Localizer.Instance["upload_error"], ex.Message, buttons, 1);
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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using System;
using System.Runtime.InteropServices;

namespace VKUI.Controls {
    public class WindowTitleBar : TemplatedControl {
        #region Properties

        #endregion

        #region Template elements

        Grid TitleBar;
        TextBlock WindowTitle;
        Border DragArea;
        Button CloseButton;
        Window OwnerWindow;

        bool isTemplateLoaded = false;
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);

            TitleBar = e.NameScope.Find<Grid>(nameof(TitleBar));
            WindowTitle = e.NameScope.Find<TextBlock>(nameof(WindowTitle));
            DragArea = e.NameScope.Find<Border>(nameof(DragArea));
            CloseButton = e.NameScope.Find<Button>(nameof(CloseButton));

            CloseButton.Click += CloseButton_Click;
            DetachedFromVisualTree += WindowTitleBar_DetachedFromVisualTree;

            isTemplateLoaded = true;
            Setup();
        }

        #endregion

        private void CloseButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            OwnerWindow.Close();
        }

        private void WindowTitleBar_DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
            OwnerWindow.PropertyChanged -= OwnerWindow_PropertyChanged;
            CloseButton.Click -= CloseButton_Click;
            DetachedFromVisualTree -= WindowTitleBar_DetachedFromVisualTree;
        }

        private void Setup() {
            if (!isTemplateLoaded) return;

            // Finding window
            // TODO: чекать, это DialogWindow или обычный Window
            // и менять стиль кнопок в зависимости от этого.
            IControl control = Parent;
            do {
                if (control is Window window) {
                    OwnerWindow = window;
                } else {
                    control = control.Parent;
                }
            } while (OwnerWindow == null && control.GetType() != typeof(Window));
            if (OwnerWindow == null) throw new ArgumentNullException("Unable to find a parent Window!");

            // Appearance
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                TitleBar.Height = 32;
                WindowTitle.Classes.Add("Windows");
                CloseButton.IsVisible = true;
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                TitleBar.Height = 22;
                WindowTitle.Classes.Add("Mac");
            } else {
                TitleBar.IsVisible = false;
            }

            // Window
            WindowTitle.Text = OwnerWindow.Title;
            OwnerWindow.PropertyChanged += OwnerWindow_PropertyChanged;
            DragArea.PointerPressed += DragArea_PointerPressed;
        }

        private void OwnerWindow_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e) {
            if (e.Property.Name == nameof(Window.Title)) {
                WindowTitle.Text = OwnerWindow.Title;
            }
        }

        private void DragArea_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e) {
            OwnerWindow.PlatformImpl.BeginMoveDrag(e);
        }
    }
}
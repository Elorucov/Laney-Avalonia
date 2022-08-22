using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ELOR.Laney.ViewModels.Controls;
using System;

namespace ELOR.Laney.Controls {
    public class ChatViewItem : TemplatedControl {
        #region Properties

        public static readonly StyledProperty<object> ElementProperty =
            AvaloniaProperty.Register<ChatViewItem, object>(nameof(Element));

        public object Element {
            get => GetValue(ElementProperty);
            set => SetValue(ElementProperty, value);
        }

        #endregion

        #region Template elements

        Border ChatViewItemRoot;

        #endregion

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            ChatViewItemRoot = e.NameScope.Find<Border>(nameof(ChatViewItemRoot));
            RenderElement();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);

            if (change.Property == ElementProperty) {
                RenderElement();
            }
        }

        private void RenderElement() {
            if (ChatViewItemRoot == null) return;
            if (Element is MessageViewModel msg) {
                ChatViewItemRoot.Child = new MessageBubble { Message = msg };
            } else if (Element is DateTime dt) {
                ChatViewItemRoot.Child = new TextBlock { Text = dt.ToString() };
            } else {
                ChatViewItemRoot.Child = null;
            }
        }
    }
}
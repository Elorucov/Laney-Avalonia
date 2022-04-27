using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using System;

namespace VKUI.Controls {
    public sealed class Spinner : TemplatedControl {
        public Spinner() { }

        #region Template elements

        PathIcon Icon;

        #endregion

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            Icon = e.NameScope.Find<PathIcon>(nameof(Icon));

            SetupSpinner(Math.Min(DesiredSize.Width, DesiredSize.Height));

            PropertyChanged += Spinner_PropertyChanged;
            DetachedFromVisualTree += Spinner_DetachedFromVisualTree;
        }

        double oldSize = 0;

        private void Spinner_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
            if (e.Property == BoundsProperty) {
                double value = Math.Min(Bounds.Width, Bounds.Height);
                if (oldSize == value) return;
                SetupSpinner(value);
                oldSize = value;
            }
        }

        private void Spinner_DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
            PropertyChanged -= Spinner_PropertyChanged;
            DetachedFromVisualTree -= Spinner_DetachedFromVisualTree;
        }

        private void Spinner_EffectiveViewportChanged(object? sender, Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            SetupSpinner(Math.Min(e.EffectiveViewport.Width, e.EffectiveViewport.Height));
        }

        private void SetupSpinner(double s) {
            int size = 16;
            if (s >= 20) size = 24;
            if (s >= 28) size = 32;
            if (s >= 38) size = 44;

            Geometry spinner = VKUITheme.Icons[$"Icon{size}Spinner"] as Geometry;
            Icon.Data = spinner;
            Icon.Width = s;
            Icon.Height = s;
        }
    }
}
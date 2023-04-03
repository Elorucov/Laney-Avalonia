using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;

namespace VKUI.Controls {
    public sealed class Spinner : TemplatedControl {
        public Spinner() { }

        #region Template elements

        VKIcon Icon;

        #endregion

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            Icon = e.NameScope.Find<VKIcon>(nameof(Icon));

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
            } else if (e.Property == IsVisibleProperty) {
                PseudoClasses.Set(":animated", IsVisible);
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
            string iconId = VKIconNames.Icon16Spinner;
            if (s >= 20) iconId = VKIconNames.Icon24Spinner;
            if (s >= 28) iconId = VKIconNames.Icon32Spinner;
            if (s >= 38) iconId = VKIconNames.Icon44Spinner;

            Icon.Id = iconId;
            Icon.Width = s;
            Icon.Height = s;
        }
    }
}
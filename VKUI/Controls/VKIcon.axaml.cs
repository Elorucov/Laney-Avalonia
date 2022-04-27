using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using System.Text.RegularExpressions;

namespace VKUI.Controls {
    public sealed class VKIcon : TemplatedControl {
        public VKIcon() { }

        #region Template elements

        bool isTemplateLoaded = false;
        Viewbox viewBox;
        Path path;

        #endregion

        #region Properties

        public static readonly StyledProperty<string> IdProperty =
            AvaloniaProperty.Register<VKIcon, string>(nameof(Id));

        public string Id {
            get => GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        #endregion

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            viewBox = e.NameScope.Find<Viewbox>(nameof(viewBox));
            path = e.NameScope.Find<Path>(nameof(path));
            isTemplateLoaded = true;

            DrawIcon(Id);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change) {
            base.OnPropertyChanged(change);
            if (!isTemplateLoaded) return;

            if (change.Property == IdProperty) DrawIcon(Id);
        }

        private void DrawIcon(string id) {
            path.IsVisible = false;
            if (String.IsNullOrEmpty(id)) return;

            viewBox.Stretch = Stretch.Uniform;
            Regex regex = new Regex(@"Icon(\d*)");
            MatchCollection matches = regex.Matches(id);
            if (matches.Count > 0) {
                string size = matches[0].Value.Substring(4);
                path.Width = path.Height = Double.Parse(size);
                if (Double.IsNaN(Width) || Double.IsNaN(Height)) viewBox.Stretch = Stretch.None;
                Geometry icon = VKUITheme.Icons[id] as Geometry;
                if (icon != null) {
                    path.Data = icon;
                    path.IsVisible = true;
                }
            }
        }
    }
}

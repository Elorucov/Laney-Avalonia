using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System;

namespace VKUI.Controls {
    public sealed class ActionSheetItem : TemplatedControl {
        public ActionSheetItem() { }

        #region Properties

        public static readonly StyledProperty<Control> BeforeProperty =
            AvaloniaProperty.Register<ActionSheetItem, Control>(nameof(Before));

        public static readonly StyledProperty<string> HeaderProperty =
            AvaloniaProperty.Register<ActionSheetItem, string>(nameof(Header));

        public static readonly StyledProperty<string> SubtitleProperty =
            AvaloniaProperty.Register<ActionSheetItem, string>(nameof(Subtitle));

        public Control Before {
            get => GetValue(BeforeProperty);
            set => SetValue(BeforeProperty, value);
        }

        public string Header {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public string Subtitle {
            get => GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public event EventHandler<RoutedEventArgs> Click;

        #endregion

        #region Template elements

        bool isTemplateLoaded = false;
        Button Root;
        ContentPresenter BeforeContainer;
        TextBlock HeaderText;
        TextBlock SubtitleText;
        PathIcon CheckedIcon;

        #endregion

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            Root = e.NameScope.Find<Button>(nameof(Root));
            BeforeContainer = e.NameScope.Find<ContentPresenter>(nameof(BeforeContainer));
            HeaderText = e.NameScope.Find<TextBlock>(nameof(HeaderText));
            SubtitleText = e.NameScope.Find<TextBlock>(nameof(SubtitleText));
            CheckedIcon = e.NameScope.Find<PathIcon>(nameof(CheckedIcon));

            isTemplateLoaded = true;
            Root.Click += HandleClick;
            // Unloaded += ActionSheetItem_Unloaded;

            SetBeforeContent(Before, BeforeContainer);
            CheckVisibility(Header, HeaderText);
            CheckVisibility(Subtitle, SubtitleText);
        }

        // After opening ActionSheet 2-nd time, HandleClick not working because Unloaded called after hiding AS 1-st time.
        //private void ActionSheetItem_Unloaded(object sender, RoutedEventArgs e) {
        //    Root.Click -= HandleClick;
        //    Unloaded -= ActionSheetItem_Unloaded;
        //}

        private void HandleClick(object? sender, RoutedEventArgs e) {
            Click?.Invoke(this, e);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);
            if (!isTemplateLoaded) return;

            if (change.Property == BeforeProperty) {
                SetBeforeContent(Before, BeforeContainer);
            } else if (change.Property == HeaderProperty) {
                CheckVisibility(Header, HeaderText);
            } else if (change.Property == SubtitleProperty) {
                CheckVisibility(Subtitle, SubtitleText);
            }
        }

        private void SetBeforeContent(Control before, ContentPresenter presenter) {
            if (before == null) {
                presenter.IsVisible = false;
                return;
            }

            presenter.IsVisible = true;
            if (before is VKIcon icon) {
                string iconClassName = Classes.Contains("Destructive") ? "ActionSheetItemBeforeIconDestructive" : "ActionSheetItemBeforeIcon";
                if (!icon.Classes.Contains(iconClassName)) icon.Classes.Insert(0, iconClassName);
                presenter.Content = icon;
            } else if (before is Avatar avatar) {
                presenter.Content = avatar;
            } else {
                presenter.IsVisible = false;
                throw new ArgumentException("The value must be VKIcon or Avatar", nameof(Before));
            }
        }

        private void CheckVisibility(string text, TextBlock textBlock) {
            textBlock.IsVisible = !String.IsNullOrEmpty(text);
        }
    }
}

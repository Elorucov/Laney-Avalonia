using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace VKUI.Controls {
    public sealed class PanelHeader : TemplatedControl {
        public PanelHeader() { }

        #region Properties

        public static readonly StyledProperty<object> ContentProperty =
            AvaloniaProperty.Register<PanelHeader, object>(nameof(Content));

        public static readonly StyledProperty<bool> IsSeparatorVisibleProperty =
            AvaloniaProperty.Register<PanelHeader, bool>(nameof(IsSeparatorVisible));

        private ObservableCollection<Button> _leftButtons = new ObservableCollection<Button>();
        private ObservableCollection<Button> _rightButtons = new ObservableCollection<Button>();

        public object Content {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public bool IsSeparatorVisible {
            get => GetValue(IsSeparatorVisibleProperty);
            set => SetValue(IsSeparatorVisibleProperty, value);
        }

        public ObservableCollection<Button> LeftButtons {
            get => _leftButtons;
        }

        public ObservableCollection<Button> RightButtons {
            get => _rightButtons;
        }

        #endregion

        #region Template elements

        bool isTemplateLoaded = false;
        StackPanel LeftButtonsEl;
        ContentPresenter HeaderContentArea;
        StackPanel RightButtonsEl;

        #endregion

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);

            LeftButtonsEl = e.NameScope.Find<StackPanel>(nameof(LeftButtonsEl));
            HeaderContentArea = e.NameScope.Find<ContentPresenter>(nameof(HeaderContentArea));
            RightButtonsEl = e.NameScope.Find<StackPanel>(nameof(RightButtonsEl));

            SetupButtons(LeftButtons, LeftButtonsEl);
            SetupButtons(RightButtons, RightButtonsEl);

            _leftButtons.CollectionChanged += LeftButtons_CollectionChanged;
            _rightButtons.CollectionChanged += RightButtons_CollectionChanged;
            DetachedFromVisualTree += PanelHeader_DetachedFromVisualTree;

            SetupHeaderContent();

            isTemplateLoaded = true;
        }

        private void LeftButtons_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            SetupButtons(LeftButtons, LeftButtonsEl);
        }

        private void RightButtons_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            SetupButtons(RightButtons, RightButtonsEl);
        }

        private void PanelHeader_DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
            _leftButtons.CollectionChanged -= LeftButtons_CollectionChanged;
            _rightButtons.CollectionChanged -= RightButtons_CollectionChanged;
            DetachedFromVisualTree -= PanelHeader_DetachedFromVisualTree;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);
            if (!isTemplateLoaded) return;

            if (change.Property == ContentProperty) {
                SetupHeaderContent();
            }
        }

        private void SetupButtons(ObservableCollection<Button> buttons, StackPanel buttonsEl) {
            buttonsEl.Children.Clear();
            if (buttons == null) return;
            foreach (Button button in buttons) {
                if (!button.Classes.Contains("Tertiary")) button.Classes.Add("Tertiary");
                if (!button.Classes.Contains("PanelHeaderButtonStyle")) button.Classes.Add("PanelHeaderButtonStyle");
                buttonsEl.Children.Add(button);
            }
        }

        private void SetupHeaderContent() {
            if (Content is string text) {
                TextBlock textBlock = new TextBlock { 
                    Text = text
                };
                textBlock.Classes.Add("DisplayBold");
                textBlock.Classes.Add("PanelHeaderTextStyle");
                HeaderContentArea.Content = textBlock;
            } else if (Content is Control control) {
                HeaderContentArea.Content = control;
            } else {
                HeaderContentArea.Content = null;
            }
        }
    }
}
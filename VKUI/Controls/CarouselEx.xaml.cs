using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using System.Diagnostics;
using System.Threading.Tasks;

namespace VKUI.Controls {
    [TemplatePart("PART_PreviousButton", typeof(Button))]
    [TemplatePart("PART_NextButton", typeof(Button))]
    public class CarouselEx : ContentControl {
        public int ScrollPixels { get; set; } = 48;

        #region Template controls

        bool isTemplateLoaded = false;
        Button PART_PreviousButton;
        Button PART_NextButton;
        private ScrollViewer scrollViewer;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            PART_PreviousButton = e.NameScope.Find<Button>(nameof(PART_PreviousButton));
            PART_NextButton = e.NameScope.Find<Button>(nameof(PART_NextButton));
            isTemplateLoaded = true;

            PART_PreviousButton.Click += PART_PreviousButton_Click;
            PART_NextButton.Click += PART_NextButton_Click;

            CheckScrollViewer();
        }

        #endregion

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);
            if (change.Property == ContentProperty) {
                CheckScrollViewer();
            }
        }

        private void PART_PreviousButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Debug.WriteLine("Previous button click!");
            var offset = scrollViewer.Offset;
            double x = offset.X - ScrollPixels;
            if (x < 0) x = 0;
            scrollViewer.Offset = new Vector(x, offset.Y);
        }

        private void PART_NextButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Debug.WriteLine("Next button click!");
            var offset = scrollViewer.Offset;
            double x = offset.X + ScrollPixels;
            double max = scrollViewer.Extent.Width - scrollViewer.DesiredSize.Width;
            if (x > max) x = max;
            scrollViewer.Offset = new Vector(x, offset.Y);
        }

        private void CheckScrollViewer() {
            if (Content is ListBox listBox) {
                if (listBox.Scroll != null) {
                    scrollViewer = listBox.Scroll as ScrollViewer;
                } else { // почти всегда так, поэтому делаем костыль.
                    listBox.Loaded += ListBox_Loaded;
                }
            } else if (Content is ScrollViewer sv) {
                scrollViewer = sv;
            }

            if (scrollViewer == null) return;
            SetUpScrollViewer();
        }

        private void ListBox_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            ListBox listBox = sender as ListBox;
            listBox.Loaded -= ListBox_Loaded;

            new System.Action(async () => {
                while (scrollViewer == null) {
                    scrollViewer = listBox.Scroll as ScrollViewer;
                    await Task.Delay(10);
                }
            })();

            SetUpScrollViewer();
        }

        private void SetUpScrollViewer() {
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

            CheckButtons();
            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            scrollViewer.Unloaded += ScrollViewer_Unloaded;
            SizeChanged += CarouselEx_SizeChanged;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            CheckButtons();
        }

        private void CarouselEx_SizeChanged(object sender, SizeChangedEventArgs e) {
            CheckButtons();
        }

        private void ScrollViewer_Unloaded(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            scrollViewer.Unloaded -= ScrollViewer_Unloaded;
        }

        private void CheckButtons() {
            if (!isTemplateLoaded) return;
            if (scrollViewer == null) {
                PART_PreviousButton.IsVisible = PART_NextButton.IsVisible = false;
                return;
            }

            var x = scrollViewer.Offset.X;
            PART_PreviousButton.IsVisible = x > 0;
            PART_NextButton.IsVisible = x < scrollViewer.Extent.Width - scrollViewer.DesiredSize.Width;
        }
    }
}

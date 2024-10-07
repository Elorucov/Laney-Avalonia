using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using ExCSS;
using System;
using System.Diagnostics;

namespace ELOR.Laney.Controls;

public class MediaSlider : TemplatedControl {
    #region Properties

    public static readonly StyledProperty<TimeSpan> DurationProperty = AvaloniaProperty.Register<MediaSlider, TimeSpan>(nameof(Duration));

    public TimeSpan Duration {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public static readonly StyledProperty<TimeSpan> PositionProperty = AvaloniaProperty.Register<MediaSlider, TimeSpan>(nameof(Position));

    public TimeSpan Position {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    #endregion

    private double ActualWidth { get { return Bounds.Width; } }
    public event EventHandler<TimeSpan> PositionChanged;

    #region Template elements

    Canvas Root;
    Border DurationLine;
    Border PositionLine;
    Border SliderThumb;

    #endregion

    bool isUILoaded = false;
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        Root = e.NameScope.Find<Canvas>(nameof(Root));
        DurationLine = e.NameScope.Find<Border>(nameof(DurationLine));
        PositionLine = e.NameScope.Find<Border>(nameof(PositionLine));
        SliderThumb = e.NameScope.Find<Border>(nameof(SliderThumb));
        isUILoaded = true;

        SizeChanged += MediaSlider_SizeChanged;
        Root.PointerPressed += StartDragThumb;
        Unloaded += MediaSlider_Unloaded;
        SetupSlider();
    }

    private void MediaSlider_Unloaded(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
        SizeChanged -= MediaSlider_SizeChanged;
        Root.PointerPressed -= StartDragThumb;
        Unloaded -= MediaSlider_Unloaded;
    }

    private void MediaSlider_SizeChanged(object sender, SizeChangedEventArgs e) {
        SetupSlider();
    }

    private void StartDragThumb(object sender, Avalonia.Input.PointerPressedEventArgs e) {
        isPressing = true;
        ChangeThumbPosition(e.GetCurrentPoint(Root).Position.X);
        Root.PointerMoved += Delta;
        Root.PointerReleased += StopDragThumb;
    }

    private void Delta(object sender, Avalonia.Input.PointerEventArgs e) {
        if (isPressing) {
            e.Handled = true;
            double x = e.GetCurrentPoint(Root).Position.X;
            ChangeThumbPosition(x);
        }
    }

    private void StopDragThumb(object sender, Avalonia.Input.PointerReleasedEventArgs e) {
        StopDragThumb();
    }

    private void StopDragThumb() {
        Root.PointerMoved -= Delta;
        Root.PointerReleased -= StopDragThumb;
        isPressing = false;

        double d = Duration.TotalMilliseconds;
        double w = ActualWidth;
        double sp = Canvas.GetLeft(SliderThumb);
        double t = SliderThumb.Width;
        double p = d / (w - t) * sp;
        if (1 / d * p <= 100) {
            Position = TimeSpan.FromMilliseconds(p);
            PositionChanged?.Invoke(this, Position);
        } else {
            SetupSlider();
        }
        // PositionFlyout.IsVisible = false;
    }

    private void ChangeThumbPosition(double x) {
        double w = ActualWidth;
        double t = SliderThumb.Width;
        double plt = 0;

        var z = x - (t / 2);
        if (z >= 0 && z <= w - t) {
            plt = z;
        } else if (z < 0) {
            plt = 0;
        } else if (z > w - t) {
            plt = w - t;
        }
        Canvas.SetLeft(SliderThumb, plt);

        double d = Duration.TotalMilliseconds;
        double pt = d / (w - t) * plt;
        TimeSpan tm = TimeSpan.FromMilliseconds(pt);
        // ChangePosFlyoutPosition(tm, x);
        // PositionFlyout.IsVisible = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);

        if (change.Property == DurationProperty || change.Property == PositionProperty) {
            SetupSlider();
        }
    }

    bool isPressing = false;
    private void SetupSlider() {
        try {
            if (isUILoaded) {
                double w = ActualWidth;
                double d = Duration.TotalMilliseconds;
                double p = Position.TotalMilliseconds;
                DurationLine.Width = w;
                if (d > 0) {
                    SliderThumb.IsVisible = true;
                    double pl = w / d * p;
                    PositionLine.Width = pl;

                    if (!isPressing) {
                        double t = SliderThumb.Width;
                        double plt = (w - t) / d * p;
                        Canvas.SetLeft(SliderThumb, plt);
                    }
                } else {
                    SliderThumb.IsVisible = false;
                }
            }
        } catch (Exception ex) {
            Debug.WriteLine($"SetupSlider error: (0x{ex.HResult.ToString("x8")}): {ex.Message.Trim()}");
        }
    }
}
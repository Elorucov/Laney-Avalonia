﻿using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using System;

namespace VKUI.Controls {
    /// <summary>
    /// A navigation control that supports simple stack-based navigation
    /// </summary>
    [TemplatePart("PART_NavigationBar", typeof(Border))]
    [TemplatePart("PART_BackButton", typeof(Button))]
    [TemplatePart("PART_ForwardButton", typeof(Button))]
    [TemplatePart("PART_ContentPresenter", typeof(TransitioningContentControl))]
    public class NavigationControl : ContentControl {
        private Button? _backButton;
        private Button? _forwardButton;
        private INavigationRouter? _navigationRouter;
        private TransitioningContentControl? _contentPresenter;

        /// <summary>
        /// Defines the <see cref="CanGoBack"/> property.
        /// </summary>
        public static readonly DirectProperty<NavigationControl, bool?> CanGoBackProperty =
            AvaloniaProperty.RegisterDirect<NavigationControl, bool?>(nameof(CanGoBack),
                o => o.CanGoBack);

        /// <summary>
        /// Defines the <see cref="CanGoForward"/> property.
        /// </summary>
        public static readonly DirectProperty<NavigationControl, bool?> CanGoForwardProperty =
            AvaloniaProperty.RegisterDirect<NavigationControl, bool?>(nameof(CanGoForward),
                o => o.CanGoForward);

        /// <summary>
        /// Defines the <see cref="Header"/> property.
        /// </summary>
        public static readonly StyledProperty<object?> HeaderProperty =
            AvaloniaProperty.Register<NavigationControl, object?>(nameof(Header));

        /// <summary>
        /// Defines the <see cref="IsBackButtonVisible"/> property.
        /// </summary>
        public static readonly StyledProperty<bool?> IsBackButtonVisibleProperty =
            AvaloniaProperty.Register<NavigationControl, bool?>(nameof(IsBackButtonVisible), true);

        /// <summary>
        /// Defines the <see cref="IsForwardButtonVisible"/> property.
        /// </summary>
        public static readonly StyledProperty<bool?> IsForwardButtonVisibleProperty =
            AvaloniaProperty.Register<NavigationControl, bool?>(nameof(IsForwardButtonVisible), false);

        /// <summary>
        /// Defines the <see cref="IsNavBarVisible"/> property.
        /// </summary>
        public static readonly StyledProperty<bool?> IsNavBarVisibleProperty =
            AvaloniaProperty.Register<NavigationControl, bool?>(nameof(IsNavBarVisible), false);

        /// <summary>
        /// Defines the <see cref="PageTransition"/> property.
        /// </summary>
        public static readonly StyledProperty<IPageTransition?> PageTransitionProperty =
            AvaloniaProperty.Register<NavigationControl, IPageTransition?>(nameof(PageTransition),
                new CrossFade(TimeSpan.FromSeconds(0.125)));

        /// <summary>
        /// Defines the <see cref="HeaderTemplate"/> property.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
            AvaloniaProperty.Register<NavigationControl, IDataTemplate?>(nameof(HeaderTemplate));

        /// <summary>
        /// Defines the <see cref="INavigationRouter"/> property.
        /// </summary>
        public static readonly DirectProperty<NavigationControl, INavigationRouter?> NavigationRouterProperty =
            AvaloniaProperty.RegisterDirect<NavigationControl, INavigationRouter?>(nameof(NavigationRouter),
                o => o.NavigationRouter, (o, v) => o.NavigationRouter = v);

        /// <summary>
        /// Gets whether it's possible to go back in the stack
        /// </summary>
        public bool? CanGoBack {
            get => NavigationRouter?.CanGoBack;
        }

        /// <summary>
        /// Gets whether it's possible to go forward in the stack
        /// </summary>
        public bool? CanGoForward {
            get => NavigationRouter?.CanGoForward;
        }

        /// <summary>
        /// Gets or sets the visibility of the navigation bar
        /// </summary>
        public bool? IsBackButtonVisible {
            get => GetValue(IsBackButtonVisibleProperty);
            set => SetValue(IsBackButtonVisibleProperty, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the navigation bar
        /// </summary>
        public bool? IsForwardButtonVisible {
            get => GetValue(IsForwardButtonVisibleProperty);
            set => SetValue(IsForwardButtonVisibleProperty, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the navigation bar
        /// </summary>
        public bool? IsNavBarVisible {
            get => GetValue(IsNavBarVisibleProperty);
            set => SetValue(IsNavBarVisibleProperty, value);
        }

        /// <summary>
        /// Gets or sets the animation played when content appears and disappears.
        /// </summary>
        public IPageTransition? PageTransition {
            get => GetValue(PageTransitionProperty);
            set => SetValue(PageTransitionProperty, value);
        }

        /// <summary>
        /// Gets or sets the header template
        /// </summary>
        public IDataTemplate? HeaderTemplate {
            get => GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the navigation router.
        /// </summary>
        public INavigationRouter? NavigationRouter {
            get => _navigationRouter;
            set {
                var oldRouter = NavigationRouter;

                if (oldRouter != null) {
                    oldRouter.Navigated -= NavigationRouter_Navigated;
                }

                SetAndRaise(NavigationRouterProperty, ref _navigationRouter, value);

                if (value != null) {
                    value.Navigated += NavigationRouter_Navigated;
                }

                _contentPresenter?.SetValue(ContentProperty, value?.CurrentPage);
            }
        }

        private void NavigationRouter_Navigated(object? sender, NavigatedEventArgs e) {
            _contentPresenter?.SetValue(ContentProperty, e.To);

            RaisePropertyChanged(CanGoBackProperty, null, CanGoBack);
            RaisePropertyChanged(CanGoForwardProperty, null, CanGoForward);
        }

        /// <summary>
        /// Gets or sets the header content.
        /// </summary>
        public object? Header {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        /// <summary>
        /// Gets or sets the BackButton template part.
        /// </summary>
        private Button? BackButton {
            get { return _backButton; }
            set {
                if (_backButton != null) {
                    _backButton.Click -= BackButton_Clicked;
                }
                _backButton = value;
                if (_backButton != null) {
                    _backButton.Click += BackButton_Clicked;
                }
            }
        }

        /// <summary>
        /// Gets or sets the ForwardButton template part.
        /// </summary>
        private Button? ForwardButton {
            get { return _forwardButton; }
            set {
                if (_forwardButton != null) {
                    _forwardButton.Click -= ForwardButton_Clicked;
                }
                _forwardButton = value;
                if (_forwardButton != null) {
                    _forwardButton.Click += ForwardButton_Clicked;
                }
            }
        }

        public NavigationControl() {
            NavigationRouter = new StackNavigationRouter();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);

            BackButton = e.NameScope.Get<Button>("PART_BackButton");

            ForwardButton = e.NameScope.Get<Button>("PART_ForwardButton");

            _contentPresenter = e.NameScope.Get<TransitioningContentControl>("PART_ContentPresenter");

            _contentPresenter.TemplateApplied += ContentPresenter_TemplateApplied;

            _contentPresenter?.SetValue(ContentProperty, NavigationRouter?.CurrentPage);
        }

        private void ContentPresenter_TemplateApplied(object? sender, TemplateAppliedEventArgs e) {
            if (_contentPresenter?.Presenter is ContentPresenter presenter) {
                presenter.PropertyChanged += Presenter_PropertyChanged;
            }
        }

        private void Presenter_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
            if (e.Property == ContentPresenter.ChildProperty) {
                LogicalChildren.Clear();
                if (e.NewValue is ILogical newLogical)
                    LogicalChildren.Add(newLogical);
            }
        }

        private void BackButton_Clicked(object? sender, RoutedEventArgs eventArgs) {
            if (NavigationRouter != null) {
                new Action(async () => await NavigationRouter.BackAsync())();
            }
        }

        private void ForwardButton_Clicked(object? sender, RoutedEventArgs eventArgs) {
            if (NavigationRouter != null) {
                new Action(async () => await NavigationRouter.ForwardAsync())();
            }
        }
    }
}
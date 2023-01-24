using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;

#nullable enable

namespace VKUI {
    public enum VKUIScheme {
        BrightLight,
        SpaceGray,
    }

    public sealed class VKUITheme : AvaloniaObject, IStyle, IResourceProvider {
        private readonly Uri _baseUri;
        private Style _spaceGray;
        private Style _brightLight;
        private Styles _sharedStyles = new();
        private bool _isLoading;
        private IStyle? _loaded;

        public static VKUITheme Current { get; private set; }

        public static IResourceDictionary Icons {
            get => (Application.Current.Resources.MergedDictionaries[0] as ResourceInclude).Loaded;
        }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKUITheme"/> class.
    /// </summary>
    /// <param name="baseUri">The base URL for the XAML context.</param>
    public VKUITheme(Uri baseUri) {
            if (Current != null) throw new Exception($"{nameof(VKUITheme)} already initialized!");
            Current = this;
            _baseUri = baseUri;
            InitStyles(baseUri);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VKUITheme"/> class.
        /// </summary>
        /// <param name="serviceProvider">The XAML service provider.</param>
        public VKUITheme(IServiceProvider serviceProvider) {
            if (Current != null) throw new Exception($"{nameof(VKUITheme)} already initialized!");
            Current = this;
            _baseUri = ((IUriContext)serviceProvider.GetService(typeof(IUriContext))).BaseUri;
            InitStyles(_baseUri);
        }


        public static readonly StyledProperty<VKUIScheme> SchemeProperty =
            AvaloniaProperty.Register<VKUITheme, VKUIScheme>(nameof(Scheme));
        /// <summary>
        /// Gets or sets the Scheme of the VKUI theme (light, dark).
        /// </summary>
        public VKUIScheme Scheme {
            get => GetValue(SchemeProperty);
            set => SetValue(SchemeProperty, value);
        }
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);
            if (change.Property == SchemeProperty) {
                if (Scheme == VKUIScheme.SpaceGray) {
                    (Loaded as Styles)![1] = _spaceGray;
                } else {
                    (Loaded as Styles)![1] = _brightLight;
                }
            }
        }

        public IResourceHost? Owner => (Loaded as IResourceProvider)?.Owner;

        /// <summary>
        /// Gets the loaded style.
        /// </summary>
        public IStyle Loaded {
            get {
                if (_loaded == null) {
                    _isLoading = true;

                    if (Scheme == VKUIScheme.BrightLight) {
                        _loaded = new Styles() { _sharedStyles, _brightLight };
                    } else if (Scheme == VKUIScheme.SpaceGray) {
                        _loaded = new Styles() { _sharedStyles, _spaceGray };
                    }
                    _isLoading = false;
                }

                return _loaded!;
            }
        }

        bool IResourceNode.HasResources => (Loaded as IResourceProvider)?.HasResources ?? false;

        IReadOnlyList<IStyle> IStyle.Children => _loaded?.Children ?? Array.Empty<IStyle>();

        public event EventHandler? OwnerChanged {
            add {
                if (Loaded is IResourceProvider rp) {
                    rp.OwnerChanged += value;
                }
            }
            remove {
                if (Loaded is IResourceProvider rp) {
                    rp.OwnerChanged -= value;
                }
            }
        }

        public SelectorMatchResult TryAttach(IStyleable target, IStyleHost? host) => Loaded.TryAttach(target, host);

        public bool TryGetResource(object key, out object? value) {
            if (!_isLoading && Loaded is IResourceProvider p) {
                return p.TryGetResource(key, out value);
            }

            value = null;
            return false;
        }

        void IResourceProvider.AddOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.AddOwner(owner);
        void IResourceProvider.RemoveOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.RemoveOwner(owner);

        private void InitStyles(Uri baseUri) {
            Application.Current.Resources["VKSansText"] = FontFamily.Parse("avares://VKUI/Fonts#VK Sans Text");
            Application.Current.Resources["VKSansDisplay"] = FontFamily.Parse("avares://VKUI/Fonts#VK Sans Display");

            _sharedStyles = new Styles {
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Typography.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Styles/ButtonStyles.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Styles/ComboBoxStyles.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Styles/CommonStyles.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Styles/ListBoxStyles.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Styles/TabStyles.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Styles/TextBoxStyles.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Styles/ToggleSwitchStyles.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Controls/ActionSheetItem.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Controls/Avatar.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Controls/Cell.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Controls/PanelHeader.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Controls/Placeholder.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Controls/Spinner.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Controls/VKIcon.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Controls/VKUIFlyoutPresenter.axaml")
                },
                new StyleInclude(baseUri) {
                    Source = new Uri("avares://VKUI/Controls/WindowTitleBar.axaml")
                }
            };

            _brightLight = (Style)AvaloniaXamlLoader.Load(new Uri("avares://VKUI/Light.axaml"));
            _spaceGray = (Style)AvaloniaXamlLoader.Load(new Uri("avares://VKUI/Dark.axaml"));
        }

        public SelectorMatchResult TryAttach(IStyleable target, object? host) => Loaded.TryAttach(target, host);
    }
}
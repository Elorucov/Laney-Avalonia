using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.VKAPILib.Objects;
using System.Runtime.InteropServices;
using VKUI.Controls;

namespace ELOR.Laney.Controls {
    public class BotKeyboardUI : TemplatedControl {
        #region Properties

        public static readonly StyledProperty<BotKeyboard> KeyboardProperty =
            AvaloniaProperty.Register<BotKeyboardUI, BotKeyboard>(nameof(Keyboard));

        public BotKeyboard Keyboard {
            get => GetValue(KeyboardProperty);
            set => SetValue(KeyboardProperty, value);
        }

        #endregion

        #region Template elements

        StackPanel Root;

        bool isUILoaded = false;
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            Root = e.NameScope.Find<StackPanel>(nameof(Root));
            isUILoaded = true;
            Render();
        }

        #endregion

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);

            if (change.Property == KeyboardProperty) {
                Render();
            }
        }

        private void Render() {
            if (!isUILoaded) return;
            Root.Children.Clear();
            if (Keyboard == null) return;

            bool isFirstRow = true;
            foreach (var row in CollectionsMarshal.AsSpan(Keyboard.Buttons)) {
                Grid buttonsRow = new Grid {
                    ColumnDefinitions = new ColumnDefinitions(),
                    Margin = new Thickness(-4, isFirstRow ? 0 : 8, -4, 0)
                };
                for (byte i = 0; i < row.Count; i++) {
                    buttonsRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                    Button button = BuildButton(row[i]);
                    button.HorizontalAlignment = HorizontalAlignment.Stretch;
                    Grid.SetColumn(button, i);
                    buttonsRow.Children.Add(button);
                }
                isFirstRow = false;
                Root.Children.Add(buttonsRow);
            }
        }

        private Button BuildButton(BotButton button) {
            Button buttonUI = new Button {
                Margin = new Thickness(4, 0, 4, 0),
                Padding = new Thickness(0, 0, 0, 0),
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
            buttonUI.Classes.Add("Medium");

            StackPanel contentIn = new StackPanel { 
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            VKIcon icon = new VKIcon {
                Margin = new Thickness(0, 8, 0, 8),
                IsVisible = false,
            };

            TextBlock label = new TextBlock { 
                FontSize = 15,
                LineHeight = 15,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeight.Medium,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(4, 10, 4, 10)
            };
            label.Classes.Add("SemiBold");
            if (button.Color != BotButtonColor.Default || button.Action.Type == BotButtonType.VKPay) 
                label.Classes.Add("ButtonIn");

            contentIn.Children.Add(icon);
            contentIn.Children.Add(label);

            Grid content = new Grid { Height = 36 };
            content.Children.Add(contentIn);
            buttonUI.Content = content;

            if (button.Action.Type == BotButtonType.VKPay) {
                buttonUI.Classes.Add("Primary");
            } else {
                switch (button.Color) {
                    case BotButtonColor.Primary:
                        buttonUI.Classes.Add("Primary");
                        icon.RegisterThemeResource(VKIcon.ForegroundProperty, "VKButtonPrimaryForegroundBrush");
                        break;
                    case BotButtonColor.Positive:
                        buttonUI.Classes.Add("Commerce");
                        icon.RegisterThemeResource(VKIcon.ForegroundProperty, "VKButtonCommerceForegroundBrush");
                        break;
                    case BotButtonColor.Negative:
                        buttonUI.Classes.Add("Negative");
                        icon.RegisterThemeResource(VKIcon.ForegroundProperty, "VKButtonCommerceForegroundBrush");
                        break;
                    default:
                        icon.RegisterThemeResource(VKIcon.ForegroundProperty, "VKButtonSecondaryForegroundBrush");
                        break;
                }
            }

            switch (button.Action.Type) {
                case BotButtonType.VKPay:
                    label.Text = "Pay via VK Pay";
                    break;
                case BotButtonType.Location:
                    label.Text = Localizer.Instance["geo"];
                    icon.Id = VKIconNames.Icon20PlaceOutline;
                    icon.IsVisible = true;
                    break;
                case BotButtonType.OpenApp:
                    label.Text = button.Action.Label;
                    icon.Id = VKIconNames.Icon20ServicesOutline;
                    icon.IsVisible = true;
                    break;
                default:
                    label.Text = button.Action.Label;
                    break;
            }

            if (button.Action.Type == BotButtonType.OpenLink) {
                var arrowIcon = new VKIcon {
                    Width = 12,
                    Height = 12,
                    Margin = new Thickness(3),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Id = VKIconNames.Icon12ArrowUpRight
                };
                arrowIcon.RegisterThemeResource(VKIcon.ForegroundProperty, "VKButtonSecondaryForegroundBrush");
                content.Children.Add(arrowIcon);
            }
            return buttonUI;
        }
    }
}
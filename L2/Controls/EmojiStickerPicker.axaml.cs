using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using ELOR.Laney.Core.Network;
using ELOR.Laney.DataModels;
using ELOR.Laney.ViewModels.Controls;
using ELOR.VKAPILib.Objects;
using NeoSmart.Unicode;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ELOR.Laney.Controls {
    public partial class EmojiStickerPicker : UserControl {
        EmojiStickerPickerViewModel ViewModel { get => DataContext as EmojiStickerPickerViewModel; }

        public EmojiStickerPicker() {
            InitializeComponent();
            Unloaded += EmojiStickerPicker_Unloaded;
        }

        public event EventHandler<string> EmojiPicked;
        public event EventHandler<Sticker> StickerPicked;

        private void EmojiStickerPicker_Unloaded(object sender, RoutedEventArgs e) {
            Unloaded -= EmojiStickerPicker_Unloaded;
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void EmojiStickerPicker_Loaded(object sender, RoutedEventArgs e) {
            if (Design.IsDesignMode) return;
            ChangeTabContentTemplate();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(EmojiStickerPickerViewModel.SelectedTab)) {
                ChangeTabContentTemplate();
            }
        }

        private void ChangeTabContentTemplate() {
            var content = ViewModel.SelectedTab.Content;
            if (content is ObservableCollection<EmojiGroup>) {
                StickersTabContent.IsVisible = false;
                EmojisTabContent.IsVisible = true;
            } else if (content is ObservableCollection<Sticker>) {
                EmojisTabContent.IsVisible = false;
                StickersTabContent.IsVisible = true;
            }
        }

        private void EmojiListBoxItem_Tapped(object sender, TappedEventArgs e) {
            Control c = sender as Control;
            SingleEmoji emoji = (SingleEmoji)c.DataContext;
            EmojiPicked?.Invoke(this, emoji.ToString());
        }

        private void EmojiListBox_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                ListBox lb = sender as ListBox;
                SingleEmoji emoji = (SingleEmoji)lb.SelectedItem;
                EmojiPicked?.Invoke(this, emoji.ToString());
            }
        }

        private void StickersListBoxItem_Tapped(object sender, TappedEventArgs e) {
            Control c = sender as Control;
            Sticker sticker = (Sticker)c.DataContext;
            StickerPicked?.Invoke(this, sticker);
        }

        private void StickersListBox_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                ListBox lb = sender as ListBox;
                Sticker sticker = (Sticker)lb.SelectedItem;
                StickerPicked?.Invoke(this, sticker);
            }
        }

        // TODO: method to download images without caching
        private async void PackImage_DataContextChanged(object? sender, EventArgs e) {
            if (sender is Image img && img.DataContext is TabItem<object> tab) {
                if (tab.Image != null) {
                    try {
                        var response = await LNet.GetAsync(tab.Image);
                        response.EnsureSuccessStatusCode();
                        var bytes = await response.Content.ReadAsByteArrayAsync();
                        Stream stream = new MemoryStream(bytes);
                        if (bytes.Length == 0) throw new Exception("Image length is 0!");
                        var bitmap = WriteableBitmap.DecodeToWidth(stream, 22, BitmapInterpolationMode.MediumQuality);
                        img.Source = bitmap;
                    } catch (Exception ex) {
                        Log.Error(ex, $"EmojiStickerPickerUI: cannot load a sticker pack icon!");
                    }
                }
            }
        }
    }
}
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using ELOR.Laney.DataModels;
using ELOR.Laney.ViewModels.Controls;
using ELOR.VKAPILib.Objects;
using NeoSmart.Unicode;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            if (ViewModel.SelectedTab.Content is ObservableCollection<EmojiGroup>) {
                StickersTabContent.IsVisible = false;
                EmojisTabContent.IsVisible = true;
            } else if (ViewModel.SelectedTab.Content is ObservableCollection<Sticker>) {
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
    }
}
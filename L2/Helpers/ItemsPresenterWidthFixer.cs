using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using ELOR.Laney.Extensions;
using System;
using System.Diagnostics;

namespace ELOR.Laney.Helpers {

    // Фиксит странный баг, из-за которого ширина
    // ItemsPresenter внутри ListBox может стать больше
    // самого ListBox, если в items-ах есть TextBlock
    // с длинным текстом.
    public class ItemsPresenterWidthFixer : IDisposable {
        ListBox listBox;
        ItemsPresenter presenter;

        public ItemsPresenterWidthFixer(ListBox target) {
            listBox = target;
            presenter = listBox.GetFirstVisualChildrenByType<ItemsPresenter>();
            if (presenter == null) throw new ArgumentException("ListBox's template doesn't contain an ItemsPresenter!");

            FixSize();
            listBox.SizeChanged += ListBox_SizeChanged;
            listBox.Unloaded += (a, b) => Dispose();
        }

        private void ListBox_SizeChanged(object sender, SizeChangedEventArgs e) {
            FixSize();
        }

        private void FixSize() {
            Debug.WriteLine($"FixSize for items presenter. WIdth: {listBox.Bounds.Width}px");
            presenter.Width = listBox.Bounds.Width;
        }

        bool disposed = false;
        public void Dispose() {
            if (disposed) return;
            disposed = true;
            listBox.SizeChanged -= ListBox_SizeChanged;
        }
    }
}
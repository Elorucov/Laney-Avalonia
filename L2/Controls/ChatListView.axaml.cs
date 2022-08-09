using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.VisualTree;
using ELOR.Laney.Collections;
using ELOR.Laney.ViewModels.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace ELOR.Laney.Controls {
    public class ChatListView : TemplatedControl {

        public static StyledProperty<DataTemplate> ItemTemplateProperty = AvaloniaProperty.Register<ChatListView, DataTemplate>(nameof(ItemTemplate));
        public DataTemplate ItemTemplate {
            get { return GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static StyledProperty<DataTemplate> GroupHeaderTemplateProperty = AvaloniaProperty.Register<ChatListView, DataTemplate>(nameof(GroupHeaderTemplate));
        public DataTemplate GroupHeaderTemplate {
            get { return GetValue(GroupHeaderTemplateProperty); }
            set { SetValue(GroupHeaderTemplateProperty, value); }
        }

        private MessagesCollection _items = new MessagesCollection();
        public static DirectProperty<ChatListView, MessagesCollection> ItemsProperty =
            AvaloniaProperty.RegisterDirect<ChatListView, MessagesCollection>(nameof(Items),
                o => o.Items, (o, v) => o.Items = v);
        public MessagesCollection Items {
            get { return _items as MessagesCollection; }
            set { SetAndRaise(ItemsProperty, ref _items, value); }
        }

        #region Constants

        private const string ItemContainerName = "ItemContainer";
        private const string GroupHeaderContainerName = "GroupHeaderContainer";

        #endregion

        #region Read-only public properties

        public MessageViewModel LastDisplayingItem { get; private set; }

        public double VirtualizationTopBound {
            get {
                return scrollViewer.Offset.Y + 100;
                // return scrollViewer.Offset.Y - 100;
            }
        }
        public double VirtualizationBottomBound {
            get {
                return scrollViewer.Offset.Y + scrollViewer.Viewport.Height - 100;
                // return scrollViewer.Offset.Y + scrollViewer.Viewport.Height + 100;
            }
        }

        #endregion

        //

        ScrollViewer scrollViewer;
        Canvas canvas;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            scrollViewer = e.NameScope.Find<ScrollViewer>(nameof(scrollViewer));
            canvas = e.NameScope.Find<Canvas>(nameof(canvas));
            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;

            PropertyChanged += (a, b) => {
                if (b.Property.Name == nameof(Items)) CheckItems();
                if (b.Property.Name == nameof(Bounds)) OnSizeChanged();
            };
            CheckItems();
        }

        double oldWidth = 0;
        private void OnSizeChanged() {
            double scrollableHeight = scrollViewer.Extent.Height;
            if (oldScrollableHeight == 0) {
                SaveCurrentVisibleItem();
                return;
            }
            if (oldScrollableHeight != scrollableHeight) { // added or removed item or elem. height changed
                bool dontRearrange = oldScrollableHeight == 0;
                Debug.WriteLine($"LayoutUpdated: oldsh: {oldScrollableHeight}; newsh: {scrollableHeight}");
                SavePosition(scrollableHeight);
                if (!dontRearrange) {
                    RearrangeAllItems();
                }
            }
            if (oldWidth != Bounds.Size.Width) {
                Debug.WriteLine($"LayoutUpdated: olwidth: {oldWidth}; newwidth: {Bounds.Size.Width}");
                oldWidth = Bounds.Size.Width;
                foreach (Control el in virtualElements) {
                    MeasureElement(el);
                }
                RearrangeAllItems();
            }
        }

        //

        private List<Control> virtualElements = new List<Control>();

        private void CheckItems() {
            if (Items != null) {
                if (Items.Count > 0) {
                    AddAllItemsToPanel();
                    RearrangeAllItems();
                } else {
                    ClearPanel();
                }
                Items.CollectionChanged += OnCollectionChanged;
            } else {
                ClearPanel();
            }
        }

        private void AddAllItemsToPanel() {
            DateTime? tempDate = null;
            foreach (var item in Items) {
                DateTime sentDate = item.SentTime.Date;
                if (tempDate != sentDate) {
                    virtualElements.Add(GenerateGroupHeaderContainer(sentDate));
                }
                virtualElements.Add(GenerateItemContainer(item));
                tempDate = sentDate;
            }
        }

        private void AddNewItemsToPanel(IList? newItems, int startingIndex) {
            foreach (var ritem in newItems) {
                if (startingIndex != -2 && ritem is MessageViewModel item) {
                    AddItemToPanel(item, startingIndex);
                } else if (startingIndex == -2 && ritem is IEnumerable<MessageViewModel> qitems) {
                    foreach (var qitem in qitems) {
                        startingIndex = Items.IndexOf(qitem);
                        AddItemToPanel(qitem, startingIndex);
                    }
                } else {
                    throw new ArgumentException("Required IEnumerable<MessageViewModel>", nameof(Items));
                }
            }
        }

        private void AddItemToPanel(MessageViewModel item, int index, Border existContainer = null) {
            int indexInList = -1;
            int i = 0;
            DateTime? tempDate = null;
            for (i = 0; i < virtualElements.Count; i++) {
                if (virtualElements[i] is Border container && container.DataContext is MessageViewModel eitem) {
                    if (eitem.Id == item.Id) {
                        Debug.WriteLine("Duplicate item!");
                        return;
                    }
                    indexInList++;
                    if (indexInList == index) break;
                } else if (virtualElements[i] is Border groupTitle && groupTitle.DataContext is DateTime dt) {
                    tempDate = dt;
                }
            }

            bool needNewGroupTitle = item.SentTime.Date != tempDate;
            Border newContainer = existContainer != null && existContainer.DataContext == item ? existContainer : GenerateItemContainer(item);
            if (needNewGroupTitle) {
                Debug.WriteLine($"AddItemToPanel: need to create new group title for {item.Id}. iteration: {i}, c: {virtualElements.Count}");
                virtualElements.Insert(index == 0 ? 0 : i, newContainer);
                virtualElements.Insert(index == 0 ? 0 : i, GenerateGroupHeaderContainer(item.SentTime.Date));
            } else {
                Debug.WriteLine($"AddItemToPanel: add {item.Id} to {i}, c: {virtualElements.Count}");
                virtualElements.Insert(i, newContainer);
            }

            int groupPosForNextItem = needNewGroupTitle ? i + 2 : i + 1;
            MessageViewModel nextItem = GetItemByPanelIndex(groupPosForNextItem);
            if (nextItem != null && nextItem.SentTime.Date != item.SentTime.Date && i > 1) {
                virtualElements.Insert(groupPosForNextItem, GenerateGroupHeaderContainer(nextItem.SentTime.Date));
            }

            if (i <= 1) return;
            DateTime? excessGroupTitle = GetGroupHeaderByIndex(i - 1);
            if (excessGroupTitle != null) {
                TryRemoveGroupHeaderFromPanel(i - 1);
            }
        }

        private void RemoveOldItemsFromPanel(IList oldItems, int startingIndex) {
            foreach (var ritem in oldItems) {
                if (ritem is MessageViewModel item) {
                    RemoveItemFromPanel(item, startingIndex);
                } else {
                    throw new ArgumentException("Required MessageViewModel", nameof(Items));
                }
            }
        }

        private Border RemoveItemFromPanel(MessageViewModel item, int index, bool keepContainer = false) {
            int removeIndex = -1;
            Border keptContainer = null;

            for (int i = 0; i < virtualElements.Count; i++) {
                Border container = virtualElements[i] as Border;
                if (container != null && container.DataContext == item) {
                    removeIndex = i;
                    Debug.WriteLine($"RemoveItemFromPanel: Found item. Index in panel: {removeIndex}");
                    break;
                }
            }

            if (keepContainer) {
                keptContainer = (Border)virtualElements[removeIndex];
                virtualElements.Remove(keptContainer);
            } else {
                virtualElements.RemoveAt(removeIndex);
            }


            DateTime? groupTitle = GetGroupHeaderByIndex(removeIndex - 1);
            if (groupTitle != null) {
                DateTime? groupTitleNext = GetGroupHeaderByIndex(removeIndex);
                if (groupTitleNext != null || removeIndex == virtualElements.Count) TryRemoveGroupHeaderFromPanel(removeIndex - 1);
                if (groupTitleNext != null) {
                    DateTime? excessGroupTitle = GetGroupHeaderByIndex(removeIndex - 1);
                    if (excessGroupTitle != null) {
                        var previtem = GetItemByPanelIndex(removeIndex - 2);
                        if (previtem == null) return keptContainer;
                        if (previtem.SentTime.Date == excessGroupTitle.Value) TryRemoveGroupHeaderFromPanel(removeIndex - 1);
                    }
                }
            }

            return keptContainer;
        }

        private void TryMoveItemInPanel(NotifyCollectionChangedEventArgs e) {
            if (e.Action != NotifyCollectionChangedAction.Move) return;

            MessageViewModel item = null;
            bool needToRefreshPanel = false;
            if (e.NewItems.Count == 1 && e.OldItems.Count == 1) {
                item = e.NewItems.Cast<MessageViewModel>().FirstOrDefault();
                MessageViewModel iold = e.OldItems.Cast<MessageViewModel>().FirstOrDefault();
                needToRefreshPanel = item != iold;
            } else {
                needToRefreshPanel = true;
            }

            if (needToRefreshPanel) {
                Debug.WriteLine($"TryMoveItemInPanel: need remove all items in Panel! new: {e.NewItems?.Count}, old: {e.OldItems?.Count}");
                ClearPanel();
                AddAllItemsToPanel();
            } else {
                MoveItemInPanel(item, e.OldStartingIndex, e.NewStartingIndex);
            }
        }

        private void MoveItemInPanel(MessageViewModel item, int oldIndex, int newIndex) {
            Border container = RemoveItemFromPanel(item, oldIndex, true);
            AddItemToPanel(item, newIndex, container);
        }

        private void TryRemoveGroupHeaderFromPanel(int index) {
            Border groupTitle = virtualElements[index] as Border;
            if (groupTitle != null && groupTitle.DataContext is DateTime) {
                virtualElements.RemoveAt(index);
            }
        }

        private void RearrangeAllItems() {
            canvas.Children.Clear();
            if (virtualElements.Count == 0) return;

            double top = 0;
            List<double> heights = new List<double>();
            foreach (Control el in CollectionsMarshal.AsSpan<Control>(virtualElements)) {
                Canvas.SetTop(el, top);
                double h = (double)el.Tag;
                heights.Add(h);
                top += h;
            }

            string s = String.Join("; ", heights);
            s += $"\nTotal: {top}px";
            Debug.WriteLine(s);

            canvas.Height = top;

            foreach (Control el in CollectionsMarshal.AsSpan<Control>(virtualElements)) {
                if (el is Border border) {
                    double t = Canvas.GetTop(el);
                    double h = (double)el.Tag;

                    double topVirt = VirtualizationTopBound;
                    double bottomVirt = VirtualizationBottomBound;

                    if (t + h < topVirt || t > bottomVirt) {
                        if (border.Name == ItemContainerName) border.Child = null;
                    }
                }
                canvas.Children.Add(el);
            }
        }

        private void MeasureElement(Control element) {
            Thickness margin = element.Margin;
            element.Measure(new Size(Bounds.Size.Width, double.MaxValue));
            double h = element.DesiredSize.Height + margin.Top + margin.Bottom;
            if (h > 0) element.Tag = h;
            Debug.WriteLine($"MeasureElement {element.GetType().Name}: height: {h}px");
        }

        private void ClearPanel() {
            virtualElements.Clear();
            canvas.Children.Clear();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            Debug.WriteLine($"OnCollectionChanged: {e.Action}; new: {e.NewItems?.Count}, old: {e.OldItems?.Count}, ns: {e.NewStartingIndex}, os: {e.OldStartingIndex}");
            switch (e.Action) {
                case NotifyCollectionChangedAction.Reset:
                    ClearPanel();
                    AddAllItemsToPanel();
                    break;
                case NotifyCollectionChangedAction.Add:
                    AddNewItemsToPanel(e.NewItems, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveOldItemsFromPanel(e.OldItems, e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    TryMoveItemInPanel(e);
                    break;
            }
            RearrangeAllItems();
        }

        private Border GenerateGroupHeaderContainer(DateTime item) {
            Control el = GetItemTemplate(item, GroupHeaderTemplate);

            Border b = new Border {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Width = Bounds.Size.Width,
                Background = new SolidColorBrush(Colors.Transparent),
                Child = el,
                DataContext = item,
                Name = GroupHeaderContainerName
            };

            MeasureElement(b);
            return b;
        }

        private Border GenerateItemContainer(MessageViewModel item) {
            Control el = GetItemTemplate(item, ItemTemplate);

            Border b = new Border {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Width = Bounds.Size.Width,
                Background = new SolidColorBrush(Colors.Transparent),
                Child = el,
                DataContext = item,
                Name = ItemContainerName
            };

            MeasureElement(b);
            return b;
        }

        private Control GetItemTemplate(object item, DataTemplate dataTemplate) {
            Control el = (Control)dataTemplate.Build(item);
            el.DataContext = item;
            return el;
        }

        //

        private MessageViewModel GetItemByPanelIndex(int index) {
            if (virtualElements.Count > index && virtualElements[index].DataContext is MessageViewModel item) return item;
            return null;
        }

        private DateTime? GetGroupHeaderByIndex(int index) {
            if (virtualElements.Count > index && virtualElements[index].DataContext is DateTime date) return date;
            return null;
        }

        #region Scroll viewer and virtualization

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            CheckScrollView();
            PerformVirtualization();
        }

        Control savedLastItem = null;
        double savedLastItemOffsetY = -1;
        double oldScrollableHeight = -1;
        double verticalOffsetForVirt = -1;

        private void CheckScrollView() {
            string dbg = $"SH: {Math.Round(scrollViewer.Extent.Height, 1)}\n";
            dbg += $"VO: {Math.Round(scrollViewer.Offset.Y, 1)}\n";

            double scrollableHeight = scrollViewer.Extent.Height;
            if (oldScrollableHeight == -1) oldScrollableHeight = scrollableHeight;
            if (oldScrollableHeight != scrollableHeight) { // added or removed item
                SavePosition(scrollableHeight);
            } else {
                SaveCurrentVisibleItem();
            }


            if (savedLastItem != null) {
                dbg += $"OY: {savedLastItemOffsetY}\n";
                if (savedLastItem.DataContext is MessageViewModel obj) {
                    dbg += $"DC: {obj.Id}";
                    LastDisplayingItem = obj;
                } else {
                    dbg += $"DC: N/A";
                }
            } else {
                dbg += $"DC: N/A";
            }
        }

        private void SaveCurrentVisibleItem() {
            double vh = scrollViewer.Viewport.Height;
            double oy = scrollViewer.Offset.Y;
            var bp = oy + vh;

            var elements = canvas.GetVisualsAt(new Point(Bounds.Size.Width / 2, bp - 1));
            if (elements != null) {
                foreach (var el in elements) {
                    if (el is Border b && b.Name == ItemContainerName) {
                        savedLastItem = b;
                        savedLastItemOffsetY = Canvas.GetTop(b);
                        break;
                    }
                }
            }
        }

        private void SavePosition(double scrollableHeight) {
            if (savedLastItem != null) {
                double cur = scrollViewer.Offset.Y;
                double off = Canvas.GetTop(savedLastItem);
                double diff = cur == scrollableHeight ? 0 : off - savedLastItemOffsetY;
                Debug.WriteLine($"SavePosition: off: {off}, diff: {diff}");

                oldScrollableHeight = scrollableHeight;
                savedLastItemOffsetY = off;
                scrollViewer.Offset = new Vector(0, scrollViewer.Offset.Y + diff);
                verticalOffsetForVirt = scrollViewer.Offset.Y + diff;
                PerformVirtualization();
                verticalOffsetForVirt = -1;
            }
        }

        private void PerformVirtualization() {
            int index = 0;
            foreach (Control el in CollectionsMarshal.AsSpan<Control>(virtualElements)) {
                if (el is Border clvi && clvi.Name == ItemContainerName) {
                    double t = Canvas.GetTop(el);
                    double h = (double)el.Tag;

                    double topVirt = VirtualizationTopBound;
                    double bottomVirt = VirtualizationBottomBound;

                    if (t + h < topVirt || t > bottomVirt) {
                        clvi.Child = null;
                    } else {
                        Control nel = GetItemTemplate(clvi.DataContext, ItemTemplate);
                        clvi.Child = nel;
                        MeasureElement(clvi);
                    }
                }
                index++;
            }
        }

        #endregion

    }
}

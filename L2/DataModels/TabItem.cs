using System;

namespace ELOR.Laney.DataModels {
    public class TabItem<T> {
        public string IconId { get; private set; }
        public Uri Image { get; private set; }
        public string Label { get; private set; }
        public T Content { get; private set; }

        public TabItem(string label, T content, string iconId = null, Uri image = null) {
            Label = label;
            Content = content;
            IconId = iconId;
            Image = image;
        }
    }
}
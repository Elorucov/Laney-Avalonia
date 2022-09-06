namespace VKUI.Utils {
    internal class PLThumb {
        internal double Width { get; set; }

        internal double Height { get; set; }

        internal double CalcWidth { get; set; }

        internal double CalcHeight { get; set; }

        internal bool LastColumn { get; set; }

        internal bool LastRow { get; set; }

        internal double GetRatio() {
            return Width / Height;
        }

        internal void SetViewSize(double width, double height, bool lastColumn, bool lastRow) {
            CalcWidth = width;
            CalcHeight = height;
            LastColumn = lastColumn;
            LastRow = lastRow;
        }
    }
}
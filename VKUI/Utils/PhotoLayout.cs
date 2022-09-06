using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VKUI.Utils {
    public class PhotoLayout {
        public static Tuple<List<Rect>, Size> Create(Size parentSize, List<Size> elementsSizes, double marginBetween) {
            if (elementsSizes.Count > 0) {
                List<PLThumb> PLThumbs = ConvertSizesToPLThumbs(elementsSizes);
                PhotoLayoutInternal.ProcessThumbnails(parentSize.Width, parentSize.Height, PLThumbs, marginBetween);
                List<Rect> formed = ConvertProcessedThumbsToRects(PLThumbs, marginBetween, parentSize.Width);
                double csw = Math.Round(formed.Last().Left + formed.Last().Width);
                double csh = Math.Round(formed.Last().Top + formed.Last().Height);
                return new Tuple<List<Rect>, Size>(formed, new Size(csw, csh));
            } else {
                return null;
            }
        }

        private static List<Rect> ConvertProcessedThumbsToRects(List<PLThumb> thumbs, double marginBetween, double width) {
            List<Rect> rectList = new List<Rect>(thumbs.Count);
            double num1 = 0.0;
            double widthOfRow = CalculateWidthOfRow(thumbs, marginBetween);
            double num2 = width / 2.0 - widthOfRow / 2.0;
            double num3 = num2;
            for (int index = 0; index < thumbs.Count; ++index) {
                PLThumb thumb = thumbs[index];
                rectList.Add(new Rect(num3, num1, thumb.CalcWidth, thumb.CalcHeight));
                if (!thumb.LastColumn && !thumb.LastRow)
                    num3 += thumb.CalcWidth + marginBetween;
                else if (thumb.LastRow)
                    num1 += thumb.CalcHeight + marginBetween;
                else if (thumb.LastColumn) {
                    num3 = num2;
                    num1 += thumb.CalcHeight + marginBetween;
                }
            }
            return rectList;
        }

        private static double CalculateWidthOfRow(List<PLThumb> thumbs, double marginBetween) {
            double width = 0;
            foreach (PLThumb thumb in thumbs) {
                width += thumb.CalcWidth;
                width += marginBetween;
                if (!thumb.LastRow) {
                    if (thumb.LastColumn)
                        break;
                } else
                    break;
            }
            if (width > 0)
                width -= marginBetween;
            return width;
        }

        private static List<PLThumb> ConvertSizesToPLThumbs(List<Size> childrenRects) {
            return childrenRects.Select(r => new PLThumb {
                Width = r.Width > 0 ? r.Width : 100,
                Height = r.Height > 0 ? r.Height : 100,
            }).ToList();
        }
    }
}
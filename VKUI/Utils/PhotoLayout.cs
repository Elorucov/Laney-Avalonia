using Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace VKUI.Utils {
    public class PhotoLayout {
        public static Tuple<List<Rect>, Size, List<bool[]>> Create(Size parentSize, List<Size> elementsSizes, double marginBetween) {
            if (elementsSizes.Count > 0) {
                List<PLThumb> PLThumbs = ConvertSizesToPLThumbs(elementsSizes);
                PhotoLayoutInternal.ProcessThumbnails(parentSize.Width, parentSize.Height, PLThumbs, marginBetween);
                List<Rect> formed = ConvertProcessedThumbsToRects(PLThumbs, marginBetween, parentSize.Width);
                double csw = Math.Round(formed.Last().Left + formed.Last().Width);
                double csh = Math.Round(formed.Last().Top + formed.Last().Height);

                List<bool[]> corners = new List<bool[]>();
                foreach (Rect rect in CollectionsMarshal.AsSpan(formed)) {
                    bool[] corner = new bool[4]; // top left, top right, bottom right, bottom left

                    double lw = Math.Round(rect.Left + rect.Width);
                    double th = Math.Round(rect.Top + rect.Height);

                    if (rect.Left == 0 && rect.Top == 0) corner[0] = true;
                    if (lw == csw && rect.Top == 0) corner[1] = true;
                    if (rect.Left == 0 && th == csh) corner[3] = true;
                    if (lw == csw && th == csh) corner[2] = true;
                    corners.Add(corner);
                    if (formed.Count == 6 && corners.Count == formed.Count) Debugger.Break();
                }

                return new Tuple<List<Rect>, Size, List<bool[]>>(formed, new Size(csw, csh), corners);
            } else {
                return null;
            }
        }

        private static List<Rect> ConvertProcessedThumbsToRects(List<PLThumb> thumbs, double marginBetween, double width) {
            List<Rect> rectList = new List<Rect>(thumbs.Count);
            double top = 0;
            double widthOfRow = CalculateWidthOfRow(thumbs, marginBetween);
            double num2 = width / 2.0 - widthOfRow / 2.0;
            double num3 = num2;
            for (int index = 0; index < thumbs.Count; ++index) {
                PLThumb thumb = thumbs[index];
                rectList.Add(new Rect(num3, top, thumb.CalcWidth, thumb.CalcHeight));

                if (!thumb.LastColumn && !thumb.LastRow) {
                    num3 += thumb.CalcWidth + marginBetween;
                } else if (thumb.LastRow) {
                    top += thumb.CalcHeight + marginBetween;
                } else if (thumb.LastColumn) {
                    num3 = num2;
                    top += thumb.CalcHeight + marginBetween;
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
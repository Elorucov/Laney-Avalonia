using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace VKUI.Utils {
    internal static class ListExtensions {
        internal static List<T> Sublist<T>(this List<T> list, int begin, int end) {
            List<T> list2 = new List<T>();
            for (int i = begin; i < end; i++) {
                list2.Add(list[i]);
            }
            return list2;
        }
    }

    internal class PhotoLayoutInternal {
        internal static void ProcessThumbnails(double maxWidth, double maxHeight, List<PLThumb> thumbs, double margin) {
            string photoRatioTypes = "";
            List<double> photoRatios = new List<double>();
            int count = thumbs.Count;
            foreach (PLThumb thumb in thumbs) {
                double ratio = thumb.GetRatio();
                photoRatioTypes += ratio > 1.2 ? 'w' : (ratio < 0.8 ? 'n' : 'q');
                photoRatios.Add(ratio);
            }

            double num1 = photoRatios.Count > 0 ? photoRatios.Sum() / (double)photoRatios.Count : 1.0;
            if (maxWidth < 0.0) {
                throw new ArgumentException($"maxWidth and maxHeight values must be greater than 0.");
            }
            double num4 = maxWidth / maxHeight;
            if (count == 1) {
                if (photoRatios[0] > 0.8)
                    thumbs[0].SetViewSize(maxWidth, maxWidth / photoRatios[0], false, false);
                else
                    thumbs[0].SetViewSize(maxHeight * photoRatios[0], maxHeight, false, false);
            } else if (count == 2) {
                if (photoRatioTypes == "ww" && num1 > 1.4 * num4 && photoRatios[1] - photoRatios[0] < 0.2) {
                    double width2 = maxWidth;
                    double height2 = Math.Min(width2 / photoRatios[0], Math.Min(width2 / photoRatios[1], (maxHeight - margin) / 2.0));
                    thumbs[0].SetViewSize(width2, height2, true, false);
                    thumbs[1].SetViewSize(width2, height2, false, false);
                } else if (photoRatioTypes == "ww" || photoRatioTypes == "qq") {
                    double width2 = (maxWidth - margin) / 2.0;
                    double height2 = Math.Min(width2 / photoRatios[0], Math.Min(width2 / photoRatios[1], maxHeight));
                    thumbs[0].SetViewSize(width2, height2, false, false);
                    thumbs[1].SetViewSize(width2, height2, false, false);
                } else {
                    double width2 = (maxWidth - margin) / photoRatios[1] / (1.0 / photoRatios[0] + 1.0 / photoRatios[1]);
                    double width3 = maxWidth - width2 - margin;
                    double height2 = Math.Min(maxHeight, Math.Min(width2 / photoRatios[0], width3 / photoRatios[1]));
                    thumbs[0].SetViewSize(width2, height2, false, false);
                    thumbs[1].SetViewSize(width3, height2, false, false);
                }
            } else if (count == 3) {
                if (photoRatioTypes == "www") {
                    double width2 = maxWidth;
                    double height2 = Math.Min(width2 / photoRatios[0], (maxHeight - margin) * 0.66);
                    thumbs[0].SetViewSize(width2, height2, true, false);
                    double width3 = (maxWidth - margin) / 2.0;
                    double height3 = Math.Min(maxHeight - height2 - margin, Math.Min(width3 / photoRatios[1], width3 / photoRatios[2]));
                    thumbs[1].SetViewSize(width3, height3, false, false);
                    thumbs[2].SetViewSize(width3, height3, false, false);
                } else {
                    double width2 = Math.Min(maxHeight * photoRatios[0], (maxWidth - margin) * 0.75);
                    thumbs[0].SetViewSize(width2, maxHeight, false, false);
                    double height2 = photoRatios[1] * (maxHeight - margin) / (photoRatios[2] + photoRatios[1]);
                    double height3 = maxHeight - height2 - margin;
                    double width3 = Math.Min(maxWidth - width2 - margin, Math.Min(height2 * photoRatios[2], height3 * photoRatios[1]));
                    thumbs[1].SetViewSize(width3, height3, false, true);
                    thumbs[2].SetViewSize(width3, height2, false, true);
                }
            } else if (count == 4) {
                if (photoRatioTypes == "wwww") {
                    double width2 = maxWidth;
                    double height2 = Math.Min(width2 / photoRatios[0], (maxHeight - margin) * 0.66);
                    thumbs[0].SetViewSize(width2, height2, true, false);
                    double val2 = (maxWidth - 2.0 * margin) / (photoRatios[1] + photoRatios[2] + photoRatios[3]);
                    double width3 = val2 * photoRatios[1];
                    double width4 = val2 * photoRatios[2];
                    double width5 = val2 * photoRatios[3];
                    double height3 = Math.Min(maxHeight - height2 - margin, val2);
                    thumbs[1].SetViewSize(width3, height3, false, false);
                    thumbs[2].SetViewSize(width4, height3, false, false);
                    thumbs[3].SetViewSize(width5, height3, false, false);
                } else {
                    double width2 = Math.Min(maxHeight * photoRatios[0], (maxWidth - margin) * 0.66);
                    thumbs[0].SetViewSize(width2, maxHeight, false, false);
                    double heightModifier = (maxHeight - 2.0 * margin) / (1.0 / photoRatios[1] + 1.0 / photoRatios[2] + 1.0 / photoRatios[3]);
                    double height2 = heightModifier / photoRatios[1];
                    double height3 = heightModifier / photoRatios[2];
                    double height4 = heightModifier / photoRatios[3];
                    double width3 = Math.Min(maxWidth - width2 - margin, heightModifier);
                    thumbs[1].SetViewSize(width3, height2, false, true);
                    thumbs[2].SetViewSize(width3, height3, false, true);
                    thumbs[3].SetViewSize(width3, height4, false, true);
                }
            } else {
                List<double> photoRatios2 = new List<double>();
                if (num1 > 1.1) {
                    foreach (double ratio in CollectionsMarshal.AsSpan(photoRatios))
                        photoRatios2.Add(Math.Max(1.0, ratio));
                } else {
                    foreach (double ratio in CollectionsMarshal.AsSpan(photoRatios))
                        photoRatios2.Add(Math.Min(1.0, ratio));
                }
                Dictionary<string, List<double>> photosLayoutVariants = new Dictionary<string, List<double>>();
                int num5;
                photosLayoutVariants[string.Concat((num5 = count))] = new List<double>() {
                    CalculateMultiThumbsHeight(photoRatios2, maxWidth, margin)
                };
                for (int i = 1; i <= count - 1; ++i) {
                    int num6 = count - i;
                    photosLayoutVariants[$"{i},{num6}"] = new List<double>() {
                        CalculateMultiThumbsHeight(photoRatios2.Sublist(0, i), maxWidth, margin),
                        CalculateMultiThumbsHeight(photoRatios2.Sublist(i, photoRatios2.Count), maxWidth, margin)
                    };
                }
                for (int i = 1; i <= count - 2; ++i) {
                    for (int j = 1; j <= count - i - 1; ++j) {
                        int num6 = count - i - j;
                        photosLayoutVariants[$"{i},{j},{num6}"] = new List<double>() {
                                CalculateMultiThumbsHeight(photoRatios2.Sublist(0, i), maxWidth, margin),
                                CalculateMultiThumbsHeight(photoRatios2.Sublist(i, i + j), maxWidth, margin),
                                CalculateMultiThumbsHeight(photoRatios2.Sublist(i + j, photoRatios2.Count), maxWidth, margin)
                            };
                    }
                }
                string optimalPhotoLayout = null;
                double minHeightDiff = 0.0;
                foreach (string key in photosLayoutVariants.Keys) {
                    List<double> photosHeight = photosLayoutVariants[key];
                    double totalHeight = margin * (double)(photosHeight.Count - 1);
                    foreach (double height in photosHeight) totalHeight += height;
                    double heightDiff = Math.Abs(totalHeight - maxHeight);
                    if (key.IndexOf(",") != -1) {
                        string[] strArray = key.Split(',');
                        if (int.Parse(strArray[0]) > int.Parse(strArray[1]) || 
                            strArray.Length > 2 && 
                            int.Parse(strArray[1]) > int.Parse(strArray[2])) heightDiff *= 1.1;
                    }
                    if (optimalPhotoLayout == null || heightDiff < minHeightDiff) {
                        optimalPhotoLayout = key;
                        minHeightDiff = heightDiff;
                    }
                }
                List<PLThumb> PLThumbList1 = new List<PLThumb>(thumbs);
                string[] optimalPhotoLayoutArray = optimalPhotoLayout.Split(',');
                List<double> rowHeights = photosLayoutVariants[optimalPhotoLayout];
                int photoIndex = 0;

                for (int column = 0; column < optimalPhotoLayoutArray.Length; ++column) {
                    int photosInRow = int.Parse(optimalPhotoLayoutArray[column]);
                    List<PLThumb> PLThumbList2 = new List<PLThumb>();
                    for (int row = 0; row < photosInRow; ++row) {
                        PLThumbList2.Add(PLThumbList1[0]);
                        PLThumbList1.RemoveAt(0);
                    }
                    double num8 = rowHeights[photoIndex];
                    ++photoIndex;
                    int num9 = PLThumbList2.Count - 1;
                    for (int index2 = 0; index2 < PLThumbList2.Count; ++index2) {
                        PLThumb PLThumb = PLThumbList2[index2];
                        double ratio = photoRatios2[0];
                        photoRatios2.RemoveAt(0);
                        double width = ratio * num8;
                        double height = num8;
                        int num11 = index2 == num9 ? 1 : 0;
                        int num12 = 0;
                        PLThumb.SetViewSize(width, height, num11 != 0, num12 != 0);
                    }
                }

                //for (int column = 0; column < optimalPhotoLayoutArray.Length; column++) {
                //    int photosInRow = int.Parse(optimalPhotoLayoutArray[column]);
                //    double rowHeight = rowHeights[column];

                //    for (int row = 0; row < photosInRow; row++) {
                //        int index = photoIndex++;
                //        double width = photoRatios[index] * rowHeight;
                //        bool lastColumn = row == photosInRow - 1;
                //        bool lastRow = column == optimalPhotoLayoutArray.Length - 1;

                //        thumbs[index].SetViewSize(width, rowHeight, lastColumn, lastRow);
                //    }
                //}
            }
        }

        private static double CalculateMultiThumbsHeight(List<double> ratios, double width, double margin) {
            return (width - (ratios.Count - 1) * margin) / ratios.Sum();
        }
    }
}

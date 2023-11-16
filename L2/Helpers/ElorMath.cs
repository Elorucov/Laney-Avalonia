using System;

namespace ELOR.Laney.Helpers {
    public class ElorMath {
        public static double Resize(double sourceWidth, double sourceHeight, double targetWidth, double targetHeight, out double resizedWidth, out double resizedHeight, bool uniform = false) {
            double sw = targetWidth / sourceWidth;
            double sh = targetHeight / sourceHeight;
            double zoom = uniform ? Math.Min(sw, sh) : Math.Max(sw, sh);

            resizedWidth = Math.Ceiling(sourceWidth * zoom);
            resizedHeight = Math.Ceiling(sourceHeight * zoom);
            return zoom;
        }

        public static bool IsLargeOrEqualThanMax(double width, double height, double maxWidth, double maxHeight) {
            //if (width > maxWidth && height > maxHeight) return true;
            //if (width < maxWidth && height < maxHeight) return false;
            double rw = 0, rh = 0;
            double zoom = Resize(width, height, maxWidth, maxHeight, out rw, out rh);

            return zoom < 1;
        }
    }
}
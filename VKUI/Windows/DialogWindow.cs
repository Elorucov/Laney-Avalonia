using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Runtime.InteropServices;

namespace VKUI.Windows {
    public class DialogWindow : Window {
        public DialogWindow() {
            Classes.Add("Dialog");
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Activated += DialogWindow_Activated;
        }

        private void DialogWindow_Activated(object sender, EventArgs e) {
            Activated -= DialogWindow_Activated;
            SizeChanged += DialogWindow_SizeChanged;

            double diffx = ClientSize.Width - DesiredSize.Width;
            double diffy = ClientSize.Height - DesiredSize.Height;

            int movex = Position.X + (int)(diffx / 2);
            int movey = Position.Y + (int)(diffy / 2);

            FixSize();
            this.Position = new PixelPoint(movex, movey);
        }

        private void DialogWindow_SizeChanged(object sender, SizeChangedEventArgs e) {
            SizeChanged -= DialogWindow_SizeChanged;
            FixSize();
        }

        // This temporary fix written specially for https://github.com/AvaloniaUI/Avalonia/issues/17202
        bool isSizeChangedFirstTime = false;
        private void FixSize() {
            if (isSizeChangedFirstTime || SizeToContent == SizeToContent.WidthAndHeight) return;
            double addx = 0, addy = 0;
            double titleBarHeight = 0;
            

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                addx = addy = 1;
                titleBarHeight = 39;
            }

            if (MinWidth != 0 && MinWidth != Double.NaN) MinWidth += addx * 2;
            if (MinHeight != 0 && MinHeight != Double.NaN) MinHeight += addy * 2;
            if (MinHeight != 0 && MinHeight != Double.NaN) MinHeight -= titleBarHeight; // Standart window titlebar's height.

            if (MaxWidth != 0 && MaxWidth != Double.NaN) MaxWidth += addx * 2;
            if (MaxHeight != 0 && MaxHeight != Double.NaN) MaxHeight += addy * 2;
            // if (MaxHeight != 0 && MaxHeight != Double.NaN) MaxHeight -= titleBarHeight;

            if (SizeToContent == SizeToContent.Width && Width != 0 && Width != Double.NaN) Width += addx * 2;
            if (SizeToContent == SizeToContent.Height && Height != 0 && Height != Double.NaN) Height += addy * 2;
            // if (Height != 0 && Height != Double.NaN) Height -= titleBarHeight;

            var root = VisualChildren[0] as Panel;
            if (root == null) return;
            foreach (var child in root.Children) { 
                if (child is VisualLayerManager vlm) {
                    vlm.Padding = new Thickness(addx);
                    break;
                }
            }
            isSizeChangedFirstTime = true;
        }
    }
}
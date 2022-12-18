using Avalonia.Controls;
using System;
using System.Runtime.InteropServices;

namespace VKUI.Windows {
    public class DialogWindow : Window {
        public DialogWindow() {
            Classes.Add("Dialog");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Classes.Add("WinFix");
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
            PlatformImpl.Move(new Avalonia.PixelPoint(movex, movey));
        }

        private void DialogWindow_SizeChanged(object sender, SizeChangedEventArgs e) {
            SizeChanged -= DialogWindow_SizeChanged;
            FixSize();
        }

        private void FixSize() {
            double addx = 0, addy = 0;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                addx = addy = 1;
            }

            double diffx = ClientSize.Width - DesiredSize.Width;
            double diffy = ClientSize.Height - DesiredSize.Height;

            double finalx = DesiredSize.Width - diffx;
            double finaly = DesiredSize.Height - diffy;

            MinWidth = finalx;
            MinHeight = finaly;

            MaxWidth = finalx + addx; 
            MaxHeight = finaly + addy;

            PlatformImpl.Resize(new Avalonia.Size(MaxWidth, MaxHeight));
        }
    }
}
using Avalonia.Controls;
using System;

namespace VKUI.Windows {
    public class DialogWindow : Window {
        public DialogWindow() {
            Classes.Add("Dialog");
            Activated += DialogWindow_Activated;
        }


        private void DialogWindow_Activated(object sender, EventArgs e) {
            Activated -= DialogWindow_Activated;

            double diffx = ClientSize.Width - DesiredSize.Width;
            double diffy = ClientSize.Height - DesiredSize.Height;

            int movex = Position.X + (int)(diffx / 2);
            int movey = Position.Y + (int)(diffy / 2);

            double finalx = DesiredSize.Width - diffx;
            double finaly = DesiredSize.Height - diffy;

            MinWidth = MaxWidth = finalx; // For Linux
            MinHeight = MaxHeight = finaly; // For Linux
            PlatformImpl.Resize(new Avalonia.Size(finalx, finaly));
            PlatformImpl.Move(new Avalonia.PixelPoint(movex, movey));
        }
    }
}
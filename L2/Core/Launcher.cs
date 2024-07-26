using Avalonia.Controls;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public static class Launcher {
        public static async Task<bool> LaunchUrl(string url) {
            return await LaunchUrl(new Uri(url));
        }

        public static async Task<bool> LaunchUrl(Uri uri) {
            return await TopLevel.GetTopLevel(VKSession.Main.Window).Launcher.LaunchUriAsync(uri);
        }

        public static bool LaunchFolder(string path) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Process p = Process.Start(new ProcessStartInfo("explorer", path) { CreateNoWindow = true });
                return p != null;
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                Process p = Process.Start("xdg-open", path); // NOT TESTED!
                return p != null;
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                Process p = Process.Start("open", path); // NOT TESTED!
                return p != null;
            } else {
                return false;
            }
        }
    }
}
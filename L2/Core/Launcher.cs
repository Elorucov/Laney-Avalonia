using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ELOR.Laney.Core {
    public static class Launcher {
        public static bool LaunchUrl(string url) {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                url = url.Replace("&", "^&");
                Process p = Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                return p != null;
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                Process p = Process.Start("xdg-open", url);
                return p != null;
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                Process p = Process.Start("open", url);
                return p != null;
            } else {
                return false;
            }
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
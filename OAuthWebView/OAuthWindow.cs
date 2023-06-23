#if WIN
#else
using SpiderEye;
#endif
using System.Drawing;

namespace OAuthWebView {
    public class OAuthWindow {
        Uri startUri;
        Uri endUri;
        string title;
        System.Drawing.Size size;
        public string LocalDataPath { get; set; } // Only Windows, for WebView2

        Uri currentUri;
        ManualResetEventSlim mres;
#if WIN
        OAuthWindowWin32 w32;
#else
	    Window window;
#endif

        public OAuthWindow(Uri startUri, Uri endUri, string windowTitle, int width, int height) {
            this.startUri = startUri;
            this.endUri = endUri;
            title = windowTitle;
            size = new System.Drawing.Size(width, height);
        }

        public async Task<Uri> StartAuthenticationAsync() {
#if WIN
            w32 = new OAuthWindowWin32();
            w32.NavigationStarting += W32_NavigationStarting;
            Rectangle rect = new Rectangle(64, 64, Convert.ToInt32(size.Width), Convert.ToInt32(size.Height));
            int res = w32.Start(startUri.ToString(), rect, LocalDataPath);

            Console.WriteLine($"win32 result: {res}. Now returning URI.");
            System.Diagnostics.Debug.WriteLine($"win32 result: {res}. Now returning URI.");
            return currentUri.AbsolutePath == endUri.AbsolutePath ? currentUri : null;
#else
            return await StartAuthenticationAsyncNonWin();
#endif
        }

        static bool appInitialized = false;

#if WIN
#else
        public async Task<Uri> StartAuthenticationAsyncNonWin() {
            if (!appInitialized) {
#if LINUX
                SpiderEye.Linux.LinuxApplication.Init();
#elif MAC
                SpiderEye.Mac.MacApplication.Init();
#endif

                appInitialized = true;
            }

            mres = new ManualResetEventSlim();
            Thread thread = Thread.CurrentThread;
            Console.WriteLine($"Current thread id: {thread.ManagedThreadId}");
            System.Diagnostics.Debug.WriteLine($"Current thread id: {thread.ManagedThreadId}");


            window = new Window() {
#if LINUX
                CanResize = true,
#else
                CanResize = false,
#endif
                Title = title,
                Size = new SpiderEye.Size(size.Width, size.Height),
                MinSize = new SpiderEye.Size(size.Width, size.Height),
                MaxSize = new SpiderEye.Size(size.Width, size.Height)
            };
            window.Navigating += Window_Navigating;
            window.Closed += Window_Closed;
            window.Show();
            window.LoadUrl(startUri.ToString());
#if LINUX

            Console.WriteLine($"Running application...");
            Application.Run();
#else
            await Task.Factory.StartNew(() => {
                Console.WriteLine($"Waiting MRES...");
                mres.Wait();
            }).ConfigureAwait(true);
#endif

            // Cannot dispose the window object yet...
            Console.WriteLine($"MRES is set! Now returning URI.");
            mres.Dispose();
            return currentUri.AbsolutePath == endUri.AbsolutePath ? currentUri : null;
        }

        private void Window_Closed(object sender, EventArgs e) {
            window.Closed -= Window_Closed;
            window.Navigating -= Window_Navigating;
            Console.WriteLine($"Window closed.");
            mres.Set();
        }
#endif

#if WIN
        private void W32_NavigationStarting(object sender, string url) {
            Console.WriteLine($"Navigating to {url}");
            System.Diagnostics.Debug.WriteLine($"Navigating to {url}");
            currentUri = new Uri(url);
            if (currentUri.AbsolutePath == endUri.AbsolutePath) {
                w32.Destroy();
            }
        }
#else
        private void Window_Navigating(object sender, NavigatingEventArgs e) {
            Console.WriteLine($"Navigating to {e.Url}");
            currentUri = e.Url;
            if (currentUri.AbsolutePath == endUri.AbsolutePath) {
                window.Close();
            }
        }
#endif
    }
}

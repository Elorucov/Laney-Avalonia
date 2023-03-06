using SpiderEye;

namespace OAuthWebView {
    public class OAuthWindow {
        Uri startUri;
        Uri endUri;
        string title;
        Size size;

        Uri currentUri;
        ManualResetEventSlim mres;
        Window window;

        public OAuthWindow(Uri startUri, Uri endUri, string windowTitle, double width, double height) {
            this.startUri = startUri;
            this.endUri = endUri;
            title = windowTitle;
            size = new Size(width, height);
        }

        static bool appInitialized = false;

        public async Task<Uri> StartAuthenticationAsync() {
            if (!appInitialized) {
#if WIN
                SpiderEye.Windows.WindowsApplication.Init();
#elif LINUX
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
                Size = size,
                MinSize = size,
                MaxSize = size
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

        private void Window_Navigating(object sender, NavigatingEventArgs e) {
            Console.WriteLine($"Navigating to {e.Url}");
            currentUri = e.Url;
            if (currentUri.AbsolutePath == endUri.AbsolutePath) {
                window.Close();
            }
        }
    }
}
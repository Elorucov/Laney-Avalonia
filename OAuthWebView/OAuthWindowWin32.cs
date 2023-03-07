using Microsoft.Web.WebView2.Core;
using System.Drawing;
using System.Runtime.InteropServices;
using static Vanara.PInvoke.Kernel32;
using static Vanara.PInvoke.User32;
using Vanara.PInvoke;
using System.Collections.Concurrent;
using System.Reflection;

namespace OAuthWebView {
    internal sealed class UiThreadSynchronizationContext : SynchronizationContext {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, nuint wParam, nuint lParam);

        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> m_queue = new();
        private readonly HWND hwnd;

        public UiThreadSynchronizationContext(HWND hwnd) : base() {
            this.hwnd = hwnd;
        }

        public override void Post(SendOrPostCallback d, object state) {
            m_queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
            User32.PostMessage(hwnd, User32.WM_USER + 1, IntPtr.Zero, IntPtr.Zero);
        }

        public override void Send(SendOrPostCallback d, object state) {
            m_queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
            // User32.SendMessage(hwnd, User32.WM_USER + 1, 0, 0);
            SendMessage(hwnd.DangerousGetHandle(), User32.WM_USER + 1, 0, 0);
        }

        public void RunAvailableWorkOnCurrentThread() {
            while (m_queue.TryTake(out KeyValuePair<SendOrPostCallback, object> workItem))
                workItem.Key(workItem.Value);
        }

        public void Complete() { m_queue.CompleteAdding(); }
    }

    internal class OAuthWindowWin32 {
        private static CoreWebView2Controller _controller;
        private static UiThreadSynchronizationContext _uiThreadSyncCtx;
        HWND hwnd;

        [STAThread]
        internal int Start(string url, Rectangle pos, string localDataPath) {
            HINSTANCE hInstance = GetModuleHandle(String.Empty);
            ushort classId;


            WNDCLASS wc = new() {
                lpfnWndProc = WndProc,
                lpszClassName = "OAuth",
                hInstance = hInstance,
                hbrBackground = HBRUSH.NULL,
                style = WindowClassStyles.CS_VREDRAW | WindowClassStyles.CS_HREDRAW
            };
            classId = RegisterClass(wc);

            if (classId == 0) throw new Exception("class not registered");

            hwnd = CreateWindowEx(
                    0,
                    "OAuth",
                    "OAuth",
                    WindowStyles.WS_GROUP | WindowStyles.WS_CAPTION | WindowStyles.WS_SYSMENU,
                    pos.X, pos.Y, pos.Width, pos.Height,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    hInstance,
                    IntPtr.Zero);

            if (hwnd.DangerousGetHandle() == IntPtr.Zero) throw new Exception("hwnd not created");

            // Disable minimize button
            SetWindowLong(hwnd, WindowLongFlags.GWL_STYLE, GetWindowLong(hwnd, WindowLongFlags.GWL_STYLE) & ~(int)WindowStyles.WS_MINIMIZEBOX);

            ShowWindow(hwnd, ShowWindowCommand.SW_NORMAL);

            _uiThreadSyncCtx = new UiThreadSynchronizationContext(hwnd);
            SynchronizationContext.SetSynchronizationContext(_uiThreadSyncCtx);

            // Start initializing WebView2 in a fire-and-forget manner. Errors will be handled in the initialization function
            _ = CreateCoreWebView2Async(hwnd, url, localDataPath);

            Console.WriteLine("Starting message pump...");
            MSG msg;
            while (GetMessage(out msg, IntPtr.Zero, 0, 0) > 0) {
                Console.WriteLine($"Message: {msg}");
                TranslateMessage(msg);
                DispatchMessage(msg);
            }

            _controller?.Close();

            return (int)msg.wParam;
        }

        internal event EventHandler<string> NavigationStarting;

        internal void Destroy() {
            DestroyWindow(hwnd);
        }

        private nint WndProc(HWND hwnd, uint uMsg, nint wParam, nint lParam) {
            switch (uMsg) {
                case (uint)WindowMessage.WM_SIZE:
                    OnSize(hwnd, wParam, GetLowWord(lParam), GetHighWord(lParam));
                    break;
                case WM_USER + 1:
                    _uiThreadSyncCtx.RunAvailableWorkOnCurrentThread();
                    break;
                case (uint)WindowMessage.WM_CLOSE:
                    DestroyWindow(hwnd);
                    break;
                case (uint)WindowMessage.WM_DESTROY:
                    PostQuitMessage();
                    break;
            }

            return DefWindowProc(hwnd, uMsg, wParam, lParam);
        }

        private void OnSize(HWND hwnd, nint wParam, int width, int height) {
            if (_controller != null)
                _controller.Bounds = new Rectangle(0, 0, width, height);
        }

        private async Task CreateCoreWebView2Async(HWND hwnd, string startUrl, string localDataPath) {
            try {
                Console.WriteLine("Initializing WebView2...");
                var environment = await CoreWebView2Environment.CreateAsync(null, localDataPath, null);
                _controller = await environment.CreateCoreWebView2ControllerAsync(hwnd.DangerousGetHandle());

                _controller.DefaultBackgroundColor = Color.Transparent; // avoids flash of white when page first renders
                _controller.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
                _controller.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
                GetClientRect(hwnd, out var hwndRect);
                _controller.Bounds = new Rectangle(0, 0, hwndRect.right, hwndRect.bottom);
                _controller.IsVisible = true;
                _controller.CoreWebView2.Navigate(startUrl);

                Console.WriteLine("WebView2 initialization succeeded.");
            } catch (WebView2RuntimeNotFoundException) {
                var result = MessageBox(hwnd, "WebView2 runtime not installed.", "Error", MB_FLAGS.MB_OK | MB_FLAGS.MB_ICONERROR);

                if (result == MB_RESULT.IDYES) {
                    //TODO: download WV2 bootstrapper from https://go.microsoft.com/fwlink/p/?LinkId=2124703 and run it
                }

                PostQuitMessage();
            } catch (Exception ex) {
                MessageBox(hwnd, $"Failed to initialize WebView2:{Environment.NewLine}{ex}", "Error", MB_FLAGS.MB_OK | MB_FLAGS.MB_ICONERROR);
                PostQuitMessage();
            }
        }

        private void CoreWebView2_DocumentTitleChanged(object sender, object e) {
            string title = _controller.CoreWebView2.DocumentTitle;
            SetWindowText(hwnd, title);
        }

        private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e) {
            NavigationStarting?.Invoke(this, e.Uri);
        }

        private int GetLowWord(nint value) {
            uint xy = (uint)value;
            int x = unchecked((short)xy);
            return x;
        }

        private int GetHighWord(nint value) {
            uint xy = (uint)value;
            int y = unchecked((short)(xy >> 16));
            return y;
        }
    }
}

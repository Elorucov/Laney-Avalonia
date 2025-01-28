using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Network;
using Serilog;

namespace ELOR.Laney {
    enum LaunchMode { Default, APIConsole }

    class Program {
        static Stopwatch stopwatch;
        public static long LaunchTime { get { return stopwatch.ElapsedMilliseconds; } }
        public static LaunchMode Mode;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) {
            stopwatch = Stopwatch.StartNew();
            int delay = 0;
            Mode = LaunchMode.Default;

            if (App.HasCmdLineValue("apiconsole")) Mode = LaunchMode.APIConsole;

            // Создаём локальную папку для хранения настроек и данных.
            string localDataPath = App.LocalDataPath;
            if (!Directory.Exists(localDataPath)) Directory.CreateDirectory(localDataPath);

            // Logger
            var loggerConfig = new LoggerConfiguration()
#if RELEASE
                .MinimumLevel.Information();
#elif BETA
                .MinimumLevel.Information();
#else
                .MinimumLevel.Verbose();
#endif

            if (Settings.EnableLogs)
                loggerConfig = loggerConfig.WriteTo.File(Path.Combine(localDataPath, "logs", $"L2_{DateTimeOffset.Now.ToUnixTimeSeconds()}.log"),
                    buffered: true, retainedFileCountLimit: 20, flushToDiskInterval: TimeSpan.FromSeconds(20));

            // Log.Logger = loggerConfig.CreateLogger();
            Log.Information("Laney is starting up. Build: {0}, Repo: {1}", App.BuildInfo, App.RepoInfo);
            Log.Information("Launch mode: {0}", Mode);
            Log.Information("Local data folder: {0}", localDataPath);
            Log.Information("Is ChaCha20Poly1305 supported: {0}", Encryption.IsChaCha20Poly1305Supported);

            // Delay (нужен при перезапуске приложения)
            Int32.TryParse(App.GetCmdLineValue("delay"), out delay);
            if (delay > 0) {
                Task.Delay(delay).RunSynchronously();
                Log.Information("Launched with delay flag ({0} ms)", delay);
            }

            if (Mode == LaunchMode.Default) {
                // в macOS нельзя вроде запустить более одного процесса одной программы.
                if (App.Platform == OSPlatform.OSX) {
                    Settings.Initialize();
                } else {
                    if (!Design.IsDesignMode && IsAlreadyRunning()) {
                        Log.Warning("Laney is already launched, quitting.");
                        Process.GetCurrentProcess().Kill();
                    }
                }
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#if RELEASE
#else
            LNet.DebugLog += (a, b) => Log.Information($"[LNET] {b}");
#endif

            try {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
            } catch (Exception ex) {
                Log.Fatal(ex, "App crashed! (TC)\n");
                Log.CloseAndFlush();
                Process.GetCurrentProcess().Kill();
            } finally {
                Log.Information("App closed.\n");
                Log.CloseAndFlush();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Exception ex = e.ExceptionObject as Exception;

            Log.Fatal(ex, "App crashed! (UE)\n");
            Log.CloseAndFlush();
            Process.GetCurrentProcess().Kill();
        }

        private static bool IsAlreadyRunning() {
            try {
                Settings.Initialize();
                return false;
            } catch {
                return true;
            }
        }

        public static void StopStopwatch() {
            stopwatch.Stop();
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>().UseAvaloniaNative().UsePlatformDetect()
            .With(new SkiaOptions {
                UseOpacitySaveLayer = true,
                MaxGpuResourceSizeBytes = 1073741824
            //}).With(new Win32PlatformOptions { 
            //    RenderingMode = new List<Win32RenderingMode> { Win32RenderingMode.Vulkan, Win32RenderingMode.Wgl, Win32RenderingMode.Software },
            //    CompositionMode = new List<Win32CompositionMode> { Win32CompositionMode.WinUIComposition, Win32CompositionMode.LowLatencyDxgiSwapChain, Win32CompositionMode.DirectComposition },
            }).With(new X11PlatformOptions {
                RenderingMode = new List<X11RenderingMode> { X11RenderingMode.Vulkan, X11RenderingMode.Glx, X11RenderingMode.Software }
            });
    }
}

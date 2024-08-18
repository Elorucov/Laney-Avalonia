using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Network;
using ELOR.Laney.Extensions;
using Serilog;

namespace ELOR.Laney {
    class Program {
        static Stopwatch stopwatch;
        public static long LaunchTime { get { return stopwatch.ElapsedMilliseconds; } }

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) {
            stopwatch = Stopwatch.StartNew();
            int delay = 0;

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
                loggerConfig = loggerConfig.WriteTo.File(Path.Combine(localDataPath, "logs", $"L2_{DateTimeOffset.Now.ToUnixTimeSeconds()}.log"));

            Log.Logger = loggerConfig.CreateLogger();
            Log.Information("Laney is starting up. Build tag: {0}", App.BuildInfoFull);
            Log.Information("Local data folder: {0}", localDataPath);
            Log.Information("Is ChaCha20Poly1305 supported: {0}", Encryption.IsChaCha20Poly1305Supported);

            // Delay (нужен при перезапуске приложения)
            Int32.TryParse(App.GetCmdLineValue("delay"), out delay);
            if (delay > 0) {
                Task.Delay(delay).RunSynchronously();
                Log.Information("Launched with delay flag ({0} ms)", delay);
            }

            // в macOS нельзя вроде запустить более одного процесса одной программы.
            if (App.Platform == OSPlatform.OSX) {
                Settings.Initialize();
            } else {
                if (!Design.IsDesignMode && IsAlreadyRunning()) {
                    Log.Warning("Laney is already launched, quitting.");
                    Process.GetCurrentProcess().Kill();
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
                MaxGpuResourceSizeBytes = 33554432
            });
    }
}

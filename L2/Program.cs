using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using ELOR.Laney.Core;
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

            // Создаём локальную папку для хранения настроек и данных.
            string localDataPath = App.LocalDataPath;
            if (!Directory.Exists(localDataPath)) Directory.CreateDirectory(localDataPath);

            // в macOS нельзя вроде запустить более одного процесса одной программы.
            if (App.Platform == OSPlatform.OSX) {
                Settings.Initialize();
            } else {
                if (!Design.IsDesignMode && IsAlreadyRunning()) Process.GetCurrentProcess().Kill();
            }

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Information();

            if (Settings.EnableLogs) 
                loggerConfig = loggerConfig.WriteTo.File(Path.Combine(localDataPath, "logs", "L2_.log"), rollingInterval: RollingInterval.Hour, retainedFileCountLimit: 10);

            Log.Logger = loggerConfig.CreateLogger();
            Log.Information("Laney is starting up. Build tag: {0}", App.BuildInfoFull);
            Log.Information("Local data folder: {0}", localDataPath);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnMainWindowClose);
            } finally {
                Log.Information("App closed.\n");
                Log.CloseAndFlush();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Exception ex = e.ExceptionObject as Exception;

            Log.Fatal(ex, "App crashed!\n");
            Log.CloseAndFlush();
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
            .With(new Win32PlatformOptions {
                UseWgl = true,
                UseWindowsUIComposition = true
            })
            .With(new SkiaOptions {
                MaxGpuResourceSizeBytes = 33554432
            })
            .With(new AvaloniaNativePlatformOptions {
                UseCompositor = true,
            });

        //public static AppBuilder BuildAvaloniaApp() {
        //    #if WIN
        //        return AppBuilder.Configure<App>().UseWin32().UseDirect2D1();
        //    #else
        //        return AppBuilder.Configure<App>().UseAvaloniaNative().UsePlatformDetect();
        //    #endif
        //}
    }
}

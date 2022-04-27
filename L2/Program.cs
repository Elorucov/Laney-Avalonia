using System;
using System.IO;
using Avalonia;
using Serilog;

namespace ELOR.Laney {
    class Program {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) {
            // Создаём локальную папку для хранения настроек и данных.
            string localDataPath = App.LocalDataPath;
            if (!Directory.Exists(localDataPath)) Directory.CreateDirectory(localDataPath);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(Path.Combine(localDataPath, "logs", "L2_.log"), rollingInterval: RollingInterval.Day)
                .CreateLogger();
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

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>().UseAvaloniaNative().UsePlatformDetect();

        //public static AppBuilder BuildAvaloniaApp() {
        //    #if WIN
        //        return AppBuilder.Configure<App>().UseWin32().UseDirect2D1();
        //    #else
        //        return AppBuilder.Configure<App>().UseAvaloniaNative().UsePlatformDetect();
        //    #endif
        //}
    }
}
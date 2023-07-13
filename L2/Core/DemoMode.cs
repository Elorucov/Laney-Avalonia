using ELOR.Laney.DataModels;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ELOR.Laney.Core {
    public class DemoMode {
        public static bool IsEnabled { get; private set; }
        public static DemoModeData Data { get; private set; }

        public static bool Check() {
            Log.Information("Checking if app is running in demo mode...");
            string path = Path.Combine(App.LocalDataPath, "demo.json");
            if (!File.Exists(path)) {
                Log.Information("File for demo mode is not found, skipping.");
                return false;
            }

            try {
                FileStream file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 4096);

                byte[] fileBytes = new byte[file.Length];
                file.Read(fileBytes, 0, fileBytes.Length);

                UTF8Encoding enc = new UTF8Encoding(true);
                string content = enc.GetString(fileBytes);

                if (content.Length == 0) {
                    Log.Warning("File for demo mode is empty! skipping.");
                    return false;
                }

                Data = JsonConvert.DeserializeObject<DemoModeData>(content);
                Log.Information("File for demo mode is found and successfully loaded!");
                IsEnabled = true;
                return true;
            } catch (FileNotFoundException) {
                Log.Warning("File for demo mode is not found, skipping.");
                return false;
            } catch (Exception ex) {
                Log.Error(ex, "An error ocured while trying to open and parse file for demo mode!");
                return false;
            }
        }

        public static DemoModeSession GetDemoSessionById(long id) {
            return DemoMode.Data.Sessions.Where(s => s.Id == id).FirstOrDefault();
        }
    }
}
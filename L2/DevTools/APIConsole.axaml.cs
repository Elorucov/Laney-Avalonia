using Avalonia.Controls;
using Avalonia.Threading;
using ELOR.Laney.Core;
using ELOR.Laney.DataModels;
using ELOR.Laney.Views.Modals;
using ELOR.VKAPILib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace ELOR.Laney;

public partial class APIConsoleWindow : Window {
    ObservableCollection<TwoStringBindable> Parameters = new ObservableCollection<TwoStringBindable>();
    VKAPI API;
    DispatcherTimer timer = new DispatcherTimer {
        Interval = TimeSpan.FromSeconds(0.5),
    };

    public APIConsoleWindow() {
        InitializeComponent();
        ParametersItems.ItemsSource = Parameters;
        Activated += APIConsoleWindow_Activated;
        AppVersion.Text = App.BuildInfo.Replace(" ", "\n");
    }

    private void APIConsoleWindow_Activated(object sender, EventArgs e) {
        Activated -= APIConsoleWindow_Activated;
        Version.Text = VKAPI.Version;

        // Test data
        Method.Text = "users.get";
        Parameters.Add(new TwoStringBindable {
            Item1 = "user_ids",
            Item2 = "172894294,168354935"
        });
        Parameters.Add(new TwoStringBindable {
            Item1 = "fields",
            Item2 = "photo_200,online_info"
        });

        // Method.Focus();
        timer.Tick += (a, b) => {
            if (TokenButton.Classes.Contains("Primary")) {
                TokenButton.Classes.Remove("Primary");
            } else {
                TokenButton.Classes.Add("Primary");
            }
        };

        // Try to get access_token from current session. If fail (for example, current app is running in normal mode), show flyout to set token.
        try {
            Settings.Initialize();

            string token = Settings.Get<string>(Settings.VK_TOKEN);
            string nonce = Settings.Get<string>(Settings.VK_TOKEN + "1");
            string tag = Settings.Get<string>(Settings.VK_TOKEN + "2");
            string dt = Encryption.Decrypt(AssetsManager.BinaryPayload.Skip(576).Take(32).OrderDescending().ToArray(), token, nonce, tag);

            if (dt == null) throw new Exception("Failed to get access_token from settings!");
            AccessToken.Text = dt;
            API = new VKAPI(dt, Lang.Text, App.UserAgent);
            Settings.UnlockSettingsFile(true);

            new Action(async () => {
                await Task.Delay(100); // Required for properly focus to "method" TextBox.
                Method.Focus();
            })();
        } catch (Exception) {
            TokenButton.Flyout.ShowAt(TokenButton);
        }
    }

    private void AddNewParameter(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        Parameters.Add(new TwoStringBindable());
    }

    private void ClearAllParameters(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        Parameters.Clear();
    }

    private void RemoveParameter(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        TwoStringBindable item = (sender as Control).DataContext as TwoStringBindable;
        Parameters.Remove(item);
    }

    private void CallMethod(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        new Action(async () => {
            CallButton.IsEnabled = false;
            try {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                foreach (var p in Parameters) {
                    if (p.Item1 == "v" || p.Item1 == "lang" || !p.Enabled) continue;
                    if (parameters.ContainsKey(p.Item1)) {
                        parameters[p.Item1] = p.Item2;
                    } else {
                        parameters.Add(p.Item1, p.Item2);
                    }
                }
                parameters.Add("v", Version.Text);
                parameters.Add("lang", Lang.Text);

                if (API == null) {
                    VKUIDialog dialog = new VKUIDialog("Access token not set!", "Without token you cannot call API methods");
                    await dialog.ShowDialog(this);
                    CallButton.IsEnabled = true;
                    return;
                }
                Response.Text = "Waiting response from VK...";

                using var response = await API.CallMethodAsync(Method.Text, parameters);
                string pretty = JsonSerializer.Serialize(response, new JsonSerializerOptions {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // To decoding cyrillic letters correctly
                    TypeInfoResolver = BuildInJsonContext.Default
                });
                Response.Text = pretty;
            } catch (Exception ex) {
                Response.Text = $"{ex.GetType()} 0x{ex.HResult.ToString("x8")}\n{ex.Message}";
            } finally {
                CallButton.IsEnabled = true;
            }
        })();
    }

    // Autofocus to parameter textbox
    private void OnParamTBLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        TextBox tb = sender as TextBox;
        tb.Focus();
    }

    private void VKUIFlyout_Opened(object? sender, EventArgs e) {
        timer.Stop();
        if (TokenButton.Classes.Contains("Primary")) TokenButton.Classes.Remove("Primary");
        TokenButton.Classes.Add("Primary");
        AccessToken.Focus();
    }

    private void VKUIFlyout_Closed(object? sender, EventArgs e) {
        new Action(async () => {
            string token = AccessToken.Text;
            if (!string.IsNullOrEmpty(token)) {
                API = new VKAPI(token, Lang.Text, App.UserAgent);
            } else {
                VKUIDialog dialog = new VKUIDialog("Access token not set!", "Without token you cannot call API methods");
                await dialog.ShowDialog(this);

                timer.Start();
            }
        })();
    }

    private void OnLangTextChanged(object? sender, TextChangedEventArgs e) {
        if (API != null) API.Language = Lang.Text;
    }
}
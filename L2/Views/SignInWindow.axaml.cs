using Avalonia.Controls;
using Serilog;

namespace ELOR.Laney.Views {
    public sealed partial class SignInWindow : Window {
        public SignInWindow() {
            InitializeComponent();
            Log.Information($"{nameof(SignInWindow)} initialized.");

            Activated += SignInWindow_Activated;
            Loaded += SignInWindow_Loaded;
        }

        private void SignInWindow_Activated(object sender, System.EventArgs e) {
            Activated -= SignInWindow_Activated;
            Program.StopStopwatch();
            Log.Information($"{nameof(SignInWindow)} activated. Launch time: {Program.LaunchTime} ms.");
        }

        private void SignInWindow_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            Loaded -= SignInWindow_Loaded;
            AuthFlow.NavigationRouter.NavigateToAsync(new SignIn.MainPage());
        }
    }
}
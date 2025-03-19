using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects.Auth;
using Serilog;
using System;
using VKUI.Controls;

namespace ELOR.Laney.Views.SignIn {
    public partial class DirectAuthPage : Page {
        public DirectAuthPage() {
            InitializeComponent();
            Log.Information($"{nameof(DirectAuthPage)} initialized.");
        }

        private void ShowError(string err) {
            ErrorInfo.IsVisible = !String.IsNullOrEmpty(err);
            if (!String.IsNullOrEmpty(err)) ErrorInfo.Text = err;
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e) {
            await NavigationRouter.BackAsync();
        }

        private void SignIn_Click(object sender, RoutedEventArgs e) {
            DoAuth();
        }

        string captchaSid = null;
        string captchaCode = null;
        private async void DoAuth() {
            if (String.IsNullOrEmpty(LoginBox.Text) || String.IsNullOrEmpty(PassBox.Text) || PassBox.Text.Length < 6) return;
            SignInButton.IsEnabled = false;
            ShowError(null);

            try {
                Log.Information($"{nameof(DirectAuthPage)}: sending credentials (have captcha: {!String.IsNullOrEmpty(captchaSid)})");
                var response = await AuthManager.AuthViaLoginAndPasswordAsync(LoginBox.Text, PassBox.Text, null, captchaSid, captchaCode);
                if (response.UserId.IsUser()) {
                    Log.Information($"{nameof(DirectAuthPage)}: success!");
                    await NavigationRouter.NavigateToAsync(new PostDirectAuthPage(response.UserId, response.AccessToken));
                } else if (!String.IsNullOrEmpty(response.Error)) {
                    HandleError(response);
                } else {
                    Log.Error($"{nameof(DirectAuthPage)}: VK auth returns a strange response!");
                    ShowError(Assets.i18n.Resources.error);
                }
            } catch (Exception ex) {
                var info = ExceptionHelper.GetDefaultErrorInfo(ex);
                Log.Error($"{nameof(DirectAuthPage)}: {info.Item1}: {info.Item2}");
                ShowError($"{info.Item1}\n{info.Item2}");
            }

            SignInButton.IsEnabled = true;
        }

        private async void HandleError(DirectAuthResponse err) {
            Log.Warning($"{nameof(DirectAuthPage)}: VK auth returns an error! {err.Error}: {err.ErrorDescription}");
            switch (err.Error) {
                case "invalid_client":
                    ShowError(!String.IsNullOrEmpty(err.ErrorDescription) ? err.ErrorDescription : $"{Assets.i18n.Resources.error}: {err.ErrorType}");
                    break;
                case "invalid_request":
                    ShowError(err.ErrorType == "wrong_otp" ? Assets.i18n.Resources.da_wrong_otp_code : $"{Assets.i18n.Resources.error}: {err.ErrorType}");
                    break;
                case "need_validation":
                    if (err.BanInfo != null) {
                        ShowError($"{err.BanInfo.MemberName}. {err.BanInfo.Message}");
                    } else {
                        if (!String.IsNullOrEmpty(err.ValidationType)) {
                            await NavigationRouter.NavigateToAsync(new DAValidationPage(LoginBox.Text, PassBox.Text, err));
                        } else {
                            ShowError($"Need validation, but not supported! {err.ErrorDescription}");
                        }
                    }
                    break;
                case "need_captcha":
                    captchaSid = err.CaptchaSid;
                    captchaCode = await VKSession.ShowCaptchaAsync(TopLevel.GetTopLevel(this) as Window, new Uri(err.CaptchaImg));
                    DoAuth();
                    break;
            }
        }
    }
}
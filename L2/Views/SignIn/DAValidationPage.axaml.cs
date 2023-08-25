using Avalonia.Controls;
using Avalonia.Interactivity;
using ELOR.Laney.Core;
using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib.Objects.Auth;
using Serilog;
using System;
using VKUI.Controls;

namespace ELOR.Laney.Views.SignIn {
    public partial class DAValidationPage : Page {
        public DAValidationPage() {
            InitializeComponent();
        }

        string login = null;
        string password = null;
        public DAValidationPage(string login, string password, DirectAuthResponse err) {
            InitializeComponent();
            Log.Information($"{nameof(DAValidationPage)} initialized.");

            this.login = login;
            this.password = password;

            SetupInfo(err);
        }

        private void SetupInfo(DirectAuthResponse err) {
            TwoFACode.Text = String.Empty;
            if (err.ValidationType == "2fa_app") {
                TwoFAInfo.Text = Localizer.Instance["da_2fa_app"];
            } else if (err.ValidationType == "2fa_sms") {
                TwoFAInfo.Text = Localizer.Instance.GetFormatted("da_2fa_sms", err.PhoneMask);
            } else {
                TwoFAInfo.Text = err.ErrorDescription;
            }
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
            SignInButton.IsEnabled = false;
            ShowError(null);

            try {
                Log.Information($"{nameof(DAValidationPage)}: sending credentials (have captcha: {!String.IsNullOrEmpty(captchaSid)})");
                var response = await AuthManager.AuthViaLoginAndPasswordAsync(login, password, TwoFACode.Text, captchaSid, captchaCode);
                if (response.UserId.IsUser()) {
                    Log.Information($"{nameof(DAValidationPage)}: success!");
                    await NavigationRouter.NavigateToAsync(new PostDirectAuthPage(response.UserId, response.AccessToken));
                } else if (!String.IsNullOrEmpty(response.Error)) {
                    HandleError(response);
                } else {
                    Log.Error($"{nameof(DAValidationPage)}: VK auth returns a strange response!");
                    ShowError(Localizer.Instance["error"]);
                }
            } catch (Exception ex) {
                var info = ExceptionHelper.GetDefaultErrorInfo(ex);
                Log.Error($"{nameof(DAValidationPage)}: {info.Item1}: {info.Item2}");
                ShowError($"{info.Item1}\n{info.Item2}");
            }

            SignInButton.IsEnabled = true;
        }

        private async void HandleError(DirectAuthResponse err) {
            Log.Warning($"{nameof(DAValidationPage)}: VK auth returns an error! {err.Error}: {err.ErrorDescription}");
            switch (err.Error) {
                case "invalid_client":
                    ShowError(!String.IsNullOrEmpty(err.ErrorDescription) ? err.ErrorDescription : $"{Localizer.Instance["error"]}: {err.ErrorType}");
                    break;
                case "invalid_request":
                    ShowError(err.ErrorType == "wrong_otp" ? Localizer.Instance["da_wrong_otp_code"] : $"{Localizer.Instance["error"]}: {err.ErrorType}");
                    break;
                case "need_validation":
                    if (err.BanInfo != null) {
                        ShowError($"{err.BanInfo.MemberName}. {err.BanInfo.Message}");
                    } else {
                        if (!String.IsNullOrEmpty(err.ValidationType)) {
                            SetupInfo(err);
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
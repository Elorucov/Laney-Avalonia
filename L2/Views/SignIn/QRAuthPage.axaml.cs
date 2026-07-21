using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ELOR.Laney.Core;
using ELOR.Laney.Extensions;
using ELOR.Laney.Helpers;
using ELOR.VKAPILib;
using ELOR.VKAPILib.Objects.Auth;
using System;
using System.Threading.Tasks;
using VKUI.Controls;

namespace ELOR.Laney.Views.SignIn {
    public partial class QRAuthPage : VKUI.Controls.Page {
        public QRAuthPage() {
            InitializeComponent();
        }

        VKAPI _api;
        string _authHash;

        private async void BackButton_Click(object sender, RoutedEventArgs e) {
            await NavigationRouter.BackAsync();
        }

        private async void Page_Loaded(object? sender, RoutedEventArgs e) {
            try {
                // Get VKAPI instance with anonym token
                _api = await DirectAuth.GetVKAPIWithAnonymTokenAsync(AuthManager.CLIENT_ID, AuthManager.CLIENT_SECRET, App.UserAgent, LNetExtensions.SendRequestToAPIViaLNetAsync);

                // Get auth code
                GetAuthCodeResponse codeResp = null;

                codeResp = await _api.Auth.GetAuthCodeAsync(Assets.i18n.Resources.lang, $"Laney {App.BuildInfo} on {App.Platform}", AuthManager.CLIENT_ID);

                QrCodeControl.Data = codeResp.AuthUrl;
                _authHash = codeResp.AuthHash;

                Loading.IsVisible = false;
                QrCodeControl.IsVisible = true;

                Check(codeResp.AuthHash);
            } catch (Exception ex) {
                await ExceptionHelper.ShowErrorDialogAsync(TopLevel.GetTopLevel(this) as Window, ex, true);
                await NavigationRouter.BackAsync();
            }
        }

        private async void Check(string authHash) {
            await Task.Factory.StartNew(async () => {
                bool isWorking = true;
                while (isWorking) {
                    await Task.Delay(1500).ConfigureAwait(false);
                    try {
                        var response = await _api.Auth.CheckAuthCodeAsync(Assets.i18n.Resources.lang,
                            AuthManager.CLIENT_ID,
                            authHash, false);

                        if (response.Status == 2 || response.Status == 3) isWorking = false;
                        await Dispatcher.UIThread.InvokeAsync(async () => {
                            switch (response.Status) {
                                case 1:
                                    Loading.IsVisible = true;
                                    QrCodeControl.IsVisible = false;
                                    OTPValidationArea.IsVisible = false;
                                    PageTitle.Text = Assets.i18n.Resources.qr_signin_p2_title;
                                    PageDesc.Text = Assets.i18n.Resources.qr_signin_p2_desc;
                                    break;
                                case 2:
                                    await NavigationRouter.NavigateToAsync(new PostDirectAuthPage(response.UserId, response.AccessToken));
                                    break;
                                case 3:
                                    await NavigationRouter.BackAsync();
                                    break;
                                case 5:
                                    Loading.IsVisible = false;
                                    QrCodeControl.IsVisible = false;
                                    OTPValidationArea.IsVisible = true;
                                    PageTitle.Text = Assets.i18n.Resources.qr_auth_otp;
                                    PageDesc.Text = Assets.i18n.Resources.qr_auth_otp_desc;
                                    break;
                                default:
                                    if (response.Status != 0) {
                                        PageDesc.Text = $"Status: {response.Status}";
                                        OTPValidationArea.IsVisible = false;
                                    }
                                    break;
                            }
                        });
                    } catch (Exception ex) {
                        isWorking = false;
                        await Dispatcher.UIThread.InvokeAsync(async () => {
                            await ExceptionHelper.ShowErrorDialogAsync(TopLevel.GetTopLevel(this) as Window, ex, true);
                            await NavigationRouter.BackAsync();
                        });
                        break;
                    }
                }
            });
        }

        private void ValidateOTP(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            new Action(async () => {
                try {
                    OTPErrorText.Text = string.Empty;
                    OTPButton.IsEnabled = false;
                    var response = await _api.Auth.ValidateAuthCodeAsync(_authHash, OTPCodeTB.Text);
                    if (response.Status == 0) {
                        OTPValidationArea.IsVisible = false;
                        Loading.IsVisible = true;
                    } else {
                        OTPErrorText.Text = $"Invalid status: {response.Status}";
                    }
                } catch (Exception ex) {
                    (string t, string d) = ExceptionHelper.GetDefaultErrorInfo(ex);
                    OTPErrorText.Text = $"{t}\n{d}";
                } finally {
                    OTPButton.IsEnabled = true;
                }
            })();
        }
    }
}
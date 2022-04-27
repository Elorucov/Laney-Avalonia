using ELOR.Laney.Core.Localization;
using ELOR.Laney.Extensions;
using ELOR.VKAPILib.Objects;
using System;
using System.IO;
using System.Net.Sockets;

namespace ELOR.Laney.Helpers {
    public static class ExceptionHelper {
        public static Tuple<string, string> GetDefaultErrorInfo(Exception ex) {
            Tuple<string, string> result = new Tuple<string, string>(String.Empty, String.Empty);
            if (ex is APIException apiEx) {
                string uem = VKAPIHelper.GetUnderstandableErrorMessage(apiEx.Code);
                result = new Tuple<string, string>($"{Localizer.Instance["error_api"]} ({apiEx.Code})", String.IsNullOrEmpty(uem) ? apiEx.Message : uem);
            } else if (ex is System.Net.Http.HttpRequestException httpex) {
                string terr = Localizer.Instance["err_network"];
                string nerr = String.Empty;
                if (httpex.StatusCode == null) {
                    if (httpex.InnerException != null) {
                        if (httpex.InnerException is IOException || httpex.InnerException is ObjectDisposedException) {
                            nerr = $"{Localizer.Instance["err_network_aborted"]}\n({httpex.InnerException.GetType()} {httpex.InnerException.HResultHEX()})";
                        } else if (httpex.InnerException is SocketException sex) {
                            switch (sex.ErrorCode) {
                                case 11001: nerr = $"{Localizer.Instance["err_network_no_connection"]}"; break; // No internet connection
                                default: nerr = $"{Localizer.Instance["err_network_general"]}\n(socketException code: {httpex.StatusCode})"; break;
                            }
                        }
                    } else {
                        nerr = $"{Localizer.Instance["err_network_general"]}\n(HResult: {httpex.HResultHEX()})";
                    }
                } else {
                    nerr = Localizer.Instance["err_network_general"];
                    switch (httpex.StatusCode) {
                        default: nerr = $"{Localizer.Instance["err_network_general"]}\n({httpex.StatusCode})"; break;
                        case System.Net.HttpStatusCode.InternalServerError:
                        case System.Net.HttpStatusCode.BadGateway:
                        case System.Net.HttpStatusCode.ServiceUnavailable:
                        case System.Net.HttpStatusCode.GatewayTimeout:
                            nerr = Localizer.Instance["err_network_shinaprovod"]; break;
                    }
                }
                result = new Tuple<string, string>(terr, nerr);
            } else {
                result = new Tuple<string, string>(Localizer.Instance["error"], $"{ex.Message.Trim()}\n(0x{ex.HResult.ToString("x8")})");
            }
            return result;
        }

        // From UWP version, need refactor.

        //public static async Task<bool> ShowErrorDialogAsync(Tuple<string, string> errorInfo, bool hideRetry = false) {
        //    Alert alert = new Alert {
        //        Header = errorInfo.Item1,
        //        Text = errorInfo.Item2,
        //        PrimaryButtonText = !hideRetry ? Locale.Get("retry") : null,
        //        SecondaryButtonText = Locale.Get("close"),
        //    };
        //    AlertButton result = await alert.ShowAsync();
        //    return result == AlertButton.Primary;
        //}

        //public static async Task<bool> ShowErrorDialogAsync(Exception ex, bool hideRetry = false) {
        //    if (ex is AggregateException agex) ex = agex.InnerException;
        //    Tuple<string, string> err = GetDefaultErrorInfo(ex);
        //    return await ShowErrorDialogAsync(err, hideRetry);
        //}
    }
}
using Avalonia.Controls;
using ELOR.Laney.Extensions;
using ELOR.Laney.Views.Modals;
using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ELOR.Laney.Helpers {
    public static class ExceptionHelper {
        public static bool IsExceptionAboutNoConnection(Exception ex) {
            HttpRequestException httpex = ex as HttpRequestException;
            if (httpex == null) return false;
            return httpex.InnerException != null &&
                (httpex.InnerException is SocketException sex && sex.ErrorCode == 11001) || // No internet/host
                (httpex.InnerException is IOException || httpex.InnerException is ObjectDisposedException); // Aborted
        }

        public static Tuple<string, string> GetDefaultErrorInfo(Exception ex) {
            Tuple<string, string> result = new Tuple<string, string>(String.Empty, String.Empty);
            if (ex is APIException apiEx) {
                string uem = VKAPIHelper.GetUnderstandableErrorMessage(apiEx.Code);
                result = new Tuple<string, string>($"{Assets.i18n.Resources.error_api} ({apiEx.Code})", String.IsNullOrEmpty(uem) ? apiEx.Message : uem);
            } else if (ex is System.Net.Http.HttpRequestException httpex) {
                string terr = Assets.i18n.Resources.err_network;
                string nerr = String.Empty;
                if (httpex.StatusCode == null) {
                    if (httpex.InnerException != null) {
                        if (httpex.InnerException is IOException || httpex.InnerException is ObjectDisposedException) {
                            nerr = $"{Assets.i18n.Resources.err_network_aborted}\n({httpex.InnerException.GetType()} {httpex.InnerException.HResultHEX()})";
                        } else if (httpex.InnerException is SocketException sex) {
                            switch (sex.ErrorCode) {
                                case 11001: nerr = $"{Assets.i18n.Resources.err_network_no_connection}"; break; // No internet connection
                                default: nerr = $"{Assets.i18n.Resources.err_network_general}\n(socketException code: {httpex.StatusCode})"; break;
                            }
                        }
                    } else {
                        nerr = $"{Assets.i18n.Resources.err_network_general}\n(HResult: {httpex.HResultHEX()})";
                    }
                } else {
                    nerr = Assets.i18n.Resources.err_network_general;
                    switch (httpex.StatusCode) {
                        default: nerr = $"{Assets.i18n.Resources.err_network_general}\n({httpex.StatusCode})"; break;
                        case System.Net.HttpStatusCode.InternalServerError:
                        case System.Net.HttpStatusCode.BadGateway:
                        case System.Net.HttpStatusCode.ServiceUnavailable:
                        case System.Net.HttpStatusCode.GatewayTimeout:
                            nerr = Assets.i18n.Resources.err_network_shinaprovod; break;
                    }
                }
                result = new Tuple<string, string>(terr, nerr);
            } else {
                result = new Tuple<string, string>(Assets.i18n.Resources.error, $"{ex.Message.Trim()}\n(0x{ex.HResult.ToString("x8")})");
            }
            return result;
        }

        public static async Task<bool> ShowErrorDialogAsync(Window owner, Tuple<string, string> errorInfo, bool hideRetry = false, string additional = null) {
            string[] buttons = hideRetry
                ? new string[] { Assets.i18n.Resources.close }
                : [Assets.i18n.Resources.retry, Assets.i18n.Resources.close];

            string message = String.IsNullOrEmpty(additional) ? errorInfo.Item2 : $"{additional}\n\n{errorInfo.Item2}";
            VKUIDialog alert = new VKUIDialog(errorInfo.Item1, message, buttons, 1);
            int result = await alert.ShowDialog<int>(owner);
            return !hideRetry && result == 1;
        }

        public static async Task<bool> ShowErrorDialogAsync(Window owner, Exception ex, bool hideRetry = false, string additional = null) {
            Log.Error(ex, $"From ShowErrorDialogAsync!");
            if (ex is AggregateException agex) ex = agex.InnerException;
            if (ex is APIException apiex && apiex.Code == 14) return true;
            Tuple<string, string> err = GetDefaultErrorInfo(ex);
            return await ShowErrorDialogAsync(owner, err, hideRetry, additional);
        }

        public static void ShowNotImplementedDialog(Window owner) {
            new System.Action(async () => {
                VKUIDialog alert = new VKUIDialog(Assets.i18n.Resources.not_implemented, Assets.i18n.Resources.not_implemented_desc);
                await alert.ShowDialog(owner);
            })();
        }
    }
}
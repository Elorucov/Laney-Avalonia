using Serilog;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ELOR.Laney.Core.Network {
    public class LServer {
        public static async Task<string> StartAndReturnQueryFromClient(CancellationToken ct) {
            string placeholder = $"<html><head><meta charset=\"UTF-8\"><title>Laney</title><script>setTimeout(()=>window.location.href=`${{window.location.origin}}/?${{window.location.hash.substr(1)}}`,300)</script></head><body>{Assets.i18n.Resources.wait}</body></html>";
            string placeholderFinal = $"<html><head><meta charset=\"UTF-8\"><title>Laney</title></head><body>{Assets.i18n.Resources.ls_done}</body></html>";

            string fragment = String.Empty;
            ct.ThrowIfCancellationRequested();
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:52639/");
            try {
                listener.Start();
                do {
                    Debug.WriteLine("Listening...");
                    HttpListenerContext ctx = await listener.GetContextAsync().WaitAsync(ct);
                    var request = ctx.Request;
                    var response = ctx.Response;
                    fragment = request.Url.Query;

                    Debug.WriteLine($"Fragment: {fragment}");
                    byte[] buffer = Encoding.UTF8.GetBytes(String.IsNullOrEmpty(fragment) ? placeholder : placeholderFinal);

                    response.ContentLength64 = buffer.Length;
                    var output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                } while (String.IsNullOrEmpty(fragment));
                Debug.WriteLine("Complete!");
                listener.Stop();
                return fragment;
            } catch (Exception ex) {
                if (ex is OperationCanceledException) {
                    Log.Information("LServer stopped by user request.");
                } else {
                    Log.Error(ex, "Exception thrown in LServer!");
                }
                if (listener.IsListening) listener.Stop();
                return fragment;
            }
        }
    }
}
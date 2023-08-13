using System;
using System.Net;
using System.Text;

namespace ELOR.Laney.Core.Network {
    public class LServer {
        public static string StartAndReturnQueryFromClient() {
            string placeholder = "<html><head><meta charset=\"UTF-8\"><title>Laney</title><script>setTimeout(()=>window.location.href=`${window.location.origin}/?${window.location.hash.substr(1)}`,300)</script></head><body>Подождите...</body></html>";
            string placeholderFinal = "<html><head><meta charset=\"UTF-8\"><title>Laney</title></head><body>Вернитесь обратно в Laney.</body></html>";

            string fragment = String.Empty;
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:52639/");
            listener.Start();
            do {
                Console.WriteLine("Listening...");
                HttpListenerContext ctx = listener.GetContext();
                var request = ctx.Request;
                var response = ctx.Response;
                fragment = request.Url.Query;

                Console.WriteLine($"Fragment: {fragment}");
                byte[] buffer = Encoding.UTF8.GetBytes(String.IsNullOrEmpty(fragment) ? placeholder : placeholderFinal);

                response.ContentLength64 = buffer.Length;
                var output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            } while (String.IsNullOrEmpty(fragment));
            Console.WriteLine("Complete!");
            listener.Stop();
            return fragment;
        }
    }
}
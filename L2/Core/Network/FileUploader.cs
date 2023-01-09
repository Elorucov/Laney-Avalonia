using Avalonia.Platform.Storage.FileIO;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace ELOR.Laney.Core.Network {
    public interface IFileUploader {
        event EventHandler<double> ProgressChanged;
        event EventHandler<Exception> UploadFailed;

        Task<string> UploadAsync();

        void Cancel();
    }

    public class VKHttpClientFileUploader : IFileUploader {
        CancellationTokenSource cts;
        string _type;
        Uri _uploadUri;
        BclStorageFile _file;

        public event EventHandler<double> ProgressChanged;
        public event EventHandler<Exception> UploadFailed;

        public VKHttpClientFileUploader(string type, Uri uploadUri, BclStorageFile file) {
            _type = type;
            _uploadUri = uploadUri;
            _file = file;
            cts = new CancellationTokenSource();
        }

        public async Task<string> UploadAsync() {
            try {
                if (_type == null && _uploadUri == null && _file == null)
                    throw new ArgumentException("One of the important parameters is null");
                Stream data = await _file.OpenReadAsync();

                using (var httpClient = new HttpClient()) {
                    httpClient.Timeout = Timeout.InfiniteTimeSpan;
                    string disposition = new string(Encoding.UTF8.GetBytes($"form-data; name=\"{_type}\"; filename=\"{_file.Name}\"")
                        .Select(b => (char)b).ToArray());

                    using (ProgressableStreamContent filecontent = new ProgressableStreamContent(data, StatusChanged)) {
                        filecontent.Headers.Add("Content-Type", "application/octet-stream");
                        filecontent.Headers.Add("Content-Disposition", disposition);

                        string boundary = $"----------------------------{Guid.NewGuid()}";
                        MultipartFormDataContent mfdc = new MultipartFormDataContent(boundary);
                        mfdc.Add(filecontent);

                        using (HttpRequestMessage hrm = new HttpRequestMessage()) {
                            hrm.Method = HttpMethod.Post;
                            hrm.RequestUri = _uploadUri;
                            hrm.Content = mfdc;

                            Log.Information($"VKHttpClientFileUploader: Starting upload file \"{_file.Name}\" to \"{_uploadUri.ToString()}\"");
                            HttpResponseMessage response = await httpClient.SendAsync(hrm, HttpCompletionOption.ResponseContentRead);

                            Log.Information($"VKHttpClientFileUploader: response encoding: {response.Content.Headers.ContentType.CharSet}");
                            if (response.Content.Headers.ContentType.CharSet == "windows-1251") {
                                string responseString = null;
                                using (var sr = new StreamReader(await response.Content.ReadAsStreamAsync(), Encoding.UTF8)) {
                                    responseString = sr.ReadToEnd();
                                }
                                return responseString;
                            } else {
                                return await response.Content.ReadAsStringAsync();
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                UploadFailed?.Invoke(this, ex);
                return null;
            }
        }

        private void StatusChanged(Tuple<ProgressableStreamStatus, long, long> status) {
            double a = status.Item2;
            double b = status.Item3;
            double p = 100 / a * b;
            Debug.WriteLine($"VKHttpClientFileUploader: Progress: {Math.Round(p, 1)}; {b}b of {a}b...");
            ProgressChanged?.Invoke(this, p);
        }

        public void Cancel() {
            Log.Information($"VKHttpClientFileUploader: Uploading canceled! \"{_file.Name}\"");
            cts.Cancel();
            cts.Dispose();
        }
    }
}
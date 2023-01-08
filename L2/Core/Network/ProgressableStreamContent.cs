using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace ELOR.Laney.Core.Network {
    public enum ProgressableStreamStatus {
        PendingUpload, Uploading, PendingResponse
    }

    public class ProgressableStreamContent : HttpContent {
        private const int defaultBufferSize = 4096;

        private Stream content;
        private int bufferSize;
        private bool contentConsumed;
        private Action<Tuple<ProgressableStreamStatus, long, long>> statusChanged;

        public ProgressableStreamContent(Stream content, Action<Tuple<ProgressableStreamStatus, long, long>> statusChanged) : this(content, defaultBufferSize, statusChanged) { }

        public ProgressableStreamContent(Stream content, int bufferSize, Action<Tuple<ProgressableStreamStatus, long, long>> statusChanged) {
            if (content == null) {
                throw new ArgumentNullException("content");
            }
            if (bufferSize <= 0) {
                throw new ArgumentOutOfRangeException("bufferSize");
            }

            this.content = content;
            this.bufferSize = bufferSize;
            this.statusChanged = statusChanged;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context) {
            Contract.Assert(stream != null);

            PrepareContent();

            return Task.Run(() => {
                var buffer = new Byte[this.bufferSize];
                var size = content.Length;
                var uploaded = 0;

                statusChanged.Invoke(new Tuple<ProgressableStreamStatus, long, long>(ProgressableStreamStatus.PendingUpload, size, uploaded));

                using (content) while (true) {
                        var length = content.Read(buffer, 0, buffer.Length);
                        if (length <= 0) break;

                        uploaded += length;
                        statusChanged.Invoke(new Tuple<ProgressableStreamStatus, long, long>(ProgressableStreamStatus.PendingUpload, size, uploaded));
                        stream.Write(buffer, 0, length);
                    }

                statusChanged.Invoke(new Tuple<ProgressableStreamStatus, long, long>(ProgressableStreamStatus.PendingResponse, size, uploaded));
            });
        }

        protected override bool TryComputeLength(out long length) {
            length = content.Length;
            return true;
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                content.Dispose();
            }
            base.Dispose(disposing);
        }


        private void PrepareContent() {
            if (contentConsumed) {
                // If the content needs to be written to a target stream a 2nd time, then the stream must support
                // seeking (e.g. a FileStream), otherwise the stream can't be copied a second time to a target 
                // stream (e.g. a NetworkStream).
                if (content.CanSeek) {
                    content.Position = 0;
                } else {
                    throw new InvalidOperationException("SR.net_http_content_stream_already_read");
                }
            }

            contentConsumed = true;
        }
    }
}
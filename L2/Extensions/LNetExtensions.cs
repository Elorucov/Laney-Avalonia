using ELOR.Laney.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ELOR.Laney.Extensions {
    public static class LNetExtensions {
        static Dictionary<string, byte[]> cachedImages = new Dictionary<string, byte[]>();
        const int cachesLimit = 500;

        public static async Task<byte[]> TryGetCachedImageAsync(Uri uri) {
            string url = uri.AbsoluteUri;
            if (!cachedImages.ContainsKey(url)) {
                var response = await LNet.GetAsync(uri);
                var bytes = await response.Content.ReadAsByteArrayAsync();
                if (cachedImages.Count == cachesLimit) cachedImages.Remove(cachedImages.First().Key);
                if (!cachedImages.ContainsKey(url)) cachedImages.Add(url, bytes); // надо
                return bytes;
            } else {
                return cachedImages[url];
            }
        }

        public static async Task<HttpResponseMessage> SendRequestToAPIViaLNetAsync(Uri uri, Dictionary<string, string> parameters, Dictionary<string, string> headers) {
            return await LNet.PostAsync(uri, parameters, headers);
        }
    }
}
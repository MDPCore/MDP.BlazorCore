using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MDP.Network.Http
{
    public static class HttpRequestMessageExtensions
    {
        // Methods
        public static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage request)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(request);

            #endregion

            // CloneRequest
            var cloneRequest = new HttpRequestMessage(request.Method, request.RequestUri);

            // CloneRequest.Headers
            foreach (var header in request.Headers)
            {
                cloneRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // CloneRequest.Content
            if (request.Content != null)
            {
                // Content
                var memoryStream = new MemoryStream();
                await request.Content.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                cloneRequest.Content = new StreamContent(memoryStream);

                // Content.Headers
                foreach (var header in request.Content.Headers)
                {
                    cloneRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Return
            return cloneRequest;
        }
    }
}

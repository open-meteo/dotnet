using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using openmeteo_sdk;
using Google.FlatBuffers;

namespace OpenMeteo
{
    public class OpenMeteo
    {
        private HttpClient Client;

        public OpenMeteo(HttpClient client)
        {
            this.Client = client;
        }

        public OpenMeteo()
        {
            this.Client = new HttpClient(new RetryHandler(new HttpClientHandler()));
        }

        /// <summary>
        /// Convert an array of bytes to an array of FlatBuffer responses
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static WeatherApiResponse[] decodeWeatherResponse(byte[] bytes)
        {
            var buffer = new ByteBuffer(bytes);

            // Get number of total weather response messages
            var total = 0;
            while (buffer.Position < buffer.Length)
            {
                var length = buffer.GetInt(buffer.Position);
                buffer.Position += length;
                total += 1;
            }

            // Prepare output array
            var results = new WeatherApiResponse[total];
            var i = 0;
            buffer.Position = 0;

            // Fill messages in array
            while (buffer.Position < buffer.Length)
            {
                var length = buffer.GetInt(buffer.Position);
                buffer.Position += 4;
                results[i] = WeatherApiResponse.GetRootAsWeatherApiResponse(buffer);
                buffer.Position += length;
                i += 1;
            }
            return results;
        }
    }

    /// <summary>
    /// Retry failed HTTP requests. See: https://stackoverflow.com/a/19650002
    /// </summary>
    public class RetryHandler : DelegatingHandler
    {
        private const int MaxRetries = 3;

        public RetryHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        { }


        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            for (int i = 0; i < MaxRetries; i++)
            {
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
            }

            return response;
        }
    }
}


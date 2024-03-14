using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using openmeteo_sdk;
using Google.FlatBuffers;

namespace OpenMeteo
{
    public class OpenMeteoClient
    {
        private HttpClient Client;

        public OpenMeteoClient(HttpClient client)
        {
            this.Client = client;
        }

        public OpenMeteoClient()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip;
            this.Client = new HttpClient(new RetryHandler(handler));
            this.Client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
        }

        /// <summary>
        /// Fetch weather data from an Open-Meteo API endpoint and decode messages using FlatBuffers
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<WeatherApiResponse[]> GetWeather(Uri uri)
        {
            var response = await Client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            var bytes = await response.Content.ReadAsByteArrayAsync();
            return DecodeWeatherResponses(bytes);
        }

        /// <summary>
        /// Convert an array of bytes to an array of FlatBuffer responses
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static WeatherApiResponse[] DecodeWeatherResponses(byte[] bytes)
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
        private const double BackoffFactor = 0.5;
        private const int BackoffMaxSeconds = 2;


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
                int waitMs = (int)Math.Min(BackoffFactor * Math.Pow(2, i), BackoffMaxSeconds) * 1000;
                await Task.Delay(waitMs);
            }
            return response;
        }
    }

    /*public static class VariablesWithTimeExtension {
        /// <summary>
        /// Iterate over timestamps
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<DateTimeOffset> DateTimeEnumerator(this VariablesWithTime value)
        {
            for (var time = value.Time; time < value.TimeEnd; time += value.Interval)
                yield return DateTimeOffset.FromUnixTimeSeconds(time);
        }
    }*/
}


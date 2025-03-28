using Metica.Experimental.Caching;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Metica.Experimental.Network
{
    /// <summary>
    /// Implements <see cref="IHttpService"/> using .Net's <see cref="HttpClient"/>,
    /// leveraging asynchronousness.
    /// </summary>
    /// <remarks>
    /// ## Ways to set the headers
    /// Headers can be set in a number of ways.
    /// 1. Use <see cref="WithPersistentHeaders(IDictionary{string, string})"/> but this should only be used for common headers like "X-API-Key"
    /// that must remain the same for all requests.
    /// 2. Passing them as parameters to the method, for example <see cref="PostAsync(string, string, Dictionary{string, string}, string)"/>. In
    /// this case they will be parsed and distributed between *request headers* and *content headers*, complying to well-known standards.
    /// For example, if an "X-API-Key":"xxxxxxxxxxxxxxxxxxx" and a "Content-Type":"application/json" are passed in the dictionary, "X-API-Key"
    /// will go into the request headers whilst the "Content-Type" will go into the content headers.
    /// ## Further reading:
    /// - https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient
    /// - https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient
    /// - https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-net-http-httpclient
    /// </remarks>
    public class HttpServiceDotnet : IHttpService
    {
        // TODO: improve constructor
        private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };
        private readonly CancellationTokenSource cts = new();

        private ICache<string, HttpResponse> _cache { get; set; } = null;

        IHttpService IHttpService.WithCache(ICache<string, HttpResponse> cache)
        {
            _cache = cache;
            return this;
        }

        /// <summary>
        /// Sets the client's persistent headers. All the existing ones will be cleared.
        /// </summary>
        /// <param name="headers">Persistent headers. For example "X-API-Key" should go here.</param>
        /// <returns>This object for *fluent* use.</returns>
        public IHttpService WithPersistentHeaders(IDictionary<string, string> headers)
        {
            _http.DefaultRequestHeaders.Clear();
            foreach (var header in headers)
            {
                _http.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            return this;
        }

        public void Dispose()
        {
            cts.Cancel();
            _http.Dispose();
        }

        /// <inheritdoc/>
        /// <param name="requestHeaders">This field can be null.</param>
        public async Task<HttpResponse> GetAsync(string url, Dictionary<string,string> requestHeaders = null)
        {
            if(_cache != null)
            {
                HttpResponse? cachedResponse = _cache.Get(url);
                if (cachedResponse.HasValue)
                {
                    return cachedResponse.Value;
                }
            }
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (requestHeaders != null)
            {
                foreach (var header in requestHeaders)
                {
                    // Try to add the header to the request headers. .NET here checks if the given header is meant for the request header.
                    // "WithoutValidation" here means throwing no exceptions but it will still fail silently and return false, for example,
                    // if we try to add a "Content-Type" to the request.Header as it should normally go in the content's headers.
                    if(!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
                    {
                        // Since GET requests don't have a Content, we return error if one given header
                        // is not compliant with "request headers".
                        return new HttpResponse(HttpResponse.ResultStatus.Failure, null, $"Invalid header: {header.Key}");
                    }
                }
            }

            try
            {
                if (cts.IsCancellationRequested)
                {
                    return new HttpResponse(HttpResponse.ResultStatus.Cancelled, string.Empty, $"{nameof(HttpServiceDotnet)}/{nameof(GetAsync)}: Task was cancelled.");
                }

                using var response = await _http.SendAsync(request, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new HttpResponse(HttpResponse.ResultStatus.Failure, null, $"Request failed with status {response.StatusCode}: {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                return new HttpResponse(HttpResponse.ResultStatus.Success, content);
            }
            catch (TaskCanceledException ex)
            {
                return new HttpResponse(HttpResponse.ResultStatus.Cancelled, null, ex.Message);
            }
            //catch (Exception ex)
            //{
                //return new HttpResponse(HttpResponse.ResultStatus.Failure, null, ex.Message);
            //}
        }

        /// <inheritdoc/>
        /// <param name="requestHeaders">This field can be null.</param>
        /// <param name="entityHeaders">This field can be null.</param>
        public async Task<HttpResponse> PostAsync(string url, string body, string bodyContentType = "application/json", Dictionary<string, string> requestHeaders = null, Dictionary<string,string> entityHeaders = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, bodyContentType)
            };

            if(requestHeaders != null)
            {
                foreach (var header in requestHeaders)
                {
                    // TryAddWithoutValidation https://learn.microsoft.com/en-us/dotnet/api/system.net.http.headers.httpheaders.tryaddwithoutvalidation?view=netframework-4.7.1#system-net-http-headers-httpheaders-tryaddwithoutvalidation(system-string-system-string)
                    if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
                    {
                        return new HttpResponse(HttpResponse.ResultStatus.Failure, null, $"Invalid header: {header.Key}");
                    }
                }
            }
            if (entityHeaders != null)
            {
                foreach (var header in entityHeaders)
                {
                    if (!request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value))
                    {
                        return new HttpResponse(HttpResponse.ResultStatus.Failure, null, $"Invalid header: {header.Key}");
                    }
                }
            }

            try
            {
                if (cts.IsCancellationRequested)
                {
                    return new HttpResponse(HttpResponse.ResultStatus.Cancelled, string.Empty, $"{nameof(HttpServiceDotnet)}/{nameof(PostAsync)}: Task was cancelled.");
                }
                using var response = await _http.SendAsync(request, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new HttpResponse(HttpResponse.ResultStatus.Failure, responseContent: null, errorMessage: $"Request failed with status {response.StatusCode}: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return new HttpResponse(HttpResponse.ResultStatus.Success, responseContent);
            }
            catch (TaskCanceledException ex)
            {
                return new HttpResponse(HttpResponse.ResultStatus.Cancelled, responseContent: null, errorMessage: ex.Message);
            }
            //catch (Exception ex)
            //{
                //return new HttpResponse(HttpResponse.ResultStatus.Failure, null, ex.Message);
            //}
        }

    }
}

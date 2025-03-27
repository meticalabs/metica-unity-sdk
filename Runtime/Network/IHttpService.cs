using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metica.Network
{
    /// <summary>
    /// A universal http response wrapper that all implementations can fill and return.
    /// </summary>
    public class HttpResponse
    {
        public enum ResultStatus { Success, Failure, Cancelled }

        public ResultStatus Status { get; set; }
        public string ResponseContent { get; set; }
        public string ErrorMessage { get; set; }

        public HttpResponse(ResultStatus status, string responseContent = null, string errorMessage = null)
        {
            Status = status;
            ResponseContent = responseContent;
            ErrorMessage = errorMessage;
        }

        public override string ToString()
        {
            return $"Status:{Status.ToString()}\nResponse Content:{ResponseContent}\nError Message:{ErrorMessage}";
        }
    }

    /// <summary>
    /// This interface requires inheritors to implement an http client.
    /// Microsoft .Net's HttpClient implementation is used as conventional guide.
    /// See (HttpClient class)[https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient]
    /// </summary>
    public interface IHttpService : IDisposable
    {
        /// <summary>
        /// Injects a simple cache implementation for internal use.
        /// The key is a combination of strings that are automatically combined to get a unique hash.
        /// </summary>
        /// <param name="cache">An implementation of a cache with string keys and <see cref="HttpResponse"/> values.</param>
        /// <returns>The same object in fluent fashion.</returns>
        //IHttpService WithCache(ICache<List<string>, HttpResponse> cache);

        /// <summary>
        /// Gets a resource from <see cref="url"/>.
        /// </summary>
        /// <param name="url">Endpoint.</param>
        /// <param name="headers">Request or content headers.</param>
        /// <returns>A <see cref="HttpResponse"/> object.</returns>
        Task<HttpResponse> GetAsync(string url, Dictionary<string, string> requestHeaders, bool useCache = true);

        /// <summary>
        /// Sends a POST request.
        /// </summary>
        /// <param name="url">Endpoint.</param>
        /// <param name="body">The body of the request.</param>
        /// <param name="bodyContentType">The content type of this request. e.g: "application/json".</param>
        /// <param name="requestHeaders">Request headers.</param>
        /// <param name="contentHeaders">Content headers.</param>
        /// <returns>A <see cref="HttpResponse"/> object.</returns>
        Task<HttpResponse> PostAsync(string url, string body, string bodyContentType = "application/json", Dictionary<string, string> requestHeaders = null, Dictionary<string, string> contentHeaders = null, bool useCache = true);
    }
}

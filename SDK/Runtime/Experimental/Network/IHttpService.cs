using Metica.Experimental.Caching;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metica.Experimental.Network
{
    /// <summary>
    /// A universal http response wrapper that all implementations can fill and return.
    /// </summary>
    public struct HttpResponse
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
        /// </summary>
        /// <param name="cache">An implementation of a cache with string keys and <see cref="HttpResponse"/> values.</param>
        /// <returns>The same object in fluent fashion.</returns>
        protected IHttpService WithCache(ICache<string, HttpResponse> cache);

        /// <summary>
        /// Gets a resource from <see cref="url"/>.
        /// </summary>
        /// <param name="url">Endpoint.</param>
        /// <param name="headers">Request or content headers.</param>
        /// <returns>A <see cref="HttpResponse"/> object.</returns>
        Task<HttpResponse> GetAsync(string url, Dictionary<string, string> requestHeaders);

        /// <summary>
        /// Sends a POST request.
        /// </summary>
        /// <param name="url">Endpoint.</param>
        /// <param name="body">The body of the request.</param>
        /// <param name="bodyContentType">The content type of this request. e.g: "application/json".</param>
        /// <param name="requestHeaders">Request headers.</param>
        /// <param name="contentHeaders">Content headers.</param>
        /// <returns>A <see cref="HttpResponse"/> object.</returns>
        Task<HttpResponse> PostAsync(string url, string body, string bodyContentType = "application/json", Dictionary<string, string> requestHeaders = null, Dictionary<string, string> contentHeaders = null);

        /*
         CancelPendingRequests()	
Cancel all pending requests on this instance.

DeleteAsync(String, CancellationToken)	
Send a DELETE request to the specified URI with a cancellation token as an asynchronous operation.

DeleteAsync(String)	
Send a DELETE request to the specified URI as an asynchronous operation.

DeleteAsync(Uri, CancellationToken)	
Send a DELETE request to the specified URI with a cancellation token as an asynchronous operation.

DeleteAsync(Uri)	
Send a DELETE request to the specified URI as an asynchronous operation.

Dispose()	
Releases the unmanaged resources and disposes of the managed resources used by the HttpMessageInvoker.

(Inherited from HttpMessageInvoker)
Dispose(Boolean)	
Releases the unmanaged resources used by the HttpClient and optionally disposes of the managed resources.

Equals(Object)	
Determines whether the specified object is equal to the current object.

(Inherited from Object)
GetAsync(String, CancellationToken)	
Send a GET request to the specified URI with a cancellation token as an asynchronous operation.

GetAsync(String, HttpCompletionOption, CancellationToken)	
Send a GET request to the specified URI with an HTTP completion option and a cancellation token as an asynchronous operation.

GetAsync(String, HttpCompletionOption)	
Send a GET request to the specified URI with an HTTP completion option as an asynchronous operation.

GetAsync(String)	
Send a GET request to the specified URI as an asynchronous operation.

GetAsync(Uri, CancellationToken)	
Send a GET request to the specified URI with a cancellation token as an asynchronous operation.

GetAsync(Uri, HttpCompletionOption, CancellationToken)	
Send a GET request to the specified URI with an HTTP completion option and a cancellation token as an asynchronous operation.

GetAsync(Uri, HttpCompletionOption)	
Send a GET request to the specified URI with an HTTP completion option as an asynchronous operation.

GetAsync(Uri)	
Send a GET request to the specified URI as an asynchronous operation.

GetByteArrayAsync(String)	
Sends a GET request to the specified URI and return the response body as a byte array in an asynchronous operation.

GetByteArrayAsync(Uri)	
Send a GET request to the specified URI and return the response body as a byte array in an asynchronous operation.

GetHashCode()	
Serves as the default hash function.

(Inherited from Object)
GetStreamAsync(String)	
Send a GET request to the specified URI and return the response body as a stream in an asynchronous operation.

GetStreamAsync(Uri)	
Send a GET request to the specified URI and return the response body as a stream in an asynchronous operation.

GetStringAsync(String)	
Send a GET request to the specified URI and return the response body as a string in an asynchronous operation.

GetStringAsync(Uri)	
Send a GET request to the specified URI and return the response body as a string in an asynchronous operation.

GetType()	
Gets the Type of the current instance.

(Inherited from Object)
MemberwiseClone()	
Creates a shallow copy of the current Object.

(Inherited from Object)
PostAsync(String, HttpContent, CancellationToken)	
Send a POST request with a cancellation token as an asynchronous operation.

PostAsync(String, HttpContent)	
Send a POST request to the specified URI as an asynchronous operation.

PostAsync(Uri, HttpContent, CancellationToken)	
Send a POST request with a cancellation token as an asynchronous operation.

PostAsync(Uri, HttpContent)	
Send a POST request to the specified URI as an asynchronous operation.

PutAsync(String, HttpContent, CancellationToken)	
Send a PUT request with a cancellation token as an asynchronous operation.

PutAsync(String, HttpContent)	
Send a PUT request to the specified URI as an asynchronous operation.

PutAsync(Uri, HttpContent, CancellationToken)	
Send a PUT request with a cancellation token as an asynchronous operation.

PutAsync(Uri, HttpContent)	
Send a PUT request to the specified URI as an asynchronous operation.

Send(HttpRequestMessage, CancellationToken)	
Sends an HTTP request with the specified request and cancellation token.

(Inherited from HttpMessageInvoker)
SendAsync(HttpRequestMessage, CancellationToken)	
Send an HTTP request as an asynchronous operation.

SendAsync(HttpRequestMessage, HttpCompletionOption, CancellationToken)	
Send an HTTP request as an asynchronous operation.

SendAsync(HttpRequestMessage, HttpCompletionOption)	
Send an HTTP request as an asynchronous operation.

SendAsync(HttpRequestMessage)	
Send an HTTP request as an asynchronous operation.

ToString()	
Returns a string that represents the current object.

(Inherited from Object)
         */
    }
}

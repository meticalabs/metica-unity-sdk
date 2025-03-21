using Metica.Experimental.Network;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Metica.Experimental
{
    public interface IMeticaHttpResult
    {
        // TODO : fields should preferrably be readonly or with private setter

        public HttpResponse.ResultStatus Status { get; set; }
        public string Error { get; set; }
        public string RawContent {  get; set; }
    }

    public abstract class EndpointManager: IAsyncDisposable
    {
        protected readonly IHttpService _httpService;
        protected readonly string _url;

        public EndpointManager(IHttpService httpService, string endpoint)
        {
            {
                _httpService = httpService;
                _url = endpoint;
            }
        }

        protected TResult ResponseToResult<TResult>(HttpResponse response) where TResult : IMeticaHttpResult, new()
        {
            if (response.Status == HttpResponse.ResultStatus.Success)
            {
                string content = response.ResponseContent;
                TResult result;
                if (string.IsNullOrEmpty(content) == false)
                    result = JsonConvert.DeserializeObject<TResult>(content);
                else
                    result = new TResult();
                result.Status = response.Status;
                result.RawContent = response.ResponseContent;
                return result;
            }
            else
            {
                return new TResult() {
                    Status = response.Status,
                    Error = response.ErrorMessage,
                    RawContent = response.ResponseContent,
                };
            }
        }

        public abstract ValueTask DisposeAsync();
    }
}

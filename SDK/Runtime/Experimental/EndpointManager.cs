using Metica.Experimental.Network;
using Newtonsoft.Json;

namespace Metica.Experimental
{
    public interface IMeticaSdkResult
    {
        // TODO : fields should be readonly or with private setter

        public HttpResponse.ResultStatus Status { get; set; }
        public string Error { get; set; }
        public string RawContent {  get; set; }
    }

    public abstract class EndpointManager
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

        protected TResult ResponseToResult<TResult>(HttpResponse response) where TResult : IMeticaSdkResult, new()
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

    }
}

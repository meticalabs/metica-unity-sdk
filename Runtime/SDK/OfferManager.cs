using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Metica.Core;
using Metica.Network;
using Metica.Model;
using Newtonsoft.Json;

namespace Metica
{
    [Serializable]
    public class OfferResult : IMeticaHttpResult
    {
        [JsonProperty("placements")]
        public Dictionary<string, List<Offer>> placements { get; set; }

        [JsonIgnore] public HttpResponse.ResultStatus Status { get; set; }
        [JsonIgnore] public string Error { get; set; }
        [JsonIgnore] public string RawContent { get; set; }

        public override string ToString()
        {
            string placementsString = string.Empty;
            if (placements != null)
            {
                foreach (var p in placements)
                {
                    placementsString = $"{placementsString}{p.Key}\n";
                }
            }

            return
                $"{nameof(OfferResult)}:\n{placementsString}\n Status: {Status}\n RawContent: {RawContent}\n Error: {Error}";
        }
    }

    public interface IMeticaAttributesProvider
    {
        Task<object> GetMeticaAttributes(string offerId, string placementId);
    }

    /// <summary>
    /// 1. Manages calls to get offers.
    /// 2. Manages a storage to provide information for events.
    /// </summary>
    /// <remarks>
    /// <b>Roadmap</b>
    /// <ul>TODO - With <see cref="AddOrUpdateStorage(Dictionary{string, List{Offer}})"/> the <see cref="_sessionPlacementStorage"/> can grow indefinitely. Implement defence against this.</ul>
    /// </remarks>
    public sealed class OfferManager : EndpointManager, IMeticaAttributesProvider
    {
        private readonly ITimeSource timeSource = new SystemDateTimeSource();
        private readonly IDeviceInfoProvider _deviceInfoProvider;

        /// <summary>
        /// All placements stored and available for the entire session.
        /// These are retrieved lazily and serve the purpose of providing the data that other parts of the SDK need (e.g. <see cref="EventManager"/>).
        /// These generally remain the same for the whole session; only when specific placements are requested this storage is updated.
        /// </summary>
        private Dictionary<string, List<Offer>> _sessionPlacementStorage = null;

        public OfferManager(IHttpService httpService, string offersEndpoint) : base(httpService, offersEndpoint)
        {
            _deviceInfoProvider = Registry.Resolve<IDeviceInfoProvider>();
        }

        public async Task<object> GetMeticaAttributes(string placementId, string offerId)
        {
            if (_sessionPlacementStorage == null)
            {
                // Lazy fetching of all placements into storage
                var result = await GetAllOffersAsync(MeticaSdk.UserId);
                _sessionPlacementStorage = result.placements;
            }

            if (_sessionPlacementStorage.ContainsKey(placementId) == false)
            {
                return null;
            }

            var offers = _sessionPlacementStorage[placementId].Where(o => o.offerId == offerId);
            if (offers == null || offers.Count() == 0)
            {
                return null;
            }

            var meticaAttributesObject = offers.First().GetMeticaAttributesObject(); // at this point we always have 1+
            return new
            {
                placementId,
                offer = meticaAttributesObject,
            };
        }

        /// <summary>
        /// Adds or updates the given placements to the storage. This <i>always</i> retrieves the latest placement by design.
        /// We tolerate a side effect (mutating <see cref="_sessionPlacementStorage"/>) for optimization.
        /// </summary>
        /// <param name="placements">New placements to add or update.</param>
        /// <remarks>This method was created to achieve availability of information that is needed for sending the events.
        /// It is and should remain private as the management of this auxiliary storage should be hidden to client-code.</remarks>
        private async Task AddOrUpdateStorage(Dictionary<string, List<Offer>> placements)
        {
            if (placements == null)
            {
                Log.Error(() =>
                    "Null placements were passed. This could be due to an inconsistent initialization of the SDK (e.g. a wrong appId).");
                return;
            }

            if (_sessionPlacementStorage == null)
            {
                // Lazy fetching of all placements into storage
                var result = await GetAllOffersAsync(MeticaSdk.UserId);
                _sessionPlacementStorage = result.placements;
            }

            foreach (var k in placements.Keys)
            {
                if (_sessionPlacementStorage.ContainsKey(k))
                {
                    _sessionPlacementStorage[k] = placements[k];
                }
                else
                {
                    _sessionPlacementStorage.Add(k, placements[k]);
                }
            }
        }

        [Obsolete]
        public async Task<OfferResult> GetOffersAsync(string userId, string[] placements, Dictionary<string, object> userData, DeviceInfo deviceInfo)
            => await GetOffersAsync(userId, placements, userData);
        /// <summary>
        /// Gets specified placements asynchronously.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="placements"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public async Task<OfferResult> GetOffersAsync(string userId, string[] placements, Dictionary<string, object> userData = null)
        {
            try
            {
                if (placements == null || placements.Length == 0)
                {
                    return await GetAllOffersAsync(userId, userData);
                }

                var requestBody = new Dictionary<string, object>
                {
                    { FieldNames.UserId, userId },
                    { FieldNames.DeviceInfo, _deviceInfoProvider.GetDeviceInfo() },
                    { FieldNames.UserData, userData }
                };

                // We only retrieve via http the pending placements
                var url = _url;
                if (placements.Length > 0)
                {
                    url = $"{url}?placements=";
                    for (int i = 0; i < placements.Length; i++)
                    {
                        var pl = placements[i];
                        url = $"{url}{pl}{((i < placements.Length - 1) ? "," : "")}";
                    }
                }

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;

                var httpResponse = await _httpService.PostAsync(url, JsonConvert.SerializeObject(requestBody, settings), "application/json");
                OfferResult offerResult = ResponseToResult<OfferResult>(httpResponse);
                await AddOrUpdateStorage(offerResult.placements); // Add or update storage with the new received placements.
                return offerResult;
            }
            catch (System.Net.Http.HttpRequestException exception)
                when (exception.InnerException is TimeoutException || exception.Message.Contains("timed out"))
            {
                Log.Error(() => $"OfferManager.GetOffersAsync: Request timed out: {exception.Message}");
                return new OfferResult
                {
                    Status = HttpResponse.ResultStatus.Failure,
                    Error = $"Timeout: {exception.Message}",
                    RawContent = null,
                    placements = null
                };
            }
            catch (System.Exception exception)
            {
                Log.Error(() => $"OfferManager.GetOffersAsync: Exception: {exception.Message}");
                return new OfferResult
                {
                    Status = HttpResponse.ResultStatus.Failure,
                    Error = exception.Message,
                    RawContent = null,
                    placements = null
                };
            }
        }

        private async Task<OfferResult> GetAllOffersAsync(string userId, Dictionary<string, object> userData = null)
        {
            var requestBody = new Dictionary<string, object>
            {
                { FieldNames.UserId, userId },
                { FieldNames.DeviceInfo, _deviceInfoProvider.GetDeviceInfo() },
                { FieldNames.UserData, userData }
            };

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            var httpResponse = await _httpService.PostAsync(_url, JsonConvert.SerializeObject(requestBody, settings),
                "application/json");
            OfferResult offerResult = ResponseToResult<OfferResult>(httpResponse);

            return offerResult;
        }

        public override ValueTask DisposeAsync()
        {
            return default; // no-op
        }
    }
}

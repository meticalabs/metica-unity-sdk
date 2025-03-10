using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metica.Unity
{

    internal interface IBackendOperations
    {
        public void CallGetOffersAPI(string[] placements,
            MeticaSdkDelegate<OffersByPlacement> offersCallback, Dictionary<string, object> userProperties = null,
            DeviceInfo deviceInfo = null);

        public void CallSubmitEventsAPI(ICollection<Dictionary<string, object>> events,
            MeticaSdkDelegate<string> callback);
        
        public void CallRemoteConfigAPI(string[] configKeys, MeticaSdkDelegate<RemoteConfig> responseCallback, Dictionary<string, object> userProperties = null,
            DeviceInfo deviceInfo = null);
    }

    internal class BackendOperationsImpl : IBackendOperations
    {
        #region OFFER REFACTOR

        public void CallGetOffersAPI(string[] placements,
            MeticaSdkDelegate<OffersByPlacement> offersCallback, Dictionary<string, object> userProperties = null,
            DeviceInfo deviceInfo = null)
        {
            MeticaScriptingRoot coroutineRunner = ScriptingObjects.GetComponent<MeticaScriptingRoot>();
            coroutineRunner.AddCoroutine(GetOffersOperationStart(placements, userProperties, deviceInfo, offersCallback));
        }

        [Serializable]
        internal class ODSRequest : RequestWithUserDataAndDeviceInfo
        {
        }

        [Serializable]
        internal class ODSResponse
        {
            public Dictionary<string, List<Offer>> placements;
        }
        
        private static ODSRequest CreateODSRequestBody(Dictionary<string, object> userData,
            DeviceInfo overrideDeviceInfo = null)
        {
            var requestWithUserDataAndDeviceInfo = RequestUtils.CreateRequestWithUserDataAndDeviceInfo(userData, overrideDeviceInfo);
            var request = new ODSRequest
            {
                userId = MeticaAPI.UserId,
                userData = requestWithUserDataAndDeviceInfo.userData,
                deviceInfo = requestWithUserDataAndDeviceInfo.deviceInfo
            };

            return request;
        }

        private IEnumerator GetOffersOperationStart(string[] Placements, Dictionary<string, object> UserProperties, DeviceInfo DeviceInfo, MeticaSdkDelegate<OffersByPlacement> OffersCallback)
        {
            yield return PostRequestOperation.PostRequest<ODSResponse>(
                $"{MeticaAPI.Config.offersEndpoint}/offers/v1/apps/{MeticaAPI.AppId}",
                new Dictionary<string, object>
                {
                    { "placements", Placements },
                },
                MeticaAPI.ApiKey,
                CreateODSRequestBody(UserProperties, DeviceInfo),
                result =>
                {
                    if (result.Error != null)
                    {
                        OffersCallback(SdkResultImpl<OffersByPlacement>.WithError(result.Error));
                    }
                    else
                    {
                        OffersCallback(SdkResultImpl<OffersByPlacement>.WithResult(new OffersByPlacement
                        {
                            placements = result.Result.Data.placements
                        }));
                    }
                });
        }

        #endregion OFFER REFACTOR


        #region EVENT REFACTORING

        public void CallSubmitEventsAPI(ICollection<Dictionary<string, object>> events,
            MeticaSdkDelegate<string> callback)
        {
            MeticaScriptingRoot coroutineRunner = ScriptingObjects.GetComponent<MeticaScriptingRoot>();
            coroutineRunner.AddCoroutine(CallEventsIngestionOperationStart(events, callback));
        }

        internal IEnumerator CallEventsIngestionOperationStart(ICollection<Dictionary<string, object>> Events, MeticaSdkDelegate<String> EventsSubmitCallback)
        {
            return PostRequestOperation.PostRequest<String>($"{MeticaAPI.Config.ingestionEndpoint}/ingest/v1/events",
                null,
                MeticaAPI.ApiKey,
                CreateIngestionRequestBody(Events),
                result =>
                {
                    EventsSubmitCallback(result.Error != null
                        ? SdkResultImpl<string>.WithError(result.Error)
                        : SdkResultImpl<string>.WithResult(result.Result?.Data ?? string.Empty));
                });
        }

        [Serializable]
        internal class RequestWithUserDataAndDeviceInfo
        {
            public string userId;
            public Dictionary<string, object> userData;
            public DeviceInfo deviceInfo;
        }

        private static object CreateIngestionRequestBody(ICollection<Dictionary<string, object>> events)
        {
            return new Dictionary<string, object> { { "events", events } };
        }


        #endregion EVENT REFACTORING

        #region CONFIG REFACTOR

        public void CallRemoteConfigAPI(string[] configKeys, MeticaSdkDelegate<RemoteConfig> responseCallback, Dictionary<string, object> userProperties = null,
            DeviceInfo deviceInfo = null)
        {
            MeticaScriptingRoot coroutineRunner = ScriptingObjects.GetComponent<MeticaScriptingRoot>();
            coroutineRunner.AddCoroutine(RemoteConfigOperationStart(configKeys, userProperties, deviceInfo, responseCallback));
        }

        public IEnumerator RemoteConfigOperationStart(
            ICollection<string> ConfigKeys,
            Dictionary<string, object> UserProperties,
            DeviceInfo DeviceInfo,
            MeticaSdkDelegate<RemoteConfig> ResponseCallback)
        {
            yield return PostRequestOperation.PostRequest<Dictionary<string, object>>(
                $"{MeticaAPI.Config.remoteConfigEndpoint}/config/v1/apps/{MeticaAPI.AppId}",
                ConfigKeys != null && ConfigKeys.Any() ?
                new Dictionary<string, object>
                {
                    { "keys", ConfigKeys },
                } : null,
                MeticaAPI.ApiKey,
                RequestUtils.CreateRequestWithUserDataAndDeviceInfo(UserProperties, DeviceInfo),
                result =>
                {
                    if (result.Error != null)
                    {
                        ResponseCallback(SdkResultImpl<RemoteConfig>.WithError(result.Error));
                    }
                    else
                    {
                        var remoteConfig = new RemoteConfig(
                            config: result.Result.Data,
                            cacheDurationSecs: ParseCacheExpirationFromHeaders(result.Result.Headers));
                        ResponseCallback(SdkResultImpl<RemoteConfig>.WithResult(remoteConfig));
                    }
                });
        }

        private static readonly char[] ChEquals = { '=' };
        private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromHours(3);
        private static long ParseCacheExpirationFromHeaders(Dictionary<string, string> headers)
        {
            var seconds = DefaultCacheDuration.Seconds;
            var cacheControl = headers["Cache-Control"];
            if (cacheControl == null) return seconds;
            try
            {
                seconds = int.Parse(cacheControl.Split(ChEquals)[1]);
            }
            catch (Exception e)
            {
                MeticaLogger.LogError(() => $"Failed to parse the cache control directive from the header value {cacheControl}");
            }

            return seconds;
        }
    }
        #endregion CONFIG REFACTOR
}
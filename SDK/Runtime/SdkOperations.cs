using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

namespace Metica.Unity
{
    [Serializable]
    internal class RequestWithUserDataAndDeviceInfo
    {
        public string userId;
        public Dictionary<string, object> userData;
        public DeviceInfo deviceInfo;
    }

    [ExecuteAlways]
    internal class CallEventsIngestionOperation : MonoBehaviour
    {
        public IObjectPool<CallEventsIngestionOperation> pool;

        public ICollection<Dictionary<string, object>> Events { get; set; }

        public MeticaSdkDelegate<String> EventsSubmitCallback { get; set; }

        public void OnDestroyPoolObject()
        {
            pool.Release(this);
            Destroy(this);
        }

        internal IEnumerator Start()
        {
            return PostRequestOperation.PostRequest<String>($"{MeticaAPI.Config.ingestionEndpoint}/ingest/v1/events",
                null,
                MeticaAPI.ApiKey,
                CreateIngestionRequestBody(Events),
                result =>
                {
                    if (result.Error != null)
                    {
                        EventsSubmitCallback(SdkResultImpl<String>.WithError(result.Error));
                    }
                    else
                    {
                        EventsSubmitCallback(SdkResultImpl<String>.WithResult(result.Result));
                    }
                });
        }

        private static object CreateIngestionRequestBody(ICollection<Dictionary<string, object>> events)
        {
            return new Dictionary<string, object> { { "events", events } };
        }
    }

    [ExecuteAlways]
    internal class GetOffersOperation : MonoBehaviour
    {
        [Serializable]
        internal class ODSRequest : RequestWithUserDataAndDeviceInfo
        {
        }

        [Serializable]
        internal class ODSResponse
        {
            public Dictionary<string, List<Offer>> placements;
        }

        public IObjectPool<GetOffersOperation> pool;
        public string[] Placements { get; set; }
        public Dictionary<string, object> UserProperties { get; set; }
        public DeviceInfo DeviceInfo { get; set; }

        public MeticaSdkDelegate<OffersByPlacement> OffersCallback { get; set; }

        public void OnDestroyPoolObject()
        {
            pool.Release(this);
            Destroy(this);
        }

        internal IEnumerator Start()
        {
            yield return PostRequestOperation.PostRequest<ODSResponse>(
                $"{MeticaAPI.Config.offersEndpoint}/offers/v1/apps/{MeticaAPI.AppId}/users/{MeticaAPI.UserId}",
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
                            placements = result.Result.placements
                        }));
                    }
                });
        }


        static ODSRequest CreateODSRequestBody(Dictionary<string, object> userData,
            DeviceInfo overrideDeviceInfo = null)
        {
            var requestWithUserDataAndDeviceInfo =
                RequestUtils.CreateRequestWithUserDataAndDeviceInfo(userData, overrideDeviceInfo);
            var request = new ODSRequest
            {
                userId = MeticaAPI.UserId,
                userData = requestWithUserDataAndDeviceInfo.userData,
                deviceInfo = requestWithUserDataAndDeviceInfo.deviceInfo
            };

            return request;
        }
    }

    [ExecuteAlways]
    internal class CallRemoteConfigOperation : MonoBehaviour
    {
        [Serializable]
        internal class RemoteConfigRequest : RequestWithUserDataAndDeviceInfo
        {
        }
        
        public IObjectPool<CallRemoteConfigOperation> pool;
        public ICollection<string> ConfigKeys { get; set; }
        public Dictionary<string, object> UserProperties { get; set; }
        public DeviceInfo DeviceInfo { get; set; }
        public MeticaSdkDelegate<Dictionary<string, object>> ResponseCallback { get; set; }

        public void OnDestroyPoolObject()
        {
            pool.Release(this);
            Destroy(this);
        }

        internal IEnumerator Start()
        {
            return PostRequestOperation.PostRequest<Dictionary<string, object>>(
                $"{MeticaAPI.Config.remoteConfigEndpoint}/config/v1/apps/{MeticaAPI.AppId}",
                new Dictionary<string, object>
                {
                    { "keys", ConfigKeys },
                },
                MeticaAPI.ApiKey,
                RequestUtils.CreateRequestWithUserDataAndDeviceInfo(UserProperties, DeviceInfo),
                result =>
                {
                    if (result.Error != null)
                    {
                        ResponseCallback(SdkResultImpl<Dictionary<string, object>>.WithError(result.Error));
                    }
                    else
                    {
                        ResponseCallback(SdkResultImpl<Dictionary<string, object>>.WithResult(result.Result));
                    }
                });
        }
    }
    
    internal abstract class RequestUtils
    {
        internal static RequestWithUserDataAndDeviceInfo CreateRequestWithUserDataAndDeviceInfo(
            Dictionary<string, object> userData,
            DeviceInfo overrideDeviceInfo = null)
        {
            string locale = Thread.CurrentThread.CurrentCulture.Name;
            if (string.IsNullOrEmpty(locale))
            {
                locale = Constants.DefaultLocale;
            }

            var systemTz = TimeZoneInfo.Local.BaseUtcOffset;
            var timezone = ((systemTz >= TimeSpan.Zero) ? "+" : "-") + systemTz.ToString(@"hh\:mm");

            var deviceInfo = overrideDeviceInfo ?? new DeviceInfo();
            deviceInfo.locale = string.IsNullOrEmpty(overrideDeviceInfo?.locale) ? locale : overrideDeviceInfo.locale;
            deviceInfo.store = overrideDeviceInfo?.store ??
                               MapRuntimePlatformToStoreType(Application.platform).ToString();
            deviceInfo.timezone = overrideDeviceInfo?.timezone ?? timezone;
            deviceInfo.appVersion =
                overrideDeviceInfo?.appVersion ?? Application.version ?? Constants.DefaultAppVersion;

            var request = new RequestWithUserDataAndDeviceInfo
            {
                userId = MeticaAPI.UserId,
                userData = userData,
                deviceInfo = deviceInfo
            };

            return request;
        }


        private static StoreTypeEnum MapRuntimePlatformToStoreType(RuntimePlatform runtimePlatform)
        {
            switch (runtimePlatform)
            {
                case RuntimePlatform.Android:
                    return StoreTypeEnum.GooglePlayStore;
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return StoreTypeEnum.AppStore;
                default:
                    throw new Exception($"Got an unsupported application platform: {runtimePlatform}");
            }
        }
    }
}
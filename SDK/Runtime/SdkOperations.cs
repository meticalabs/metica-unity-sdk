using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

namespace Metica.Unity
{
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
        public const string DefaultLocale = "en-US";
        public const string DefaultAppVersion = "1.0.0";
        
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
        
        
        internal static ODSRequest CreateODSRequestBody(Dictionary<string, object> userData,
            DeviceInfo overrideDeviceInfo = null)
        {
            string locale = Thread.CurrentThread.CurrentCulture.Name;
            if (string.IsNullOrEmpty(locale))
            {
                locale = DefaultLocale;
            }
            
            var systemTz = TimeZoneInfo.Local.BaseUtcOffset;
            var timezone = ((systemTz >= TimeSpan.Zero) ? "+" : "-") +  systemTz.ToString(@"hh\:mm");
            
            var deviceInfo = overrideDeviceInfo ?? new DeviceInfo();
            deviceInfo.locale = string.IsNullOrEmpty(overrideDeviceInfo?.locale) ? locale : overrideDeviceInfo.locale;
            deviceInfo.store = overrideDeviceInfo?.store ??
                               MapRuntimePlatformToStoreType(Application.platform).ToString();
            deviceInfo.timezone = overrideDeviceInfo?.timezone ?? timezone;
            deviceInfo.appVersion = overrideDeviceInfo?.appVersion ?? Application.version ?? DefaultAppVersion;

            var request = new ODSRequest
            {
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
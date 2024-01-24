using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable all NotAccessedField.Global
// ReSharper disable file UnusedMember.Local

namespace Metica.Unity
{
    [Serializable]
    public class ODSRequest
    {
        public Dictionary<string, object> userData;
        public DeviceInfo deviceInfo;
    }

    [Serializable]
    public class ODSResponse
    {
        public Dictionary<string, List<Offer>> placements;
    }

    public class OffersManager
    {
        private MeticaContext _ctx;
        [CanBeNull] private CachedOffersByPlacement _cachedOffers = null;

        public void Init(MeticaContext ctx)
        {
            this._ctx = ctx;

            // load offers from disk
            var cachedOffers = OffersCache.Read();
        }

        public void GetOffers(string[] placements, MeticaSdkDelegate<OffersByPlacement> offersCallback)
        {
            var offers = new Dictionary<string, List<Offer>>();

            // if the cache is recent, and not running inside the editor, return the cached offers
            if (_cachedOffers != null && _cachedOffers.offers != null &&
                (new DateTime() - _cachedOffers.cacheTime).TotalHours < 2 &&
                !Application.isEditor)
            {
                Debug.Log("Returning cached offers");
                offersCallback(new SdkResultImpl<OffersByPlacement>(_cachedOffers.offers));
                return;
            }

            // if there is no internet connection, return the cached offers
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("No internet connection, returning cached offers");
                offersCallback(new SdkResultImpl<OffersByPlacement>(
                    _cachedOffers != null && _cachedOffers.offers != null
                        ? _cachedOffers.offers
                        : new OffersByPlacement()));
                return;
            }

            string jsonBody = "";
            try
            {
                jsonBody = CreateJsonRequestBody(new Dictionary<string, object>());
                Debug.Log("json body: " + jsonBody);
            }
            catch (Exception e)
            {
                Debug.LogError("Error while fetching offers: " + e.Message);
                Debug.LogException(e);
            }

            using (var www = new UnityWebRequest(
                       $"{MeticaAPI.MeticaOffersEndpoint}/offers/v1/apps/{_ctx.appId}/users/{_ctx.userId}?placements={String.Join(",", placements)}",
                       "POST"))
            {
                var jsonToSend = new UTF8Encoding().GetBytes(jsonBody);
                www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("X-API-KEY", _ctx.apiKey);

                www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + www.error + ", status: " + www.responseCode);
                }
                else
                {
                    Debug.Log("Response: " + www.downloadHandler.text);

                    var offersList = JsonConvert.DeserializeObject<ODSResponse>(www.downloadHandler.text);
                    Debug.Log("Got " + offersList.placements["mainMulti"].Count + " offers back");

                    offers = offersList.placements;

                    OffersCache.Write(new OffersByPlacement()
                    {
                        placements = offers
                    });
                }
            }

            offersCallback(new SdkResultImpl<OffersByPlacement>(new OffersByPlacement()
            {
                placements = offers
            }));
        }

        private StoreTypeEnum mapRuntimePlatformToStoreType(RuntimePlatform runtimePlatform)
        {
            switch (runtimePlatform)
            {
                case RuntimePlatform.Android:
                    return StoreTypeEnum.GooglePlayStore;
                case RuntimePlatform.tvOS:
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return StoreTypeEnum.AppStore;
                default:
                    throw new Exception("Got an unsupported application platform: " + runtimePlatform);
            }
        }

        private bool isSupportedPlatform()
        {
            var supportedPlatforms = new List<RuntimePlatform>
            {
                RuntimePlatform.Android, RuntimePlatform.IPhonePlayer, RuntimePlatform.tvOS, RuntimePlatform.OSXPlayer
            };
            return supportedPlatforms.Contains(Application.platform);
        }

        private string CreateJsonRequestBody([CanBeNull] Dictionary<string, object> userData)
        {
            var locale = Thread.CurrentThread.CurrentCulture.Name;

            var systemTz = TimeZoneInfo.Local.BaseUtcOffset;
            Debug.Log(TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes);
            var deviceInfo = new DeviceInfo
            {
                locale = locale,
                store = mapRuntimePlatformToStoreType(Application.platform).ToString(),
                timezone = systemTz.TotalMinutes == 0
                    ? "+00:00"
                    : $"{systemTz.Hours:D2}:{systemTz.Minutes:D2}",
                appVersion = Application.version
            };

            var request = new ODSRequest
            {
                userData = userData,
                deviceInfo = deviceInfo
            };

            return JsonConvert.SerializeObject(request);
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
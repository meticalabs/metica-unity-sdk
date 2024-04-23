using System;
using System.Collections;
using System.Collections.Generic;
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
                BackendOperations.CreateIngestionRequestBody(Events),
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
    }

    [ExecuteAlways]
    internal class GetOffersOperation : MonoBehaviour
    {
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
                BackendOperations.CreateODSRequestBody(UserProperties, DeviceInfo),
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
    }
}
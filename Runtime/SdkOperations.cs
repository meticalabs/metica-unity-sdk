using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Metica.Unity
{
    [ExecuteAlways]
    internal class SubmitEventsOperation : MonoBehaviour
    {
        public List<Dictionary<string, object>> Events { get; set; }

        public MeticaSdkDelegate<String> EventsSubmitCallback { get; set; }

        internal IEnumerator Start()
        {
            return PostRequestOperation.PostRequest<String>($"{MeticaAPI.MeticaIngestionEndpoint}/ingest/v1/events",
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

                    if (!Application.isEditor)
                    {
                        Destroy(this);
                    }
                    else
                    {
                        DestroyImmediate(this);
                    }
                });
        }
    }

    [ExecuteAlways]
    internal class GetOffersOperation : MonoBehaviour
    {
        public string[] Placements { get; set; }
        public Dictionary<string, object> UserProperties { get; set; }
        public DeviceInfo DeviceInfo { get; set; }

        public MeticaSdkDelegate<OffersByPlacement> OffersCallback { get; set; }

        internal IEnumerator Start()
        {
            yield return PostRequestOperation.PostRequest<ODSResponse>(
                $"{MeticaAPI.MeticaOffersEndpoint}/offers/v1/apps/{MeticaAPI.AppId}/users/{MeticaAPI.UserId}",
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

                    if (!Application.isEditor)
                    {
                        Destroy(this);
                    }
                    else
                    {
                        DestroyImmediate(this);
                    }
                });
        }
    }
}
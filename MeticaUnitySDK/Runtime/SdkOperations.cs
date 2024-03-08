using System;
using System.Collections;
using System.Collections.Generic;
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
                        Debug.Log("Notifying error");
                        EventsSubmitCallback(SdkResultImpl<String>.WithError(result.Error));
                    }
                    else
                    {
                        Debug.Log("Notifying result");
                        EventsSubmitCallback(SdkResultImpl<String>.WithResult(result.Result));
                    }

                    Debug.Log("Destroying self");
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

        public MeticaSdkDelegate<OffersByPlacement> OffersCallback { get; set; }

        internal IEnumerator Start()
        {
            yield return PostRequestOperation.PostRequest<ODSResponse>(
                $"{MeticaAPI.MeticaOffersEndpoint}/offers/v1/apps/{MeticaAPI.AppId}/users/{MeticaAPI.UserId}?placements={String.Join(",", Placements)}",
                $"placements={String.Join(",", Placements)}",
                MeticaAPI.ApiKey,
                BackendOperations.CreateODSRequestBody(new Dictionary<string, object>()),
                result =>
                {
                    if (result.Error != null)
                    {
                        Debug.Log("Notifying error");
                        OffersCallback(SdkResultImpl<OffersByPlacement>.WithError(result.Error));
                    }
                    else
                    {
                        Debug.Log("Notifying result");
                        OffersCallback(SdkResultImpl<OffersByPlacement>.WithResult(new OffersByPlacement
                        {
                            placements = result.Result.placements
                        }));
                    }

                    Debug.Log("Destroying self");
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


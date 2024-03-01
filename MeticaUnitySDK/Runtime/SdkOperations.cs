using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Metica.Unity
{
    public class SubmitEventsOperation : MonoBehaviour
    {
        public MeticaContext Context { get; set; }
        public List<Dictionary<string, object>> Events { get; set; }

        // TODO: implement this
    }

    [ExecuteAlways]
    public class GetOffersOperation : MonoBehaviour
    {
        public MeticaContext Context { get; set; }

        public string[] Placements { get; set; }

        public MeticaSdkDelegate<OffersByPlacement> OffersCallback { get; set; }

        internal IEnumerator Start()
        {
            return PostRequestOperation.PostRequest<ODSResponse>(
                $"{MeticaAPI.MeticaOffersEndpoint}/offers/v1/apps/{Context.appId}/users/{Context.userId}?placements={String.Join(",", Placements)}",
                $"placements={String.Join(",", Placements)}",
                Context.apiKey,
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
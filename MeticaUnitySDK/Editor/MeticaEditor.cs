using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;

// ReSharper disable all NotAccessedField.Global
// ReSharper disable file UnusedMember.Local

namespace Metica.Unity
{
    [ExecuteInEditMode]
    public class MeticaSdkEdit : MonoBehaviour
    {
        public string appId;

        void Update()
        {
        }
    }

    public class MeticaEditor : MonoBehaviour
    {
        public string appId;
        public string apiKey;
        public string meticaEndpoint = "http://localhost:9090";
        public string[] placements = new String[] { };

        // Expose the GetOffers method in the editor
        [ContextMenu("Get Offers")]
        private void GetOffersInEditor()
        {
            var ctx = new MeticaContext
            {
                appId = appId,
                apiKey = apiKey,
                userId = "test-user"
            };

            if (!MeticaAPI.Initialized)
            {
                MeticaAPI.Initialise(ctx.userId, ctx.appId, ctx.apiKey, result =>
                {
                    if (result.Error != null)
                    {
                        Debug.LogError(result);
                    }
                });
            }
            
            var offersManager = new OffersManager();
            offersManager.Init(ctx);
            var resDelegate = new MeticaSdkDelegate<OffersByPlacement>(result =>
            {
                Debug.Log(result.Result);
                Debug.Log(result.Error);
                if (result.Error != null)
                {
                    Debug.LogError("Error while fetching offers: " + result.Error);
                }
                else
                {
                    foreach (var p in placements)
                    {
                        var offers = result.Result.placements.ContainsKey(p) ? result.Result.placements[p] : new List<Offer>();
                        Debug.Log($"Placement {p} offers: {JsonConvert.SerializeObject(offers)}" );
                    }
                }
            });

            EditorApplication.delayCall += () => { offersManager.GetOffers(new String[] { "main" }, resDelegate); };
        }
    }
}
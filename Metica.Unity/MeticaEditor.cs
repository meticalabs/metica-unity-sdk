using System;
using UnityEngine;

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

#if UNITY_EDITOR
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

            var offersManager = new OffersManager();
            offersManager.Init(ctx);
            var resDelegate = new MeticaSdkDelegate<OffersByPlacement>(result =>
            {
                if (result.Error != null)
                {
                    Debug.LogError("Error while fetching offers: " + result.Error);
                }
                else
                {
                    foreach (var p in placements)
                    {
                        var offers = result.Result.placements.ContainsKey(p) ? result.Result.placements[p] : new List<Offer>();
                        Debug.Log($"Placement {p} offers: {JSON.Serialize(offers)}" );
                    }
                }
            });

            EditorApplication.delayCall += () => { offersManager.GetOffers(new String[] { "main" }, resDelegate); };
        }
#endif
    }
}
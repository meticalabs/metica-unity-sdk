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
        private string _appId;
        private string _apiKey;
        private string _meticaEndpoint;
        private string _userId;
        
        public string appId
        {
            get => _appId;
            set
            {
                _appId = value;
                Init();
            }
        }
        
        public string apiKey
        {
            get => _apiKey;
            set
            {
                _apiKey = value;
                Init();
            }
        }

        public string userId 
        {
            get => _userId;
            set
            {
                _userId = value;
                Init();
            }
        }

        public string meticaEndpoint
        {
            get => _meticaEndpoint;
            set
            {
                _meticaEndpoint = value;
                Init();
            }
        }

        public string[] placements = new String[] { };
        
        private MeticaContext Init()
        {
            var ctx = new MeticaContext
            {
                appId = _appId,
                apiKey = _apiKey,
                userId = _userId
            };

            if (!MeticaAPI.Initialized)
            {
                Debug.Log("Initializing In-Editor Metica SDK");
                MeticaAPI.Initialise(ctx.userId, ctx.appId, ctx.apiKey, result =>
                {
                    if (result.Error != null)
                    {
                        Debug.LogError(result);
                    }
                });
            }

            return ctx;
        }

        
        public void GetOffersInEditor(MeticaSdkDelegate<OffersByPlacement> callback)
        {
            var offersManager = new OffersManager();
            offersManager.Init();
             
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
                        var offers = result.Result.placements.ContainsKey(p)
                            ? result.Result.placements[p]
                            : new List<Offer>();
                        Debug.Log($"Placement {p} offers: {JsonConvert.SerializeObject(offers)}");
                    }
                    callback.Invoke(result);
                }
            });

            offersManager.GetOffers(new[] { "main" }, resDelegate); 
            // EditorApplication.delayCall += () => { offersManager.GetOffers(new String[] { "main" }, resDelegate); };
        }

        // Expose the GetOffers method in the editor
        [ContextMenu("Get Offers")]
        public void GetOffersInEditor()
        {
            GetOffersInEditor(result => { });

        }
    }
}
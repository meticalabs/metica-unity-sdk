using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Metica.Unity
{
    public class MeticaEditorAPI
    {
        [SerializeField]
        private string _appId;
        [SerializeField]
        private string _apiKey;
        [SerializeField]
        private string _ingestionEndpoint;
        private string _offersEndpoint;
        [SerializeField]
        private string _userId;
        
        public string AppId
        {
            get => _appId;
            set
            {
                _appId = value;
                Init();
            }
        }
        
        public string APIKey
        {
            get => _apiKey;
            set
            {
                _apiKey = value;
                Init();
            }
        }

        public string UserId 
        {
            get => _userId;
            set
            {
                _userId = value;
                Init();
            }
        }

        public string IngestionEndpoint
        {
            get => _ingestionEndpoint;
            set
            {
                _ingestionEndpoint = value;
                Init();
            }
        }
        
        public string OffersEndpoint
        {
            get => _ingestionEndpoint;
            set
            {
                _ingestionEndpoint = value;
                Init();
            }
        }

        internal void Init()
        {
            var ctx = new MeticaContext
            {
                appId = AppId,
                apiKey = APIKey,
                userId = UserId
            };

            MeticaAPI.MeticaOffersEndpoint = OffersEndpoint;
            MeticaAPI.MeticaIngestionEndpoint = IngestionEndpoint;
            
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
            else
            {
                MeticaAPI.Context = ctx;
            }
        }

        internal List<DisplayLogEntry> GetDisplayLog(string offerId)
        {
            Init();
            return MeticaAPI.DisplayLog.GetEntriesForOffer(offerId);
        }
        
        public void GetOffersInEditor(string[] placements, MeticaSdkDelegate<OffersByPlacement> callback)
        {
            Init();
            MeticaAPI.GetOffers(placements, callback);
            // var offersManager = new OffersManager();
            // offersManager.Init();
            // offersManager.GetOffers(placements, callback); 
            // EditorApplication.delayCall += () => { offersManager.GetOffers(new String[] { "main" }, resDelegate); };
        }
    }
}
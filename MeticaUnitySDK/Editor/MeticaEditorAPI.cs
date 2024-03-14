using System.Collections.Generic;
using UnityEngine;

namespace Metica.Unity
{
    public class MeticaEditorAPI
    {
        private string _appId;
        private string _apiKey;
        private string _ingestionEndpoint;
        private string _offersEndpoint;
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
            get => _offersEndpoint;
            set
            {
                _offersEndpoint = value;
                Init();
            }
        }

        private void Init()
        {
            MeticaAPI.AppId = AppId;
            MeticaAPI.ApiKey = APIKey;
            MeticaAPI.UserId = UserId;
            MeticaAPI.MeticaOffersEndpoint = OffersEndpoint;
            MeticaAPI.MeticaIngestionEndpoint = IngestionEndpoint;

            if (!MeticaAPI.Initialized)
            {
                Debug.Log("Initializing In-Editor Metica SDK");
                MeticaAPI.Initialise(UserId, AppId, APIKey, result =>
                {
                    if (result.Error != null)
                    {
                        Debug.LogError(result);
                    }
                });
            }
        }

        internal List<DisplayLogEntry> GetDisplayLog(string offerId)
        {
            Init();
            return MeticaAPI.DisplayLog.GetEntriesForOffer(offerId);
        }

        public void LogOfferDisplay(string offerId, string placementId)
        {
            Init();
            MeticaAPI.LogOfferDisplay(offerId, placementId);
        }

        public void LogOfferPurchase(string offerId, string placementId, double amount, string currency)
        {
            Init();
            MeticaAPI.LogOfferPurchase(offerId, placementId, amount, currency);
        }

        public void LogOfferInteraction(string offerId, string placementId, string interactionType)
        {
            Init();
            MeticaAPI.LogOfferInteraction(offerId, placementId, interactionType);
        }

        public void LogUserEvent(Dictionary<string, object> userEvent)
        {
            Init();
            MeticaAPI.LogUserEvent(userEvent);
        }

        public void LogUserAttributes(Dictionary<string, object> userEvent)
        {
            Init();
            MeticaAPI.LogUserAttributes(userEvent);
        }
        
        public void GetOffersInEditor(string[] placements, MeticaSdkDelegate<OffersByPlacement> callback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            Init();
            MeticaAPI.GetOffers(placements, callback, userProperties, deviceInfo);
        }

        internal EventsLogger GetEventsLogger()
        {
            Init();
            return ScriptingObjects.GetComponent<EventsLogger>();
        }
    }
}
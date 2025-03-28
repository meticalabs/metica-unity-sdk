using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Metica.Unity;

// Include one or both these two lines where MeticaLogger or OfferByPlacement are not found.
using MeticaLogger = Log;
using OffersByPlacement = Metica.SDK.OfferResult;

/// <summary>
/// Example using call style as used prior to 1.3.1 release.
/// This code style still works but using async calls is recommended.
/// *Simulator* is surely a hyperbolic name but this roughly simulate a game/app session.
/// </summary>
public class Simulator : MonoBehaviour
{
    [SerializeField]
    bool _getConfig = true,
        _logLogin = true,
        _logInstall = true,
        _logOfferPurchase = true,
        _logOfferInteraction = true,
        _logOfferDisplay = true,
        _logAdRevenue = true,
        _logStateUpdate = true,
        _logCustom = true;

    private void Start()
    {
        StartCoroutine(TestCoroutine());
    }

    /// <summary>
    /// This is an example delegate to use, in this case, with <see cref="Metica.Unity.MeticaAPI.GetOffers(string[], MeticaSdkDelegate{OffersByPlacement}, Dictionary{string, object}, Metica.SDK.Model.DeviceInfo)"/>
    /// </summary>
    /// <param name="result"></param>
    private void OnOffersReceived(ISdkResult<OffersByPlacement> result)
    {
        Log.Debug(() => $"Delegate processing : {result.Result.ToString()}");
    }

    /// <summary>
    /// Long coroutine to roughly simulate an application behaviour.
    /// </summary>
    /// <returns></returns>
    private IEnumerator TestCoroutine()
    {
        bool canProceed = false;

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

        if (_logInstall)
        {
            MeticaLogger.LogDebug(() => "<i>Install Event</i>");

            //MeticaAPI.LogUserEvent("install", new Dictionary<string, object>());
            MeticaAPI.LogInstall();

            yield return new WaitForSeconds(1f);
        }

        if (_logLogin)
        {
            Log.Debug(() => "<i>Login Event</i>");
            MeticaAPI.LogLogin();

            yield return new WaitForSeconds(1f);
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

        MeticaLogger.LogDebug(() => "<i>Getting ALL Offers</i>");

        OffersByPlacement offersByPlacements = new();
        MeticaAPI.GetOffers(new string[] { }, (result) =>
        {
            Log.Debug(() => result.Result.ToString());
            offersByPlacements = result.Result;
            canProceed = true;
        });
        yield return new WaitUntil(() => canProceed == true);
        canProceed = false;

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

        MeticaLogger.LogDebug(() => "<i>Getting Offers By Placement</i>");

        foreach (var placement in offersByPlacements.placements)
        {
            MeticaAPI.GetOffers(new string[] { placement.Key }, (result) =>
            {
                Log.Debug(() => result.Result.ToString());
                offersByPlacements = result.Result;
                canProceed = true;
            },
            userProperties: null,
            deviceInfo: null
            );
            yield return new WaitUntil(() => canProceed == true);
            canProceed = false;
        }

        MeticaLogger.LogDebug(() => "<i>Getting Offers By Placement, process with DELEGATE</i>");
        MeticaAPI.GetOffers(new string[] { }, OnOffersReceived);

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

        if (_getConfig)
        {
            MeticaLogger.LogDebug(() => "<i>Getting all Configs</i>");

            MeticaAPI.GetConfig((result) =>
            {
                MeticaLogger.LogDebug(() => result.Result.ToString());
                canProceed = true;
            },
            configKeys: null,
            userProperties: null,
            deviceInfo: null
            );
            yield return new WaitUntil(() => canProceed == true);
            canProceed = false;
        }

        if (_getConfig)
        {
            MeticaLogger.LogDebug(() => "<i>Getting specific Configs</i>");

            MeticaAPI.GetConfig((result) =>
            {
                MeticaLogger.LogDebug(() => result.Result.ToString());
                canProceed = true;
            },
            configKeys: new List<string> { "hello_world" },
            userProperties: null,
            deviceInfo: null
            );
            yield return new WaitUntil(() => canProceed == true);
            canProceed = false;
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

        string[] placementsList = offersByPlacements.placements.Keys.ToArray();

        if (_logOfferPurchase)
        {
            MeticaLogger.LogDebug(() => "<i>Offer Purchase</i>");

            for (var i = 0; i < 4; i++)
            {
                int rand = Random.Range(0, placementsList.Length);
                string randPlacement = placementsList[rand];
                var placement = offersByPlacements.placements[randPlacement];
                if (placement.Count > 0)
                {
                    MeticaAPI.LogOfferPurchase(placement[Random.Range(0, placement.Count)].offerId, randPlacement, Random.Range(0.1f, 99.0f), "EUR");
                    yield return new WaitForSeconds(0.2f);
                }
            }

            for (var i = 0; i < 4; i++)
            {
                MeticaAPI.LogOfferPurchaseWithProductId(i % 2 == 0 ? "Apples" : "Pears", Random.Range(0.2f, 100.0f), "EUR");
                yield return new WaitForSeconds(0.2f);
            }

            for (var i = 0; i < 4; i++)
            {
                Dictionary<string, object> customPayload = new Dictionary<string, object> { { "screen", $"game{i + 3}" } };
                MeticaAPI.LogOfferPurchaseWithProductId(i % 2 == 0 ? "Coffee" : "Toffee", Random.Range(0.2f, 100.0f), "EUR", customPayload);
                yield return new WaitForSeconds(0.2f);
            }
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

        if (_logOfferInteraction)
        {
            MeticaLogger.LogDebug(() => "<i>Offer Interaction</i>");

            for (var i = 0; i < 4; i++)
            {
                int rand = Random.Range(0, placementsList.Length);
                string randPlacement = placementsList[rand];
                var placement = offersByPlacements.placements[randPlacement];
                if (placement.Count > 0)
                {
                    MeticaAPI.LogOfferInteraction(placement[Random.Range(0, placement.Count)].offerId, randPlacement, "click");
                    yield return new WaitForSeconds(0.2f);
                }
            }

            for (var i = 0; i < 4; i++)
            {
                MeticaAPI.LogOfferInteractionWithProductId(i % 2 == 0 ? "Apples" : "Pears", i % 3 == 0 ? "click" : "dismiss");
                yield return new WaitForSeconds(0.2f);
            }

            for (var i = 0; i < 4; i++)
            {
                Dictionary<string, object> customPayload = new Dictionary<string, object> { { "customField", (i % 2 == 0) ? "xyz" : "abc" } };
                MeticaAPI.LogOfferInteractionWithProductId(i % 2 == 0 ? "Coffee" : "Toffee", i % 3 == 0 ? "click" : "dismiss", customPayload);
                yield return new WaitForSeconds(0.2f);
            }
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

        if (_logOfferDisplay)
        {
            MeticaLogger.LogDebug(() => "<i>Offer Display/Impression</i>");

            for (var i = 0; i < 4; i++)
            {
                int rand = Random.Range(0, placementsList.Length);
                string randPlacement = placementsList[rand];
                var placement = offersByPlacements.placements[randPlacement];
                if (placement.Count > 0)
                {
                    MeticaAPI.LogOfferDisplay(placement[Random.Range(0, placement.Count)].offerId, randPlacement);
                    yield return new WaitForSeconds(0.2f);
                }
            }

            for (var i = 0; i < 4; i++)
            {
                MeticaAPI.LogOfferDisplayWithProductId(i % 2 == 0 ? "Apples" : "Pears");
                yield return new WaitForSeconds(0.2f);
            }

            for (var i = 0; i < 4; i++)
            {
                Dictionary<string, object> customPayload = new Dictionary<string, object> { { "display", $"display{i * 3}" } };
                MeticaAPI.LogOfferDisplayWithProductId(i % 2 == 0 ? "Coffee" : "Toffee", customPayload);
                yield return new WaitForSeconds(0.2f);
            }
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

        if (_logAdRevenue)
        {
            MeticaLogger.LogDebug(() => "<i>Ad Revenue</i>");


            for (var i = 0; i < 4; i++)
            {
                MeticaAPI.LogAdRevenue(Random.Range(0.1f, 100f), "EUR", i % 3 == 0 ? null : "mainScreen", i % 2 == 0 ? null : "banner", Random.Range(0, 10) < 5 ? null : "babazon");
                yield return new WaitForSeconds(0.2f);
            }

            for (var i = 0; i < 4; i++)
            {
                Dictionary<string, object> customPayload = new Dictionary<string, object> { { "adTime", $"{(i + 1) * 9}s" } };
                MeticaAPI.LogAdRevenue(Random.Range(0.1f, 100f), "EUR", i % 3 == 0 ? null : "mainScreen", i % 2 == 0 ? null : "banner", Random.Range(0, 10) < 5 ? null : "babazon", customPayload);
                yield return new WaitForSeconds(0.2f);
            }
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

        if (_logStateUpdate)
        {
            MeticaLogger.LogDebug(() => "<i>State Update</i>");

            for (int i = 0; i < 4; i++)
            {
                int level = Random.Range(0, 10);
                MeticaAPI.LogFullStateUpdate(new Dictionary<string, object>()
            {
                { "name", "Mork" },
                { "items", new List<string>() { "sheild0299028", "sword2983786y4", "boots08ds8d88", "belt9d8yh9f7dsf" } },
                { "characterName", "Lana" },
                { "level", level },
            });
                yield return new WaitForSeconds(0.5f);
            }

            for (int i = 0; i < 4; i++)
            {
                int level = Random.Range(0, 10);
                MeticaAPI.LogPartialStateUpdate(new Dictionary<string, object>()
            {
                { "name", "Mork" },
                { "characterName", "Lana" },
                { "level", level+i },
            });
                yield return new WaitForSeconds(0.5f);
            }

            for (int i = 0; i < 4; i++)
            {
                Dictionary<string, object> customPayload = new Dictionary<string, object> { { "savedPrincess", (i % 2 == 0) ? "yes" : "no" } };
                int level = Random.Range(0, 10);
                MeticaAPI.LogPartialStateUpdate(new Dictionary<string, object>()
            {
                { "name", "Mork" },
                { "characterName", "Lana" },
                { "level", level+i },
            });
                yield return new WaitForSeconds(0.5f);
            }
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

        if (_logCustom)
        {
            List<string> customEventTypes = new List<string>() { "gameOver", "rageQuit", "ultraBonus", "totalDomination" };
            List<Dictionary<string, object>> customAttributes = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>(){{ "matchTime", Random.Range(1,3000) }},
                new Dictionary<string, object>(){{ "matchTime", Random.Range(1,200) }, { "difficulty", Random.Range(1,10) } },
                new Dictionary<string, object>(){{ "prizeId", $"prize{Random.Range(1,30)}" }},
                new Dictionary<string, object>(){},
            };
            for (int i = 0; i < 4; i++)
            {
                int e = Random.Range(0, customEventTypes.Count);
                MeticaAPI.LogCustomEvent(customEventTypes[e], customAttributes[e]);
            }
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

        yield return null;

        MeticaLogger.LogDebug(() => "Test coroutine finished.");
    }
}

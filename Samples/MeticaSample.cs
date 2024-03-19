using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Metica.Unity
{
    public class MeticaSample : MonoBehaviour
    {
        GameObject canvas;
        Text textElement;
        Button fetchOffersButton;

        // For controlling panel visibility
        private bool isPanelVisible = false;

        void Start()
        {
            // Create new Canvas GameObject
            canvas = new GameObject("Canvas");
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();

            // Create the Button
            var elements = new GameObject("Elements");
            elements.transform.SetParent(canvas.transform, false);

            // textElement = new GameObject("Text", typeof(Text)).GetComponent<Text>();
            textElement = elements.AddComponent<Text>();
            textElement.transform.SetParent(canvas.transform, false);
            textElement.color = Color.white;

            Font legacyFont = (Font)Resources.GetBuiltinResource(typeof(Font), "LegacyRuntime.ttf");
            textElement.font = legacyFont;
            textElement.material = legacyFont.material;

            // Position the button and the text on the canvas
            // transform.localPosition = new Vector3(0, 0, 0);
            textElement.GetComponent<RectTransform>().localPosition = new Vector3(0, 50, 0);

            MeticaAPI.Initialise("userId", "appId", "apiKey",
                (result => textElement.text = (result.Result ? "Initialised" : "Failed to initialise")));
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                MeticaAPI.GetOffers(new[] { "main" }, (result) =>
                {
                    if (result.Error != null)
                    {
                        textElement.text = "Error: " + result.Error;
                    }
                    else
                    {
                        var resultPlacements = result.Result.placements;
                        textElement.text = "Offers: " + (resultPlacements.ContainsKey("main") ? resultPlacements["main"].Count : 0) + " offers found";
                    }
                });
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                textElement.text = "Logging offer display";
                MeticaAPI.LogOfferDisplay("offerId", "main");
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                textElement.text = "Logging user attributes";
                MeticaAPI.LogUserAttributes(new Dictionary<string, object>()
                {
                    { "name", "Joe" },
                    { "level", 5 },
                });
            }
        }
    }
}
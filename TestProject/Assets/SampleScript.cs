using System.Collections.Generic;
using Metica.Unity;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class SampleScript : MonoBehaviour
{
    [SerializeField] private string _userId = string.Empty;
    [SerializeField] private string _appId = string.Empty;
    [SerializeField] private string _apiKey = string.Empty;
    [SerializeField] private SdkConfigProvider _sdkConfiguration;

    GameObject canvas;
    Text textElement;

    void Start()
    {
        Assert.IsFalse(string.IsNullOrEmpty(_userId));
        Assert.IsFalse(string.IsNullOrEmpty(_appId));
        Assert.IsFalse(string.IsNullOrEmpty(_apiKey));

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

        Font legacyFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        textElement.font = legacyFont;
        textElement.material = legacyFont.material;

        // Position the button and the text on the canvas
        // transform.localPosition = new Vector3(0, 0, 0);
        textElement.GetComponent<RectTransform>().localPosition = new Vector3(0, 50, 0);

        MeticaAPI.Initialise(_userId, _appId, _apiKey, _sdkConfiguration.SdkConfig,
            (result => textElement.text = (result.Result ? "Initialised" : "Failed to initialise")));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
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

        if (Input.GetKeyDown(KeyCode.C))
        {
            MeticaAPI.GetConfig(result =>
            {
                if (result.Error != null)
                {
                    textElement.text = "GetConfig Error: " + result.Error;
                }
                else
                {
                    var configStr = JsonConvert.SerializeObject(result.Result);
                    textElement.text = "Config: " + configStr;
                }
            });
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            textElement.text = "Logging offer display";
            MeticaAPI.LogOfferDisplay("offerId", "main");
        }

        if (Input.GetKeyDown(KeyCode.U))
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

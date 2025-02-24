using System.Collections.Generic;
using System.Text;
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

    [SerializeField] Text textElement, _versionText;
    [SerializeField] Button _getOffersButton, _getConfigButton, _getConfigSpecificButton, _logOfferDisplayButton, _logUserAttributesButton, _logPartialUserAttributesButton, _logAdRevenueButton;
    [SerializeField] InputField _configIdInput;

    void Start()
    {
        Assert.IsFalse(string.IsNullOrEmpty(_userId));
        Assert.IsFalse(string.IsNullOrEmpty(_appId));
        Assert.IsFalse(string.IsNullOrEmpty(_apiKey));
        Assert.IsNotNull(_sdkConfiguration, "Please assign an Sdk Configuration. A new one can be created in Create > Metica > SDK > New SDK Configuration.");

        Font legacyFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        textElement.font = legacyFont;
        textElement.material = legacyFont.material;

        // Position the button and the text on the canvas
        // transform.localPosition = new Vector3(0, 0, 0);
        textElement.GetComponent<RectTransform>().localPosition = new Vector3(0, 50, 0);

        MeticaAPI.Initialise(_userId, _appId, _apiKey, _sdkConfiguration.SdkConfig,
            (result => textElement.text = (result.Result ? "Initialised" : "Failed to initialise")));

        _getOffersButton.onClick.AddListener(TestGetOffers);
        _getConfigButton.onClick.AddListener(TestGetConfig);
        _getConfigSpecificButton.onClick.AddListener(TestGetConfigSpecific);
        _logOfferDisplayButton.onClick.AddListener(TestLogOfferDisplay);
        _logUserAttributesButton.onClick.AddListener(TestLogUserAttributes);
        _logPartialUserAttributesButton.onClick.AddListener(TestPartialLogUserAttributes);
        _logAdRevenueButton.onClick.AddListener(TestLogAdRevenue);

        _versionText.text = MeticaAPI.SDKVersion;
    }

    private void TestGetOffers()
    {
        MeticaAPI.UserId = _userId;
        MeticaAPI.GetOffers(null, (result) =>
        {
            if (result.Error != null)
            {
                textElement.text = "Error: " + result.Error;
            }
            else
            {
                var resultPlacements = result.Result.placements;
                //textElement.text = "Offers: " + (resultPlacements.ContainsKey("generic") ? resultPlacements["generic"].Count : 0) + " offers found";
                StringBuilder sb = new();
                foreach (var placement in resultPlacements)
                {
                    sb.Append($"Offers:\n[{placement.Key}] #{placement.Value.Count}\n");
                    foreach (var p in placement.Value)
                    {
                        sb.AppendLine($"\tid:{p.offerId}");
                    }
                }
                textElement.text = sb.ToString();
            }
        });
    }

    private void TestGetConfig()
    {
        MeticaAPI.UserId = _userId;
        // Retrieve all configs
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

    private void TestGetConfigSpecific()
    {
        MeticaAPI.UserId = _userId;
        // Retrieve config with specific name
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
        }, new List<string> { _configIdInput.text });
    }

    private void TestLogOfferDisplay()
    {
        textElement.text = "Logging offer display";
        MeticaAPI.LogOfferDisplay("offerId", "23851");
    }

    private void TestLogUserAttributes()
    {
        textElement.text = "Logging user attributes";
        MeticaAPI.LogUserAttributes(new Dictionary<string, object>()
            {
                { "name", "Joe" },
                { "level", 5 },
            });
    }

    private void TestPartialLogUserAttributes()
    {
        textElement.text = "Logging user attributes";
        MeticaAPI.LogPartialStateUpdate(new Dictionary<string, object>()
            {
                //{ "name", "Joe" },
                { "level", 6 },
            });
    }

    private void TestLogAdRevenue()
    {
        textElement.text = "Logging Ad Revenue";
        MeticaAPI.LogAdRevenue(0.0001d, "GBP", "GameOverScreen", "video", "serendipitAds");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            TestGetOffers();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            TestGetConfig();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            TestGetConfigSpecific();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            TestLogOfferDisplay();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            TestLogUserAttributes();
        }
    }

    private void OnDestroy()
    {
        _getOffersButton.onClick.RemoveAllListeners();
        _getConfigButton.onClick.RemoveAllListeners();
        _getConfigSpecificButton.onClick.RemoveAllListeners();
        _logOfferDisplayButton.onClick.RemoveAllListeners();
        _logUserAttributesButton.onClick.RemoveAllListeners();
        _logPartialUserAttributesButton.onClick.RemoveAllListeners();
    }
}

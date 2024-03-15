using UnityEngine;
using UnityEngine.UI;

namespace Metica.Unity
{
    public class MeticaSample : MonoBehaviour
    {
        private GameObject panel;
    private Button fetchOffersButton;
    private Text offersText;
    
    // For controlling panel visibility
    private bool isPanelVisible = false;

    void Start()
    {
        // Create the Panel
        panel = new GameObject("OffersPanel");
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(200, 100);
        var panelImage = panel.AddComponent<Image>();
        panelImage.color = Color.blue; // Dark blue color can be set by changing RGB values
        panel.transform.SetParent(GameObject.Find("Canvas").transform, false); // Assuming a Canvas already exists in the scene

        // Create the Button
        var buttonObj = new GameObject("FetchOffersButton");
        buttonObj.transform.SetParent(panel.transform, false);
        fetchOffersButton = buttonObj.AddComponent<Button>();
        fetchOffersButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -25);
        fetchOffersButton.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 30);
        var buttonText = new GameObject("ButtonText").AddComponent<Text>();
        buttonText.transform.SetParent(buttonObj.transform, false);
        buttonText.text = "Fetch Offers";
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.black;
        var buttonImage = buttonObj.AddComponent<Image>();
        fetchOffersButton.targetGraphic = buttonImage;

        // Set the button's text font
        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        buttonText.font = ArialFont;
        buttonText.material = ArialFont.material;

        // Create the Text for offers
        var textObj = new GameObject("OffersText");
        textObj.transform.SetParent(panel.transform, false);
        offersText = textObj.AddComponent<Text>();
        offersText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 15);
        offersText.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 60);
        offersText.font = ArialFont;
        offersText.color = Color.white;
        offersText.supportRichText = true;
        offersText.alignment = TextAnchor.UpperLeft;
        offersText.text = "";

        // Initially hide the panel
        panel.SetActive(isPanelVisible);

        // Add listener to the button
        fetchOffersButton.onClick.AddListener(FetchOffers);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            isPanelVisible = !isPanelVisible;
            panel.SetActive(isPanelVisible);
        }
    }

    void FetchOffers()
    {
        // This is where you put the logic to fetch and display offers
        // Placeholder content for demonstration
        offersText.text = "Offer 1\nOffer 2\nOffer 3";
    }

    }
}
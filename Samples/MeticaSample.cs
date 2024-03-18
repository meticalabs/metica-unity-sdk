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
            var buttonObj = new GameObject("FetchOffersButton");
            buttonObj.transform.SetParent(canvas.transform, false);
            
            fetchOffersButton = buttonObj.AddComponent<Button>();
            // fetchOffersButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -25);
            fetchOffersButton.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 30);
            fetchOffersButton.transform.SetParent(canvas.transform, false);
            
            var buttonText = new GameObject("ButtonText").AddComponent<Text>();
            buttonText.transform.SetParent(buttonObj.transform, false);
            
            buttonText.text = "Fetch Offers";
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.black;
            
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = Color.gray;
            fetchOffersButton.targetGraphic = buttonImage;

            // Set the button's text font
            Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            buttonText.font = ArialFont;
            buttonText.material = ArialFont.material;

            // Create the Button GameObject
            // button = new GameObject("Button", typeof(Button)).GetComponent<Button>();
            // button.transform.SetParent(canvas.transform);

            // Set button size 
            // RectTransform transform = button.GetComponent<RectTransform>();
            // transform.sizeDelta = new Vector2(160, 30);

            // Set Button Text
            // button.gameObject.AddComponent<Text>();
            // Text buttonText = button.GetComponent<Text>();
            // buttonText.text = "Click Me!";
            // buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            // buttonText.color = Color.black;

            // Button click action
            // button.onClick.AddListener(delegate { buttonText.text = "Clicked!"; });

            // Create the Text GameObject
            // textElement = new GameObject("Text", typeof(Text)).GetComponent<Text>();
            // textElement.transform.SetParent(canvas.transform);
            //
            // // Set Text properties
            // textElement.text = "Not Clicked";
            // textElement.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            // textElement.color = Color.white;
            //
            // // Set Text Background
            // textElement.gameObject.AddComponent<Image>();
            // Image textBackground = textElement.GetComponent<Image>();
            // textBackground.color = Color.blue;
            //
            // // Position the button and the text on the canvas
            // transform.localPosition = new Vector3(0, 0, 0);
            // textElement.GetComponent<RectTransform>().localPosition = new Vector3(0, 50, 0);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                isPanelVisible = !isPanelVisible;
                canvas.SetActive(isPanelVisible);
            }
        }
    }
}
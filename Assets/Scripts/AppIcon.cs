using UnityEngine;
using UnityEngine.UI;

public class AppIcon : MonoBehaviour
{
    public Image indicator; // Reference to the indicator image
    private AppState currentState = AppState.Closed;

    private void Start()
    {
        // Hide the indicator initially
        UpdateIndicator(AppState.Closed);
    }

    // Method to update the indicator based on the app's state
    public void UpdateIndicator(AppState state)
    {
        currentState = state;
        switch (state)
        {
            case AppState.Opened:
                // Set the indicator visible and color to yellow
                indicator.gameObject.SetActive(true);
                SetIndicatorColorFromHex("#F7BD48");
                ChangeIndicatorWidth(18);
                break;
            case AppState.Minimized:
                // Set the indicator smaller width and color to black
                indicator.gameObject.SetActive(true);
                indicator.color = Color.black;
                ChangeIndicatorWidth(8);
                break;
            case AppState.Closed:
                // Hide the indicator when the app is closed
                indicator.gameObject.SetActive(false);
                break;
        }
    }

    // Call this method to change the width of the indicator
    public void ChangeIndicatorWidth(float newWidth)
    {
        // Ensure Image component reference is not null
        if (indicator != null)
        {
            // Get the RectTransform component of the indicator
            RectTransform rectTransform = indicator.rectTransform;

            // Update the width using sizeDelta
            rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
        }
        else
        {
            Debug.LogWarning("Image component reference is null.");
        }
    }

    // Call this method to set the color of the indicator using a hexadecimal string
    public void SetIndicatorColorFromHex(string hex)
    {
        if (indicator != null)
        {
            Color newColor;
            if (ColorUtility.TryParseHtmlString(hex, out newColor))
            {
                indicator.color = newColor;
            }
            else
            {
                Debug.LogWarning($"Failed to parse color hex code: {hex}");
            }
        }
    }

    // Check if the app is currently minimized
    public bool IsMinimized()
    {
        return currentState == AppState.Minimized;
    }
}
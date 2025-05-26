using  UnityEngine;
using UnityEngine.UI;

public class WindowMaximizer : MonoBehaviour
{
    [SerializeField] private Button maximizeButton;
    
    private RectTransform windowRectTransform;
    private bool isMaximized = false;
    
    // Store original window properties
    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalSizeDelta;
    private Vector2 originalAnchoredPosition;
    private Vector3 originalScale;

    // Public property to check window state
    public bool IsMaximized => isMaximized;

    private void Awake()
    {
        windowRectTransform = GetComponent<RectTransform>();
        
        // Store original window properties
        StoreOriginalProperties();
        
        // Setup maximize button click listener
        if (maximizeButton != null)
        {
            maximizeButton.onClick.AddListener(ToggleMaximize);
        }
        else
        {
            Debug.LogWarning("Maximize button not assigned to WindowMaximizer on " + gameObject.name);
        }
    }

    private void StoreOriginalProperties()
    {
        originalAnchorMin = windowRectTransform.anchorMin;
        originalAnchorMax = windowRectTransform.anchorMax;
        originalSizeDelta = windowRectTransform.sizeDelta;
        originalAnchoredPosition = windowRectTransform.anchoredPosition;
        originalScale = windowRectTransform.localScale;
    }

    public void ToggleMaximize()
    {
        if (isMaximized)
        {
            RestoreWindow();
        }
        else
        {
            MaximizeWindow();
        }
        
        isMaximized = !isMaximized;
    }

    // New method to set window state without toggling
    public void SetWindowState(bool maximized)
    {
        if (maximized && !isMaximized)
        {
            MaximizeWindow();
            isMaximized = true;
        }
        else if (!maximized && isMaximized)
        {
            RestoreWindow();
            isMaximized = false;
        }
    }

    private void MaximizeWindow()
    {
        // Set anchors to stretch across the entire parent
        windowRectTransform.anchorMin = Vector2.zero;
        windowRectTransform.anchorMax = Vector2.one;
        
        // Reset position and size
        windowRectTransform.anchoredPosition = Vector2.zero;
        windowRectTransform.sizeDelta = Vector2.zero;
        
        // Ensure scale is 1
        windowRectTransform.localScale = Vector3.one;
    }

    private void RestoreWindow()
    {
        // Restore original properties
        windowRectTransform.anchorMin = originalAnchorMin;
        windowRectTransform.anchorMax = originalAnchorMax;
        windowRectTransform.sizeDelta = originalSizeDelta;
        windowRectTransform.anchoredPosition = originalAnchoredPosition;
        windowRectTransform.localScale = originalScale;
    }
} 

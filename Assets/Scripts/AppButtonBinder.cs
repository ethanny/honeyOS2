using UnityEngine;
using UnityEngine.UI;

public class AppButtonBinder : MonoBehaviour
{
    public Button closeButton;
    public Button minimizeButton;
    public App appToControl;

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(appToControl.OnCloseButtonPressed);

        if (minimizeButton != null)
            minimizeButton.onClick.AddListener(appToControl.OnMinimizeButtonPressed);
    }
}

using System;
using UnityEngine;

public class PageReplacementPopUpManager : MonoBehaviour
{
    [SerializeField] GameObject invalidPageReference;

    public static PageReplacementPopUpManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void ShowPopUp(String PopUp)
    {
        switch (PopUp)
        {
            case "InvalidPageReference":
                invalidPageReference.GetComponent<PopupController>().Show();
                break;
            default:
                Debug.LogWarning("Error: Unknown popup type in PageReplacementPopUpManager");
                break;
        }
    }

    public void ClosePopUp(String PopUp)
    {
        switch(PopUp)
        {
            case "InvalidPageReference":
                invalidPageReference.GetComponent<PopupController>().Hide();
                break;
            default:
                Debug.LogWarning("Error: Unknown popup type in PageReplacementPopUpManager");
                break;
        }
    }

    public GameObject InvalidPageReference {
        get => invalidPageReference;
    }
} 
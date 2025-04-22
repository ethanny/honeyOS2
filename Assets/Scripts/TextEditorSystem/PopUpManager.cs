using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpManager : MonoBehaviour
{
    [SerializeField] GameObject unsavedChangesOpenFile;
    [SerializeField] GameObject unsavedChangesNewFile;
    [SerializeField] GameObject unsavedChangesCloseApp;

    public static PopUpManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void ShowPopUp(String PopUp)
    {
        switch (PopUp)
        {
            case "UnsavedChangesOpenFile":
                unsavedChangesOpenFile.GetComponent<PopupController>().Show();
                break;
            case "UnsavedChangesNewFile":
                unsavedChangesNewFile.GetComponent<PopupController>().Show();
                break;
            case "UnsavedChangesCloseApp":
                Debug.Log("SHOWPOPUP CLOSEAPP");
                unsavedChangesCloseApp.GetComponent<PopupController>().Show();
                break;
            default:
                Debug.LogWarning("Error");
                break;
        }
    }

    public void ClosePopUp(String PopUp)
    {
        switch(PopUp)
        {
            case "UnsavedChangesOpenFile":
                unsavedChangesOpenFile.GetComponent<PopupController>().Hide();
                break;
            case "UnsavedChangesNewFile":
                unsavedChangesNewFile.GetComponent<PopupController>().Hide();
                break;
            case "UnsavedChangesCloseApp":
                unsavedChangesCloseApp.GetComponent<PopupController>().Hide();
                break;
            default:
                Debug.LogWarning("Error");
                break;
        }
    }

    public GameObject UnsavedChangesOpenFile {
        get => unsavedChangesOpenFile;
    }

    public GameObject UnsavedChangesNewFile {
        get => unsavedChangesNewFile;
    }

    public GameObject UnsavedChangesCloseApp {
        get => unsavedChangesCloseApp;
    }

}

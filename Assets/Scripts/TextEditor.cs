using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Define the TextEditor class that extends the App class
public class TextEditor : App
{
    [SerializeField] TextEditorController textEditorController;
    public TextMeshProUGUI textMeshPro;

    // Override the ResetApp method to reset FileManager to default state
    protected override void Reset()
    {   
        //textMeshPro.text = "I AM IN TEXT EDITOR.";
        // Implement FileManager-specific reset behavior here
        textEditorController.NewFile();
    }
    public override void Close()
    {
        // PopUpManager.Instance.ClosePopUp("UnsavedChangesCloseApp");
        if (PopUpManager.Instance.UnsavedChangesCloseApp.activeInHierarchy)
            PopUpManager.Instance.UnsavedChangesCloseApp.GetComponent<PopupController>().Hide();
        base.Close(); 
        // if (!textEditorController.SaveCancelled)
            DesktopManager.Instance.CurrentAppInstance = null;
    }
    public void CheckClose()
    {
        Debug.Log("iN OVERRIdE CLOSE");
        Debug.Log("ButtonManager.Instance.SaveButton.interactable): " + ButtonManager.Instance.SaveButton.interactable);
        if (ButtonManager.Instance.SaveButton.interactable)
        {
            Debug.Log("in if LESGOOOO");
            PopUpManager.Instance.ShowPopUp("UnsavedChangesCloseApp");

        }
        else
        {
            Debug.Log("in else wth");
            if (PopUpManager.Instance.UnsavedChangesCloseApp.activeInHierarchy)
                PopUpManager.Instance.UnsavedChangesCloseApp.GetComponent<PopupController>().Hide();
            base.Close();
            // if (!textEditorController.SaveCancelled)
                DesktopManager.Instance.CurrentAppInstance = null;
        }
    }

    public void SmartClose()
    {
        if (!textEditorController.SaveCancelled) 
        {
            if (PopUpManager.Instance.UnsavedChangesCloseApp.activeInHierarchy)
                PopUpManager.Instance.UnsavedChangesCloseApp.GetComponent<PopupController>().Hide();
            base.Close();
            DesktopManager.Instance.CurrentAppInstance = null;
        }
    }

    public void ChangeText()
    {
        // Assign the new text to the TextMeshPro component
        //textMeshPro.text = "I AM IN TEXT EDITOR. I MODIFIED SHITZ.";
    }
    
    // Add TextEditor-specific functionality here if needed
    
    // Called when the script instance is being loaded
    // private void Start()
    // {
    //     // Add a listener to the OnValueChanged event of the Input Field
    //     // inputField.onValueChanged.AddListener(OnTextInput);
    // }

    // Function to handle text input
    public void OnTextInput(string text)
    {
        // Handle text input here
        Debug.Log("Text Input: " + text);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Define the TextEditor class that extends the App class
public class TextEditor : App
{
    [SerializeField] private TextEditorController textEditorController;
    [SerializeField] private ButtonManager buttonManager;
    public TextMeshProUGUI textMeshPro;

    protected override void Reset()
    {
        textEditorController.NewFile();
    }

    public override void Close()
    {
        if (PopUpManager.Instance.UnsavedChangesCloseApp.activeInHierarchy)
            PopUpManager.Instance.UnsavedChangesCloseApp.GetComponent<PopupController>().Hide();

        base.Close();
    }

    public void CheckClose()
    {
        Debug.Log("In CheckClose()");
        Debug.Log("SaveButton.interactable: " + buttonManager.SaveButton.interactable);

        if (buttonManager.SaveButton.interactable)
        {
            PopUpManager.Instance.ShowPopUp("UnsavedChangesCloseApp");
        }
        else
        {
            if (PopUpManager.Instance.UnsavedChangesCloseApp.activeInHierarchy)
                PopUpManager.Instance.UnsavedChangesCloseApp.GetComponent<PopupController>().Hide();

            base.Close();
        }
    }

    public void SmartClose()
    {
        if (!textEditorController.SaveCancelled)
        {
            if (PopUpManager.Instance.UnsavedChangesCloseApp.activeInHierarchy)
                PopUpManager.Instance.UnsavedChangesCloseApp.GetComponent<PopupController>().Hide();

            base.Close();
        }
    }

    public void ChangeText()
    {
        // Optional custom logic when text is changed
    }

    public void OnTextInput(string text)
    {
        Debug.Log("Text Input: " + text);
    }
}

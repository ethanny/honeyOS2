using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;
using SFB;
using UnityEngine.Networking;

// BUG ALERT!
// 1. On start of text editor, when clicking filename, text are not highlighted. This is because of placeholder in inputfield.
// 2. When opening file, since value (textfield and/or filename) has changed, SaveButton.interactable is set to true. Should be false. [SOLVED]
// 3. On popup: When user clicks save (specific to SaveAs()) which opens Save Panel and exits panel w/o saving, Open File Panel opens. 
//    Open File Panel should not open when saving is cancelled. [SOLVED]
// 4. Continuation of #3. When cancelling Open File Panel and cancelling Unsaved Changes Pop Up, the next time user clicks Open File
//    Button, the Open File Panel opens without signifying unsaved changes. [SOLVED]

// POTENTIAL ISSUES
// 1. Shortcut checking for unsaved changes by checking if SaveButton is interactable.

public class TextEditorController : MonoBehaviour
{

    [SerializeField] TMP_InputField FileName;
    [SerializeField] TMP_InputField TextField;
    private string currentFilePath;
    public bool SaveCancelled = false;
    private bool isCut = false;

    public FiniteStack<string> undoStack;
    public FiniteStack<string> redoStack;
    // public FiniteStack<string> popRedo;



    void Start()
    {
        // File name
        FileName.onEndEdit.AddListener(OnEndEditFileName);
        TextField.onValueChanged.AddListener(OnValueChangedTextField);
        undoStack = new FiniteStack<string>();
        redoStack = new FiniteStack<string>();
        // popRedo = new FiniteStack<string>();

        NewFile();
    }

    void Update()
    {
        CheckKeyboardShortcuts();
    }

    private void CheckKeyboardShortcuts()
    {
        // Determine the modifier key based on the operating system
        KeyCode modifierKey = (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) ? KeyCode.LeftCommand : KeyCode.LeftControl;
        KeyCode modifierKeyRight = (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) ? KeyCode.RightCommand : KeyCode.RightControl;

        // Check if the modifier key (either Left or Right) is being pressed
        if (Input.GetKey(modifierKey) || Input.GetKey(modifierKeyRight))
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    SaveAs();
                }
            }
            if (Input.GetKeyDown(KeyCode.S)) Save();
            else if (Input.GetKeyDown(KeyCode.N)) NewFile();
            else if (Input.GetKeyDown(KeyCode.O)) OpenFile();
            else if (Input.GetKeyDown(KeyCode.Z)) Undo();
            else if (Input.GetKeyDown(KeyCode.Y)) Redo();
            else if (Input.GetKeyDown(KeyCode.C)) Copy();
            else if (Input.GetKeyDown(KeyCode.V)) Paste();
            else if (Input.GetKeyDown(KeyCode.X)) Cut();
        }
    }


    public void Save()
    {
        if (!string.IsNullOrEmpty(currentFilePath))
        {
            File.WriteAllText(currentFilePath, TextField.text);
            ClosePopUp(); // FAULTY. REASON FOR CLOSING WHEN EXITING OPENFILE PANEL AFTER SAVE() ON POPUP
            ButtonManager.Instance.SaveButton.GetComponent<ButtonController>().SetInteractable(false);
        }
        else
            SaveAs();
    }

    public void SaveAs()
    {
        // Open Save File Panel
        string path = StandaloneFileBrowser.SaveFilePanel("Save File", "", FileName.text, "bby");
        Debug.Log("SaveAs() path after cancelling: " + path);
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, TextField.text);
            currentFilePath = path;
            FileName.text = Path.GetFileName(path);
            OnEndEditFileName(FileName.text);
            ClosePopUp();
            ButtonManager.Instance.SaveButton.GetComponent<ButtonController>().SetInteractable(false);
            SaveCancelled = false;
        }
        else
        {
            SaveCancelled = true;
        }
    }

    public void OpenFile()
    {
        // Open Open File Panel
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "bby", false);
        if (paths.Length > 0)
        {
            Debug.Log("Passed even cancel");
            StartCoroutine(OutputRoutineOpen(new System.Uri(paths[0]).AbsoluteUri));
            currentFilePath = paths[0];
            FileName.text = Path.GetFileName(paths[0]);
            OnEndEditFileName(FileName.text);
            ClosePopUp();
            ButtonManager.Instance.SaveButton.GetComponent<ButtonController>().SetInteractable(false);
            ButtonManager.Instance.UndoButton.GetComponent<ButtonController>().SetInteractable(false);
            ButtonManager.Instance.RedoButton.GetComponent<ButtonController>().SetInteractable(false);
            SaveCancelled = false;
        }
    }

    public void CheckOpenFile()
    {
        if (ButtonManager.Instance.SaveButton.interactable)
            PopUpManager.Instance.ShowPopUp("UnsavedChangesOpenFile");
        else
            OpenFile();
    }

    public void SmartOpenFile()
    {
        if (!SaveCancelled)
            OpenFile();
    }

    public void NewFile()
    {
        currentFilePath = null;
        FileName.text = "";
        TextField.text = "";
        ClosePopUp();
        // Set Save Button not interactable when first loading up text since no changes have been made
        ButtonManager.Instance.SaveButton.GetComponent<ButtonController>().SetInteractable(false);
        ButtonManager.Instance.UndoButton.GetComponent<ButtonController>().SetInteractable(false);
        ButtonManager.Instance.RedoButton.GetComponent<ButtonController>().SetInteractable(false);
        SaveCancelled = false;
    }
    public void CheckNewFile()
    {
        if (ButtonManager.Instance.SaveButton.interactable)
            PopUpManager.Instance.ShowPopUp("UnsavedChangesNewFile");
        else
            NewFile();
    }

    public void SmartNewFile()
    {
        if (!SaveCancelled)
            NewFile();
    }


    private IEnumerator OutputRoutineOpen(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log("WWW ERROR: " + www.error);
        else
        {
            TextField.text = www.downloadHandler.text;
            ButtonManager.Instance.SaveButton.GetComponent<ButtonController>().SetInteractable(false);
            ButtonManager.Instance.UndoButton.GetComponent<ButtonController>().SetInteractable(false);
            ButtonManager.Instance.RedoButton.GetComponent<ButtonController>().SetInteractable(false);
        }
    }

    void OnEndEditFileName(string newFileName)
    {
        FileName.text = newFileName;
        if (!FileName.text.EndsWith(".bby"))
            FileName.text += ".bby";
    }

    void OnValueChangedTextField(string newContent)
    {
        ButtonManager.Instance.SaveButton.GetComponent<ButtonController>().SetInteractable(true);
        ButtonManager.Instance.UndoButton.GetComponent<ButtonController>().SetInteractable(true);
        if (!string.IsNullOrEmpty(TextField.text) && TextField.text != undoStack.Peek())
        {
            undoStack.Push(TextField.text);
            // if (popRedo.Peek() != TextField.text){
            //     redoStack.Clear();
            // }
        }
    }

    public void ClosePopUp()
    {
        if (PopUpManager.Instance.UnsavedChangesOpenFile.activeInHierarchy)
            PopUpManager.Instance.UnsavedChangesOpenFile.GetComponent<PopupController>().Hide();
        if (PopUpManager.Instance.UnsavedChangesNewFile.activeInHierarchy)
            PopUpManager.Instance.UnsavedChangesNewFile.GetComponent<PopupController>().Hide();
        // if (PopUpManager.Instance.UnsavedChangesCloseApp.activeInHierarchy)
        //     PopUpManager.Instance.UnsavedChangesCloseApp.GetComponent<PopupController>().Hide();
    }

    public void Undo()
    {
        if (undoStack.Count > 0)
        {
            if (!isCut)
            {
                undoStack.Pop();
            }
            else
            {
                isCut = false;
            }
            redoStack.Push(TextField.text);
            if (undoStack.Peek() != null)
            {
                TextField.text = undoStack.Peek();
            }
            else
            {
                TextField.text = "";
            }
            if (undoStack.Count == 0)
            {
                ButtonManager.Instance.UndoButton.GetComponent<ButtonController>().SetInteractable(false);
            }
            ButtonManager.Instance.RedoButton.GetComponent<ButtonController>().SetInteractable(true);
        }
    }

    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            TextField.text = redoStack.Peek();
            // popRedo.Push(redoStack.Peek());
            redoStack.Pop();
            if (redoStack.Count == 0)
            {
                ButtonManager.Instance.RedoButton.GetComponent<ButtonController>().SetInteractable(false);
            }
        }
    }

    public void ClearUndoRedoStacks()
    {
        undoStack.Clear();
        redoStack.Clear();
    }

    public void Copy()
    {
        int startIndex = Mathf.Min(TextField.selectionAnchorPosition, TextField.selectionFocusPosition);
        int endIndex = Mathf.Max(TextField.selectionAnchorPosition, TextField.selectionFocusPosition);

        // Check if there is any text selected
        if (startIndex >= 0 && endIndex <= TextField.text.Length)
        {
            // Get the selected text
            string selectedText = TextField.text.Substring(startIndex, endIndex - startIndex);

            if (selectedText != "")
            {
                // Copy the selected text to the system clipboard
                GUIUtility.systemCopyBuffer = selectedText;
                ButtonManager.Instance.PasteButton.GetComponent<ButtonController>().SetInteractable(true);
            }
            else
            {
                Debug.Log("No text is selected.");
            }
        }
        else
        {
            // If no text is selected or indices are invalid, you can optionally handle this case (e.g., display a message)
            Debug.Log("No text is selected or indices are invalid.");
        }
    }

    public void Cut()
    {
        if (TextField.selectionAnchorPosition != TextField.selectionFocusPosition)
        {
            int startIndex = Mathf.Min(TextField.selectionAnchorPosition, TextField.selectionFocusPosition);
            int endIndex = Mathf.Max(TextField.selectionAnchorPosition, TextField.selectionFocusPosition);

            // Ensure that both indices are within the valid range of the text length
            if (startIndex >= 0 && endIndex <= TextField.text.Length)
            {
                // Get the selected text
                string selectedText = TextField.text.Substring(startIndex, endIndex - startIndex);

                if (selectedText != "")
                {
                    // Copy the selected text to the system clipboard
                    GUIUtility.systemCopyBuffer = selectedText;
                    // Remove the selected text from the input field
                    TextField.text = TextField.text.Remove(startIndex, endIndex - startIndex);

                    // Update the selection to indicate no text is selected
                    TextField.selectionAnchorPosition = startIndex;
                    TextField.selectionFocusPosition = startIndex;
                    isCut = true;
                    ButtonManager.Instance.PasteButton.GetComponent<ButtonController>().SetInteractable(true);
                }
                else
                {
                    Debug.Log("No text is selected.");
                }
            }
        }
        else
        {
            // If no text is selected, you can optionally handle this case (e.g., display a message)
            Debug.Log("No text is selected.");
        }
    }

    public void Paste()
    {
        string clipboardText = GUIUtility.systemCopyBuffer;
        int caretPosition = TextField.caretPosition;

        // Get the text before and after the caret position
        string textBeforeCaret = TextField.text.Substring(0, caretPosition);
        string textAfterCaret = "";

        // Check if caretPosition is at the end of the text
        if (caretPosition < TextField.text.Length)
        {
            textAfterCaret = TextField.text.Substring(caretPosition);
        }

        // Concatenate the clipboard text with the text after the caret position
        string newText = textBeforeCaret + clipboardText + textAfterCaret;

        // Set the new text to the input field
        TextField.text = newText;

        // Move the caret position to the end of the pasted text
        TextField.caretPosition = caretPosition + clipboardText.Length;
    }

    public void BoldSelectedText()
    {
        // Check if there is any text selected
        if (TextField.selectionAnchorPosition != TextField.selectionFocusPosition)
        {
            int startIndex = Mathf.Min(TextField.selectionAnchorPosition, TextField.selectionFocusPosition);
            int endIndex = Mathf.Max(TextField.selectionAnchorPosition, TextField.selectionFocusPosition);

            // Ensure that both indices are within the valid range of the text length
            if (startIndex >= 0 && endIndex <= TextField.text.Length)
            {
                // Get the selected text
                string selectedText = TextField.text.Substring(startIndex, endIndex - startIndex);

                if (selectedText != "")
                {
                    // Create bolded version with TMPro rich text tags
                    string boldText = "<b>" + selectedText + "</b>";

                    // Get the text before and after the selection
                    string textBeforeSelection = TextField.text.Substring(0, startIndex);
                    string textAfterSelection = TextField.text.Substring(endIndex);

                    // Create the new text with the bold formatting
                    string newText = textBeforeSelection + boldText + textAfterSelection;

                    // Update the text field
                    TextField.text = newText;

                    // Update undo stack
                    undoStack.Push(TextField.text);

                    // Update the caret position to be after the bolded text
                    TextField.caretPosition = startIndex + boldText.Length;

                    // Mark document as changed
                    ButtonManager.Instance.SaveButton.GetComponent<ButtonController>().SetInteractable(true);
                }
                else
                {
                    Debug.Log("No text is selected.");
                }
            }
        }
        else
        {
            Debug.Log("No text is selected.");
        }
    }

}

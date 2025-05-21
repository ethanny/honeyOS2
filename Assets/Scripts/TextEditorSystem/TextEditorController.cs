using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;
using SFB;
using UnityEngine.Networking;

public class TextEditorController : MonoBehaviour
{
    [SerializeField] private ButtonManager buttonManager;
    [SerializeField] TMP_InputField FileName;
    [SerializeField] TMP_InputField TextField;
    private string currentFilePath;
    public bool SaveCancelled = false;
    private bool isCut = false;

    public FiniteStack<string> undoStack;
    public FiniteStack<string> redoStack;

    void Start()
    {
        FileName.onEndEdit.AddListener(OnEndEditFileName);
        TextField.onValueChanged.AddListener(OnValueChangedTextField);
        undoStack = new FiniteStack<string>();
        redoStack = new FiniteStack<string>();
        NewFile();
    }

    void Update()
    {
        CheckKeyboardShortcuts();
    }

    private void CheckKeyboardShortcuts()
    {
        KeyCode modifierKey = (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) ? KeyCode.LeftCommand : KeyCode.LeftControl;
        KeyCode modifierKeyRight = (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) ? KeyCode.RightCommand : KeyCode.RightControl;

        if (Input.GetKey(modifierKey) || Input.GetKey(modifierKeyRight))
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                if (Input.GetKeyDown(KeyCode.S))
                    SaveAs();
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
            ClosePopUp();
            buttonManager.SaveButton.GetComponent<ButtonController>().SetInteractable(false);
        }
        else
        {
            SaveAs();
        }
    }

    public void SaveAs()
    {
        string path = StandaloneFileBrowser.SaveFilePanel("Save File", "", FileName.text, "bby");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, TextField.text);
            currentFilePath = path;
            FileName.text = Path.GetFileName(path);
            OnEndEditFileName(FileName.text);
            ClosePopUp();
            buttonManager.SaveButton.GetComponent<ButtonController>().SetInteractable(false);
            SaveCancelled = false;
        }
        else
        {
            SaveCancelled = true;
        }
    }

    public void OpenFile()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "bby", false);
        if (paths.Length > 0)
        {
            StartCoroutine(OutputRoutineOpen(new System.Uri(paths[0]).AbsoluteUri));
            currentFilePath = paths[0];
            FileName.text = Path.GetFileName(paths[0]);
            OnEndEditFileName(FileName.text);
            ClosePopUp();
            buttonManager.SaveButton.GetComponent<ButtonController>().SetInteractable(false);
            buttonManager.UndoButton.GetComponent<ButtonController>().SetInteractable(false);
            buttonManager.RedoButton.GetComponent<ButtonController>().SetInteractable(false);
            SaveCancelled = false;
        }
    }

    public void CheckOpenFile()
    {
        if (buttonManager.SaveButton.interactable)
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
        buttonManager.SaveButton.GetComponent<ButtonController>().SetInteractable(false);
        buttonManager.UndoButton.GetComponent<ButtonController>().SetInteractable(false);
        buttonManager.RedoButton.GetComponent<ButtonController>().SetInteractable(false);
        SaveCancelled = false;
    }

    public void CheckNewFile()
    {
        if (buttonManager.SaveButton.interactable)
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
        {
            Debug.Log("WWW ERROR: " + www.error);
        }
        else
        {
            TextField.text = www.downloadHandler.text;
            buttonManager.SaveButton.GetComponent<ButtonController>().SetInteractable(false);
            buttonManager.UndoButton.GetComponent<ButtonController>().SetInteractable(false);
            buttonManager.RedoButton.GetComponent<ButtonController>().SetInteractable(false);
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
        buttonManager.SaveButton.GetComponent<ButtonController>().SetInteractable(true);
        buttonManager.UndoButton.GetComponent<ButtonController>().SetInteractable(true);
        if (!string.IsNullOrEmpty(TextField.text) && TextField.text != undoStack.Peek())
        {
            undoStack.Push(TextField.text);
        }
    }

    public void ClosePopUp()
    {
        if (PopUpManager.Instance.UnsavedChangesOpenFile.activeInHierarchy)
            PopUpManager.Instance.UnsavedChangesOpenFile.GetComponent<PopupController>().Hide();
        if (PopUpManager.Instance.UnsavedChangesNewFile.activeInHierarchy)
            PopUpManager.Instance.UnsavedChangesNewFile.GetComponent<PopupController>().Hide();
    }

    public void Undo()
    {
        if (undoStack.Count > 0)
        {
            if (!isCut) undoStack.Pop();
            else isCut = false;

            redoStack.Push(TextField.text);
            TextField.text = undoStack.Peek() ?? "";

            if (undoStack.Count == 0)
                buttonManager.UndoButton.GetComponent<ButtonController>().SetInteractable(false);

            buttonManager.RedoButton.GetComponent<ButtonController>().SetInteractable(true);
        }
    }

    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            TextField.text = redoStack.Peek();
            redoStack.Pop();

            if (redoStack.Count == 0)
                buttonManager.RedoButton.GetComponent<ButtonController>().SetInteractable(false);
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

        if (startIndex >= 0 && endIndex <= TextField.text.Length)
        {
            string selectedText = TextField.text.Substring(startIndex, endIndex - startIndex);
            if (selectedText != "")
            {
                GUIUtility.systemCopyBuffer = selectedText;
                buttonManager.PasteButton.GetComponent<ButtonController>().SetInteractable(true);
            }
        }
    }

    public void Cut()
    {
        if (TextField.selectionAnchorPosition != TextField.selectionFocusPosition)
        {
            int startIndex = Mathf.Min(TextField.selectionAnchorPosition, TextField.selectionFocusPosition);
            int endIndex = Mathf.Max(TextField.selectionAnchorPosition, TextField.selectionFocusPosition);

            if (startIndex >= 0 && endIndex <= TextField.text.Length)
            {
                string selectedText = TextField.text.Substring(startIndex, endIndex - startIndex);
                if (selectedText != "")
                {
                    GUIUtility.systemCopyBuffer = selectedText;
                    TextField.text = TextField.text.Remove(startIndex, endIndex - startIndex);
                    TextField.selectionAnchorPosition = startIndex;
                    TextField.selectionFocusPosition = startIndex;
                    isCut = true;
                    buttonManager.PasteButton.GetComponent<ButtonController>().SetInteractable(true);
                }
            }
        }
    }

    public void Paste()
    {
        string clipboardText = GUIUtility.systemCopyBuffer;
        int caretPosition = TextField.caretPosition;
        string textBeforeCaret = TextField.text.Substring(0, caretPosition);
        string textAfterCaret = (caretPosition < TextField.text.Length) ? TextField.text.Substring(caretPosition) : "";
        string newText = textBeforeCaret + clipboardText + textAfterCaret;

        TextField.text = newText;
        TextField.caretPosition = caretPosition + clipboardText.Length;
    }

    public void BoldSelectedText()
    {
        if (TextField.selectionAnchorPosition != TextField.selectionFocusPosition)
        {
            int startIndex = Mathf.Min(TextField.selectionAnchorPosition, TextField.selectionFocusPosition);
            int endIndex = Mathf.Max(TextField.selectionAnchorPosition, TextField.selectionFocusPosition);

            if (startIndex >= 0 && endIndex <= TextField.text.Length)
            {
                string selectedText = TextField.text.Substring(startIndex, endIndex - startIndex);
                if (selectedText != "")
                {
                    string boldText = "<b>" + selectedText + "</b>";
                    string textBeforeSelection = TextField.text.Substring(0, startIndex);
                    string textAfterSelection = TextField.text.Substring(endIndex);
                    string newText = textBeforeSelection + boldText + textAfterSelection;
                    TextField.text = newText;
                    undoStack.Push(TextField.text);
                    TextField.caretPosition = startIndex + boldText.Length;
                    buttonManager.SaveButton.GetComponent<ButtonController>().SetInteractable(true);
                }
            }
        }
    }
}

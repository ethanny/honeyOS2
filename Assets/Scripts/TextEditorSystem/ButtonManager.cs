using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] private Button saveButton;
    [SerializeField] private Button saveAsButton;
    [SerializeField] private Button openFileButton;
    [SerializeField] private Button newFileButton;

    [SerializeField] private Button undoButton;
    [SerializeField] private Button redoButton;

    [SerializeField] private Button copyButton;
    [SerializeField] private Button cutButton;
    [SerializeField] private Button pasteButton; 

    [SerializeField] private Button closeButton;
    [SerializeField] private Button minimizeButton;

    [SerializeField] private App appToControl; // <-- Assigned in Inspector

    private void Start()
    {
        if (closeButton != null && appToControl != null)
            closeButton.onClick.AddListener(appToControl.OnCloseButtonPressed);

        if (minimizeButton != null && appToControl != null)
            minimizeButton.onClick.AddListener(appToControl.OnMinimizeButtonPressed);

        if (closeButton != null && appToControl != null)
    {
        closeButton.onClick.RemoveAllListeners(); // Just to be safe
        closeButton.onClick.AddListener(appToControl.OnCloseButtonPressed);
    }

    if (minimizeButton != null && appToControl != null)
    {
        minimizeButton.onClick.RemoveAllListeners();
        minimizeButton.onClick.AddListener(appToControl.OnMinimizeButtonPressed);
    }
    }

    public Button SaveButton => saveButton;
    public Button SaveAsButton => saveAsButton;
    public Button OpenFileButton => openFileButton;
    public Button NewFileButton => newFileButton;
    public Button UndoButton => undoButton;
    public Button RedoButton => redoButton;
    public Button CopyButton => copyButton;
    public Button CutButton => cutButton;
    public Button PasteButton => pasteButton;
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] Button saveButton;
    [SerializeField] Button saveAsButton;
    [SerializeField] Button openFileButton;
    [SerializeField] Button newFileButton;

    [SerializeField] Button undoButton;
    [SerializeField] Button redoButton;

    [SerializeField] Button copyButton;
    [SerializeField] Button cutButton;
    [SerializeField] Button pasteButton; 

    public static ButtonManager Instance { get; private set;}

    private void Awake()
    {
        Instance = this;
    }

    public Button SaveButton {
        get => saveButton;
    }
    public Button SaveAsButton {
        get => saveAsButton;
    }
    public Button OpenFileButton {
        get => openFileButton;
    }
    public Button NewFileButton {
        get => newFileButton;
    }

    public Button UndoButton {
        get => undoButton;
    }
    public Button RedoButton {
        get => redoButton;
    }
    public Button CopyButton {
        get => copyButton;
    }
    public Button CutButton {
        get => copyButton;
    }
    public Button PasteButton {
        get => copyButton;
    }
}

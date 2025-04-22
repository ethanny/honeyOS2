using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    private Button button;
    private TMP_Text text;
    private SpriteRenderer icon;
    Color newColor;

    void Awake()
    {
        button = GetComponent<Button>();
        text = button.GetComponentInChildren<TMP_Text>();
        icon = button.GetComponentInChildren<SpriteRenderer>();
        newColor = text.color;
        
    }
    
    public void Update()
    {
        
    }

    public void SetInteractable(bool state)
    {
        button.interactable = state;
        if (!button.interactable)
        {
            newColor.a = 175 / 255f;
            text.color = newColor;
            icon.color = newColor;
        }
        else if (button.interactable)
        {
            newColor.a = 255 / 255f;
            text.color = newColor;
            icon.color= newColor;
        }

    }
}

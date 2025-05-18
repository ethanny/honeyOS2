using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AlgorithmManager : MonoBehaviour
{
    private string selectedAlgorithm;
    
    public TMP_Text algorithmTitleText;
    
    public void SetAlgorithm(string algorithm)
    {
        selectedAlgorithm = algorithm;
        
        switch (algorithm)
        {
            case "FIFO":
                algorithmTitleText.text = "First In, First Out";
                break;
            case "OPR":
                algorithmTitleText.text = "Optimal Page Replacement";
                break;
            case "LRU":
                algorithmTitleText.text = "Least Recently Used";
                break;
            case "MRU":
                algorithmTitleText.text = "Most Recently Used";
                break;
            default:
                algorithmTitleText.text = "Unknown Algorithm";
                break;
        }
    }
} 
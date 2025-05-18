using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AlgorithmManager : MonoBehaviour
{
    private string selectedAlgorithm;
    
    public TMP_Text algorithmTitleText;
    public TMP_InputField referenceStringInput;
    public Slider frameCountSlider;
    public TMP_Text frameCountText;
    
    private List<int> referenceString = new List<int>();
    private int frameCount = 3; // Default frame count
    
    // UI elements for reference string display
    public GameObject referenceBoxPrefab;
    public Transform referenceBoxContainer;
    private List<GameObject> referenceBoxes = new List<GameObject>();
    
    // UI elements for simulation grid
    public GameObject simulationCellPrefab;
    public Transform simulationGridContainer;
    private List<List<GameObject>> simulationGrid = new List<List<GameObject>>();
    
    // Color settings
    private Color defaultColor = Color.white;
    private Color accessedColor = Color.blue;
    private Color pageFaultColor = Color.red;
    private Color pageHitColor = Color.green;
    
    private void Start()
    {
        // Initialize frame count slider
        if (frameCountSlider != null)
        {
            frameCountSlider.minValue = 1;
            frameCountSlider.maxValue = 7;
            frameCountSlider.value = frameCount;
            frameCountSlider.onValueChanged.AddListener(UpdateFrameCount);
            UpdateFrameCountText();
        }
    }
    
    public void UpdateFrameCount(float value)
    {
        frameCount = Mathf.RoundToInt(value);
        UpdateFrameCountText();
    }
    
    private void UpdateFrameCountText()
    {
        if (frameCountText != null)
        {
            frameCountText.text = "Frames: " + frameCount;
        }
    }
    
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
    
    public void ProcessReferenceString()
    {
        referenceString.Clear();
        
        string input = referenceStringInput.text;
        string[] values = input.Split(' ');
        
        foreach (string value in values)
        {
            if (int.TryParse(value, out int pageNumber))
            {
                referenceString.Add(pageNumber);
            }
        }
        
        Debug.Log("Reference string processed: " + string.Join(", ", referenceString));
        DisplayReferenceString();
        SetupSimulationGrid();
    }
    
    private void DisplayReferenceString()
    {
        // Clear existing boxes
        ClearReferenceBoxes();
        
        // Create new boxes for each page reference
        foreach (int pageNumber in referenceString)
        {
            GameObject box = Instantiate(referenceBoxPrefab, referenceBoxContainer);
            TMP_Text textComponent = box.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = pageNumber.ToString();
            }
            
            // Set default color
            Image boxImage = box.GetComponent<Image>();
            if (boxImage != null)
            {
                boxImage.color = defaultColor;
            }
            
            referenceBoxes.Add(box);
        }
    }
    
    private void SetupSimulationGrid()
    {
        // Clear existing grid
        ClearSimulationGrid();
        
        // Configure grid layout if using Grid Layout Group
        GridLayoutGroup gridLayout = simulationGridContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.constraintCount = referenceString.Count;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        }
        
        // Create grid with frameCount rows and referenceString.Count columns
        for (int i = 0; i < frameCount; i++)
        {
            List<GameObject> row = new List<GameObject>();
            
            for (int j = 0; j < referenceString.Count; j++)
            {
                GameObject cell = Instantiate(simulationCellPrefab, simulationGridContainer);
                
                // Set empty text initially
                TMP_Text textComponent = cell.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = "";
                }
                
                // Set default color
                Image cellImage = cell.GetComponent<Image>();
                if (cellImage != null)
                {
                    cellImage.color = defaultColor;
                }
                
                row.Add(cell);
            }
            
            simulationGrid.Add(row);
        }
        
        // Add fault/hit indicator row
        List<GameObject> indicatorRow = new List<GameObject>();
        for (int j = 0; j < referenceString.Count; j++)
        {
            GameObject cell = Instantiate(simulationCellPrefab, simulationGridContainer);
            
            TMP_Text textComponent = cell.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = "";
            }
            
            indicatorRow.Add(cell);
        }
        simulationGrid.Add(indicatorRow);
    }
    
    private void ClearReferenceBoxes()
    {
        foreach (GameObject box in referenceBoxes)
        {
            Destroy(box);
        }
        referenceBoxes.Clear();
    }
    
    private void ClearSimulationGrid()
    {
        foreach (List<GameObject> row in simulationGrid)
        {
            foreach (GameObject cell in row)
            {
                Destroy(cell);
            }
        }
        simulationGrid.Clear();
    }
    
    public void HighlightReferenceBox(int index)
    {
        if (index >= 0 && index < referenceBoxes.Count)
        {
            Image boxImage = referenceBoxes[index].GetComponent<Image>();
            if (boxImage != null)
            {
                boxImage.color = accessedColor;
            }
        }
    }
    
    public void ResetReferenceBoxColors()
    {
        foreach (GameObject box in referenceBoxes)
        {
            Image boxImage = box.GetComponent<Image>();
            if (boxImage != null)
            {
                boxImage.color = defaultColor;
            }
        }
    }
    
    public void UpdateSimulationCell(int row, int col, string value, Color color)
    {
        if (row >= 0 && row < simulationGrid.Count && col >= 0 && col < simulationGrid[row].Count)
        {
            GameObject cell = simulationGrid[row][col];
            
            // Update text
            TMP_Text textComponent = cell.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = value;
            }
            
            // Update color
            Image cellImage = cell.GetComponent<Image>();
            if (cellImage != null)
            {
                cellImage.color = color;
            }
        }
    }
    
    public void Reset()
    {
        // Clear reference string
        referenceString.Clear();
        
        // Clear boxes
        ClearReferenceBoxes();
        
        // Clear simulation grid
        ClearSimulationGrid();
        
        // Reset input field
        if (referenceStringInput != null)
        {
            referenceStringInput.text = "";
        }
        
        Debug.Log("Page replacement simulator reset");
    }
} 
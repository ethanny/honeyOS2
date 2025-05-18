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
    public TMP_Text pageFaultCountText;
    public TMP_Text hitRateText;
    
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
    
    // FIFO Simulation
    public void RunFIFOSimulation()
    {
        if (referenceString.Count == 0)
        {
            Debug.LogWarning("No reference string to simulate");
            return;
        }
        
        // Initialize counters
        int pageFaults = 0;
        int hits = 0;
        
        // FIFO page replacement data structures
        HashSet<int> pageSet = new HashSet<int>();
        Queue<int> pageQueue = new Queue<int>();
        
        // Memory state representation for visualization
        int[] frames = new int[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            frames[i] = -1; // -1 means empty frame
        }
        
        // Process each page in the reference string
        for (int i = 0; i < referenceString.Count; i++)
        {
            int currentPage = referenceString[i];
            
            // Don't highlight reference boxes
            // HighlightReferenceBox(i);
            
            // Check if page is already in memory (hit)
            if (pageSet.Contains(currentPage))
            {
                // Page Hit
                hits++;
                
                // Update indicator cell
                UpdateSimulationCell(frameCount, i, "H", pageHitColor);
                
                // No change in frames, just copy previous column state
                if (i > 0)
                {
                    for (int f = 0; f < frameCount; f++)
                    {
                        // Copy the previous column's value
                        UpdateSimulationCell(f, i, 
                            frames[f] == -1 ? "" : frames[f].ToString(), 
                            frames[f] == currentPage ? accessedColor : defaultColor);
                    }
                }
            }
            else
            {
                // Page Fault
                pageFaults++;
                
                // Update indicator cell
                UpdateSimulationCell(frameCount, i, "F", pageFaultColor);
                
                if (pageSet.Count < frameCount)
                {
                    // Memory not full, add page to an empty frame
                    for (int f = 0; f < frameCount; f++)
                    {
                        if (frames[f] == -1)
                        {
                            frames[f] = currentPage;
                            break;
                        }
                    }
                }
                else
                {
                    // Memory full, replace oldest page (FIFO)
                    int oldestPage = pageQueue.Dequeue();
                    pageSet.Remove(oldestPage);
                    
                    // Find and replace the oldest page in frames
                    for (int f = 0; f < frameCount; f++)
                    {
                        if (frames[f] == oldestPage)
                        {
                            frames[f] = currentPage;
                            break;
                        }
                    }
                }
                
                // Add new page to set and queue
                pageSet.Add(currentPage);
                pageQueue.Enqueue(currentPage);
                
                // Update frame visualization
                for (int f = 0; f < frameCount; f++)
                {
                    UpdateSimulationCell(f, i, 
                        frames[f] == -1 ? "" : frames[f].ToString(), 
                        frames[f] == currentPage ? accessedColor : defaultColor);
                }
            }
        }
        
        // Display statistics
        if (pageFaultCountText != null)
        {
            pageFaultCountText.text = "Page Faults: " + pageFaults;
        }
        
        if (hitRateText != null)
        {
            float hitRate = (float)hits / referenceString.Count * 100f;
            hitRateText.text = "Hit Rate: " + hitRate.ToString("F2") + "%";
        }
    }
} 
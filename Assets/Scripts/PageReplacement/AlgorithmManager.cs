using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class AlgorithmManager : MonoBehaviour
{
    private string selectedAlgorithm;
    
    public TMP_Text algorithmTitleText;
    public TMP_InputField referenceStringInput;
    public Slider frameCountSlider;
    public TMP_Text frameCountText;
    public TMP_Text pageFaultCountText;
    public TMP_Text hitCountText;
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
    private Color accessedColor;
    private Color pageFaultColor;
    private Color pageHitColor;
    
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
        
        // Initialize colors from hex
        ColorUtility.TryParseHtmlString("#68BBFF", out accessedColor);
        ColorUtility.TryParseHtmlString("#FF556C", out pageFaultColor);
        ColorUtility.TryParseHtmlString("#4ED050", out pageHitColor);
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
                algorithmTitleText.text = "Page Algorithm";
                break;
        }
        
        Debug.Log("Selected algorithm: " + selectedAlgorithm);
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
        
        // Reset statistics
        if (pageFaultCountText != null)
        {
            pageFaultCountText.text = "Page Faults: 0";
        }
        
        if (hitCountText != null)
        {
            hitCountText.text = "Hits: 0";
        }
        
        if (hitRateText != null)
        {
            hitRateText.text = "Hit Rate: 0.00%";
        }
        
        // Reset input field
        if (referenceStringInput != null)
        {
            referenceStringInput.text = "";
        }
        
        Debug.Log("Page replacement simulator reset");
    }
    
    // Run the currently selected algorithm
    public void RunSelectedAlgorithm()
    {
        if (referenceString.Count == 0)
        {
            Debug.LogWarning("No reference string to simulate");
            return;
        }
        
        // Clear previous simulation results
        ClearSimulationResults();
        
        // Run the appropriate algorithm based on selection
        switch (selectedAlgorithm)
        {
            case "FIFO":
                StartCoroutine(RunFIFOSimulationCoroutine());
                break;
            case "OPR":
                StartCoroutine(RunOPRSimulationCoroutine());
                break;
            case "LRU":
                StartCoroutine(RunLRUSimulationCoroutine());
                break;
            case "MRU":
                StartCoroutine(RunMRUSimulationCoroutine());
                break;
            default:
                // Default to FIFO if no algorithm is selected
                StartCoroutine(RunFIFOSimulationCoroutine());
                break;
        }
    }
    
    // Placeholder methods for other algorithms
    private IEnumerator RunOPRSimulationCoroutine()
    {
        Debug.Log("OPR algorithm simulation started");
        
        // Initialize counters
        int pageFaults = 0;
        int hits = 0;
        
        // Memory state representation for visualization
        List<int> frames = new List<int>();
        for (int i = 0; i < frameCount; i++)
        {
            frames.Add(-1); // -1 means empty frame
        }
        
        // Process each page in the reference string
        for (int i = 0; i < referenceString.Count; i++)
        {
            int currentPage = referenceString[i];
            
            // Check if page is already in memory (hit)
            if (Search(currentPage, frames))
            {
                // Page Hit
                hits++;
                
                // Update indicator cell
                UpdateSimulationCell(frameCount, i, "H", pageHitColor);
                
                // No change in frames, just copy previous column state
                for (int f = 0; f < frameCount; f++)
                {
                    // Copy the previous column's value
                    UpdateSimulationCell(f, i, 
                        frames[f] == -1 ? "" : frames[f].ToString(), 
                        frames[f] == currentPage ? accessedColor : defaultColor);
                }
            }
            else
            {
                // Page Fault
                pageFaults++;
                
                // Update indicator cell
                UpdateSimulationCell(frameCount, i, "F", pageFaultColor);
                
                if (frames.Count(f => f != -1) < frameCount)
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
                    // Memory full, replace page that won't be used for the longest time
                    int indexToReplace = Predict(referenceString.ToArray(), frames, referenceString.Count, i + 1);
                    frames[indexToReplace] = currentPage;
                }
                
                // Update frame visualization
                for (int f = 0; f < frameCount; f++)
                {
                    UpdateSimulationCell(f, i, 
                        frames[f] == -1 ? "" : frames[f].ToString(), 
                        frames[f] == currentPage ? accessedColor : defaultColor);
                }
            }
            
            // Update statistics after each step
            if (pageFaultCountText != null)
            {
                pageFaultCountText.text = "Page Faults: " + pageFaults;
            }
            
            if (hitCountText != null)
            {
                hitCountText.text = "Hits: " + hits;
            }
            
            if (hitRateText != null)
            {
                float hitRate = (float)hits / (i + 1) * 100f;
                hitRateText.text = "Hit Rate: " + hitRate.ToString("F2") + "%";
            }
            
            // Wait for 1 second before the next step
            yield return new WaitForSeconds(1f);
        }
        
        // Final statistics
        if (pageFaultCountText != null)
        {
            pageFaultCountText.text = "Page Faults: " + pageFaults;
        }
        
        if (hitCountText != null)
        {
            hitCountText.text = "Hits: " + hits;
        }
        
        if (hitRateText != null)
        {
            float hitRate = (float)hits / referenceString.Count * 100f;
            hitRateText.text = "Hit Rate: " + hitRate.ToString("F2") + "%";
        }
    }
    
    // Helper functions for OPR algorithm
    
    // Check if a page exists in frames
    private bool Search(int page, List<int> frames)
    {
        return frames.Contains(page);
    }
    
    // Find the page that will not be used for the longest time in future
    private int Predict(int[] pages, List<int> frames, int totalPages, int currentIndex)
    {
        // Store the index of pages which are going to be used farthest in future
        int resultIndex = -1;
        int farthestPosition = currentIndex;
        
        for (int i = 0; i < frames.Count; i++)
        {
            // Skip empty frames
            if (frames[i] == -1)
                continue;
                
            int j;
            // Find when this page will be used next
            for (j = currentIndex; j < totalPages; j++)
            {
                if (frames[i] == pages[j])
                {
                    if (j > farthestPosition)
                    {
                        farthestPosition = j;
                        resultIndex = i;
                    }
                    break;
                }
            }
            
            // If a page is never referenced in future, return its index
            if (j == totalPages)
                return i;
        }
        
        // If all pages will be used again but one is farthest, return that
        // If no pages found or all are equally distant, return the first frame
        return (resultIndex == -1) ? 0 : resultIndex;
    }
    
    private IEnumerator RunLRUSimulationCoroutine()
    {
        Debug.Log("LRU algorithm simulation started");
        
        // Initialize counters
        int pageFaults = 0;
        int hits = 0;
        
        // LRU page replacement data structures
        HashSet<int> pageSet = new HashSet<int>();
        Dictionary<int, int> lastUsedIndex = new Dictionary<int, int>();
        
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
                
                // Update the last used index for the current page
                lastUsedIndex[currentPage] = i;
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
                    // Memory full, replace least recently used page
                    int lruPage = -1;
                    int lruIndex = int.MaxValue;
                    
                    // Find the least recently used page
                    foreach (int page in pageSet)
                    {
                        if (lastUsedIndex[page] < lruIndex)
                        {
                            lruIndex = lastUsedIndex[page];
                            lruPage = page;
                        }
                    }
                    
                    // Remove LRU page from set
                    pageSet.Remove(lruPage);
                    
                    // Find and replace the LRU page in frames
                    for (int f = 0; f < frameCount; f++)
                    {
                        if (frames[f] == lruPage)
                        {
                            frames[f] = currentPage;
                            break;
                        }
                    }
                }
                
                // Add new page to set and update last used index
                pageSet.Add(currentPage);
                
                // Update or add the last used index
                if (lastUsedIndex.ContainsKey(currentPage))
                {
                    lastUsedIndex[currentPage] = i;
                }
                else
                {
                    lastUsedIndex.Add(currentPage, i);
                }
                
                // Update frame visualization
                for (int f = 0; f < frameCount; f++)
                {
                    UpdateSimulationCell(f, i, 
                        frames[f] == -1 ? "" : frames[f].ToString(), 
                        frames[f] == currentPage ? accessedColor : defaultColor);
                }
            }
            
            // Update statistics after each step
            if (pageFaultCountText != null)
            {
                pageFaultCountText.text = "Page Faults: " + pageFaults;
            }
            
            if (hitCountText != null)
            {
                hitCountText.text = "Hits: " + hits;
            }
            
            if (hitRateText != null)
            {
                float hitRate = (float)hits / (i + 1) * 100f;
                hitRateText.text = "Hit Rate: " + hitRate.ToString("F2") + "%";
            }
            
            // Wait for 1 second before the next step
            yield return new WaitForSeconds(1f);
        }
        
        // Final statistics
        if (pageFaultCountText != null)
        {
            pageFaultCountText.text = "Page Faults: " + pageFaults;
        }
        
        if (hitCountText != null)
        {
            hitCountText.text = "Hits: " + hits;
        }
        
        if (hitRateText != null)
        {
            float hitRate = (float)hits / referenceString.Count * 100f;
            hitRateText.text = "Hit Rate: " + hitRate.ToString("F2") + "%";
        }
    }
    
    private IEnumerator RunMRUSimulationCoroutine()
    {
        Debug.Log("MRU algorithm simulation started");
        
        // Initialize counters
        int pageFaults = 0;
        int hits = 0;
        
        // MRU page replacement data structures
        HashSet<int> pageSet = new HashSet<int>();
        Dictionary<int, int> lastUsedIndex = new Dictionary<int, int>();
        
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
                
                // Update the last used index for the current page
                lastUsedIndex[currentPage] = i;
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
                    // Memory full, replace most recently used page
                    int mruPage = -1;
                    int mruIndex = -1;
                    
                    // Find the most recently used page
                    foreach (int page in pageSet)
                    {
                        if (lastUsedIndex[page] > mruIndex)
                        {
                            mruIndex = lastUsedIndex[page];
                            mruPage = page;
                        }
                    }
                    
                    // Remove MRU page from set
                    pageSet.Remove(mruPage);
                    
                    // Find and replace the MRU page in frames
                    for (int f = 0; f < frameCount; f++)
                    {
                        if (frames[f] == mruPage)
                        {
                            frames[f] = currentPage;
                            break;
                        }
                    }
                }
                
                // Add new page to set and update last used index
                pageSet.Add(currentPage);
                
                // Update or add the last used index
                if (lastUsedIndex.ContainsKey(currentPage))
                {
                    lastUsedIndex[currentPage] = i;
                }
                else
                {
                    lastUsedIndex.Add(currentPage, i);
                }
                
                // Update frame visualization
                for (int f = 0; f < frameCount; f++)
                {
                    UpdateSimulationCell(f, i, 
                        frames[f] == -1 ? "" : frames[f].ToString(), 
                        frames[f] == currentPage ? accessedColor : defaultColor);
                }
            }
            
            // Update statistics after each step
            if (pageFaultCountText != null)
            {
                pageFaultCountText.text = "Page Faults: " + pageFaults;
            }
            
            if (hitCountText != null)
            {
                hitCountText.text = "Hits: " + hits;
            }
            
            if (hitRateText != null)
            {
                float hitRate = (float)hits / (i + 1) * 100f;
                hitRateText.text = "Hit Rate: " + hitRate.ToString("F2") + "%";
            }
            
            // Wait for 1 second before the next step
            yield return new WaitForSeconds(1f);
        }
        
        // Final statistics
        if (pageFaultCountText != null)
        {
            pageFaultCountText.text = "Page Faults: " + pageFaults;
        }
        
        if (hitCountText != null)
        {
            hitCountText.text = "Hits: " + hits;
        }
        
        if (hitRateText != null)
        {
            float hitRate = (float)hits / referenceString.Count * 100f;
            hitRateText.text = "Hit Rate: " + hitRate.ToString("F2") + "%";
        }
    }
    
    // FIFO Simulation
    public void RunFIFOSimulation()
    {
        if (referenceString.Count == 0)
        {
            Debug.LogWarning("No reference string to simulate");
            return;
        }
        
        // Clear previous simulation results first
        ClearSimulationResults();
        
        // Start the simulation coroutine
        StartCoroutine(RunFIFOSimulationCoroutine());
    }
    
    private void ClearSimulationResults()
    {
        // Clear the grid display
        for (int i = 0; i < simulationGrid.Count; i++)
        {
            for (int j = 0; j < simulationGrid[i].Count; j++)
            {
                UpdateSimulationCell(i, j, "", defaultColor);
            }
        }
        
        // Reset statistics
        if (pageFaultCountText != null)
        {
            pageFaultCountText.text = "Page Faults: 0";
        }
        
        if (hitCountText != null)
        {
            hitCountText.text = "Hits: 0";
        }
        
        if (hitRateText != null)
        {
            hitRateText.text = "Hit Rate: 0.00%";
        }
    }
    
    private IEnumerator RunFIFOSimulationCoroutine()
    {
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
            
            // Update statistics after each step
            if (pageFaultCountText != null)
            {
                pageFaultCountText.text = "Page Faults: " + pageFaults;
            }
            
            if (hitCountText != null)
            {
                hitCountText.text = "Hits: " + hits;
            }
            
            if (hitRateText != null)
            {
                float hitRate = (float)hits / (i + 1) * 100f;
                hitRateText.text = "Hit Rate: " + hitRate.ToString("F2") + "%";
            }
            
            // Wait for 1 second before the next step
            yield return new WaitForSeconds(1f);
        }
        
        // Final statistics
        if (pageFaultCountText != null)
        {
            pageFaultCountText.text = "Page Faults: " + pageFaults;
        }
        
        if (hitCountText != null)
        {
            hitCountText.text = "Hits: " + hits;
        }
        
        if (hitRateText != null)
        {
            float hitRate = (float)hits / referenceString.Count * 100f;
            hitRateText.text = "Hit Rate: " + hitRate.ToString("F2") + "%";
        }
    }
} 
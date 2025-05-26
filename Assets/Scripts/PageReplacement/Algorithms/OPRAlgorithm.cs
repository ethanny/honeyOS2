using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OPRAlgorithm : PageReplacementAlgorithm
{
    public OPRAlgorithm(List<int> referenceString, int frameCount, AlgorithmManager manager) 
        : base(referenceString, frameCount, manager)
    {
    }
    
    public override IEnumerator RunSimulation()
    {
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
            if (frames.Contains(currentPage))
            {
                // Page Hit
                hits++;
                
                // Update indicator cell
                manager.UpdateSimulationCell(frameCount, i, "H", manager.pageHitColor);
                
                // No change in frames, just copy previous column state
                for (int f = 0; f < frameCount; f++)
                {
                    // Copy the previous column's value
                    manager.UpdateSimulationCell(f, i, 
                        frames[f] == -1 ? "" : frames[f].ToString(), 
                        frames[f] == currentPage ? manager.accessedColor : manager.defaultColor);
                }
            }
            else
            {
                // Page Fault
                pageFaults++;
                
                // Update indicator cell
                manager.UpdateSimulationCell(frameCount, i, "F", manager.pageFaultColor);
                
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
                    manager.UpdateSimulationCell(f, i, 
                        frames[f] == -1 ? "" : frames[f].ToString(), 
                        frames[f] == currentPage ? manager.accessedColor : manager.defaultColor);
                }
            }
            
            // Update statistics
            UpdateStatistics(pageFaults, hits, i);
            
            // Wait for next step with pause support
            yield return WaitForNextStep();
        }
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
} 
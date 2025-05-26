using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LFUAlgorithm : PageReplacementAlgorithm
{
    public LFUAlgorithm(List<int> referenceString, int frameCount, AlgorithmManager manager) 
        : base(referenceString, frameCount, manager)
    {
    }
    
    public override IEnumerator RunSimulation()
    {
        // Initialize counters
        int pageFaults = 0;
        int hits = 0;
        
        // LFU page replacement data structures
        HashSet<int> pageSet = new HashSet<int>();
        Dictionary<int, int> pageFrequency = new Dictionary<int, int>();
        Dictionary<int, int> lastUsedIndex = new Dictionary<int, int>(); // For breaking ties
        
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
                manager.UpdateSimulationCell(frameCount, i, "H", manager.pageHitColor);
                
                // Increase frequency count
                pageFrequency[currentPage]++;
                
                // Update last used index for tie-breaking
                lastUsedIndex[currentPage] = i;
                
                // No change in frames, just copy previous column state
                for (int f = 0; f < frameCount; f++)
                {
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
                    // Memory full, replace least frequently used page
                    int lfuPage = -1;
                    int minFrequency = int.MaxValue;
                    int earliestUsed = int.MaxValue;
                    
                    // Find the least frequently used page
                    // If there's a tie in frequency, use the least recently used page
                    foreach (int page in pageSet)
                    {
                        if (pageFrequency[page] < minFrequency || 
                            (pageFrequency[page] == minFrequency && lastUsedIndex[page] < earliestUsed))
                        {
                            minFrequency = pageFrequency[page];
                            earliestUsed = lastUsedIndex[page];
                            lfuPage = page;
                        }
                    }
                    
                    // Remove LFU page from set and frequency tracking
                    pageSet.Remove(lfuPage);
                    pageFrequency.Remove(lfuPage);
                    lastUsedIndex.Remove(lfuPage);
                    
                    // Find and replace the LFU page in frames
                    for (int f = 0; f < frameCount; f++)
                    {
                        if (frames[f] == lfuPage)
                        {
                            frames[f] = currentPage;
                            break;
                        }
                    }
                }
                
                // Add new page to set and initialize its frequency
                pageSet.Add(currentPage);
                pageFrequency[currentPage] = 1;
                lastUsedIndex[currentPage] = i;
                
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
            
            // Wait for 1 second before the next step
            yield return WaitForNextStep();
        }
    }
} 
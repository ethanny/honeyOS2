using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRUAlgorithm : PageReplacementAlgorithm
{
    public MRUAlgorithm(List<int> referenceString, int frameCount, AlgorithmManager manager) 
        : base(referenceString, frameCount, manager)
    {
    }
    
    public override IEnumerator RunSimulation()
    {
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
                manager.UpdateSimulationCell(frameCount, i, "H", manager.pageHitColor);
                
                // No change in frames, just copy previous column state
                if (i > 0)
                {
                    for (int f = 0; f < frameCount; f++)
                    {
                        // Copy the previous column's value
                        manager.UpdateSimulationCell(f, i, 
                            frames[f] == -1 ? "" : frames[f].ToString(), 
                            frames[f] == currentPage ? manager.accessedColor : manager.defaultColor);
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
                    manager.UpdateSimulationCell(f, i, 
                        frames[f] == -1 ? "" : frames[f].ToString(), 
                        frames[f] == currentPage ? manager.accessedColor : manager.defaultColor);
                }
            }
            
            // Update statistics
            UpdateStatistics(pageFaults, hits, i);
            
            // Wait for 1 second before the next step
            yield return new WaitForSeconds(1f);
        }
    }
} 
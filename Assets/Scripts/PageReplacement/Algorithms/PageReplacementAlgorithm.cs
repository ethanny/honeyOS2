using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class PageReplacementAlgorithm
{
    protected List<int> referenceString;
    protected int frameCount;
    
    // UI elements for updating
    protected AlgorithmManager manager;
    
    public PageReplacementAlgorithm(List<int> referenceString, int frameCount, AlgorithmManager manager)
    {
        this.referenceString = referenceString;
        this.frameCount = frameCount;
        this.manager = manager;
    }
    
    public abstract IEnumerator RunSimulation();
    
    // Helper method to update UI for a step
    protected void UpdateStatistics(int pageFaults, int hits, int currentStep)
    {
        // Update statistics after each step
        if (manager.pageFaultCountText != null)
        {
            manager.pageFaultCountText.text = "Page Faults: " + pageFaults;
        }
        
        if (manager.hitCountText != null)
        {
            manager.hitCountText.text = "Page Hits: " + hits;
        }
        
        if (manager.hitRateText != null)
        {
            float hitRate = (float)hits / (currentStep + 1) * 100f;
            manager.hitRateText.text = "Hit Rate: " + hitRate.ToString("F2") + "%";
        }
    }

    // Helper method to wait between steps with pause support
    protected IEnumerator WaitForNextStep()
    {
        // First check if we're paused
        yield return manager.WaitIfPaused();
        
        // Then wait for the step delay
        yield return new WaitForSeconds(1f);
        
        // Check for pause again after the delay
        yield return manager.WaitIfPaused();
    }
} 
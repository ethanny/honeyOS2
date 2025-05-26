using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Define the base class for all apps
public class App : MonoBehaviour
{   
    public AppIcon appIcon; // Reference to the associated app icon
    private Animator animator;
    protected WindowMaximizer windowMaximizer; // Reference to the window maximizer component
    private bool wasMaximizedBeforeMinimize = false; // Store window state before minimizing

    private void Start()
    {
        // Get the Animator component attached to this GameObject
        animator = GetComponent<Animator>();
        
        // Get the WindowMaximizer component
        windowMaximizer = GetComponent<WindowMaximizer>();
        if (windowMaximizer == null)
        {
            Debug.LogWarning($"WindowMaximizer component not found on {gameObject.name}. Window maximizing will not be available.");
        }
    }

    public virtual void Open()
    {   
        if (animator == null)
        {
            Debug.LogError($"Animator is null on {gameObject.name}! Animation will not play.");
            return;
        }
        
        // gameObject.SetActive(true);
        animator.Play("Open");
        UpdateIcon(AppState.Opened);

        // When opening from a closed state, always start in normal mode
        if (windowMaximizer != null)
        {
            // Only restore maximized state if the app was minimized
            if (appIcon.IsMinimized())
            {
                windowMaximizer.SetWindowState(wasMaximizedBeforeMinimize);
            }
            // else
            // {
            //     // Opening from closed state - start in normal mode
            //     windowMaximizer.SetWindowState(false);
            // }
        }
    }

    public virtual void Close()
    {   
        if (animator == null)
        {
            Debug.LogError($"Animator is null on {gameObject.name}! Animation will not play.");
            return;
        }
        
        // Reset window state and clear stored state
        if (windowMaximizer != null)
        {
            wasMaximizedBeforeMinimize = false;
            windowMaximizer.SetWindowState(false); // Always restore to normal size when closing
        }
        
        animator.Play("Close");
        // gameObject.SetActive(false);
        UpdateIcon(AppState.Closed);
        Reset();
    }

    public virtual void Minimize()
    {   
        if (animator == null)
        {
            Debug.LogError($"Animator is null on {gameObject.name}! Animation will not play.");
            return;
        }
        
        // Store window state before minimizing
        if (windowMaximizer != null)
        {
            wasMaximizedBeforeMinimize = windowMaximizer.IsMaximized;
            // windowMaximizer.SetWindowState(false); // Always restore to normal size when minimizing
        }
        
        animator.Play("Close");

        // Update the app icon state
        UpdateIcon(AppState.Minimized);
    }
    
    // Method to reset the app to its default state
    protected virtual void Reset() 
    {
        // Reset window state
        wasMaximizedBeforeMinimize = false;
        if (windowMaximizer != null)
        {
            windowMaximizer.SetWindowState(false);
        }
    }

    // Method to update the app icon indicator
    private void UpdateIcon(AppState state)
    {
        if (appIcon != null)
        {
            appIcon.UpdateIndicator(state);
        }
    }
}

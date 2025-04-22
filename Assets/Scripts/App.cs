using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Define the base class for all apps
public class App : MonoBehaviour
{   
    public AppIcon appIcon; // Reference to the associated app icon
    private Animator animator;

    private void Start()
    {
        // Get the Animator component attached to this GameObject
        animator = GetComponent<Animator>();
    }

    public virtual void Open()
    {   
        // gameObject.SetActive(true);
        animator.Play("Open");
        UpdateIcon(AppState.Opened);
    }

    public virtual void Close()
    {   
        animator.Play("Close");
        // gameObject.SetActive(false);
        UpdateIcon(AppState.Closed);
        Reset();
    }

    public virtual void Minimize()
    {   
        animator.Play("Close");

        // Update the app icon state
        UpdateIcon(AppState.Minimized);

        // Start a coroutine to delay deactivation
        // StartCoroutine(DeactivateAfterAnimation());
    }

    // Coroutine to deactivate the GameObject after minimize animation finishes
    // private IEnumerator DeactivateAfterAnimation()
    // {
    //     // Wait until the current animation completes
    //     yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    //     // Deactivate the GameObject
    //     // gameObject.SetActive(false);
       
    // }
    
    // Method to reset the app to its default state
    protected virtual void Reset() {}

    // Method to update the app icon indicator
    private void UpdateIcon(AppState state)
    {
        if (appIcon != null)
        {
            appIcon.UpdateIndicator(state);
        }
    }

}

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// Define the base class for all apps
public class App : MonoBehaviour, IPointerDownHandler
{
    public AppIcon appIcon; // Reference to the associated app icon
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        gameObject.SetActive(false); // Ensure it starts closed
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
        BringToFront();

        if (animator != null)
            animator.Play("Open");

        UpdateIcon(AppState.Opened);
    }

    public virtual void Close()
    {
        if (animator != null)
            animator.Play("Close");

        UpdateIcon(AppState.Closed);
        Reset();
        gameObject.SetActive(false); // ✅ Actually hides the app
    }

    public virtual void Minimize()
    {
        if (animator != null)
            animator.Play("Close");

        UpdateIcon(AppState.Minimized);
        gameObject.SetActive(false); // ✅ Actually hides the app
    }

    public void BringToFront()
    {
        transform.SetAsLastSibling(); // Moves this app to the top of the UI stack
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        BringToFront(); // Brings app forward when clicked
    }

    protected virtual void Reset() { }

    private void UpdateIcon(AppState state)
    {
        if (appIcon != null)
        {
            appIcon.UpdateIndicator(state);
        }
    }

    public void OnCloseButtonPressed()
{
    Close(); // Calls this app's Close() method
}

public void OnMinimizeButtonPressed()
{
    Minimize(); // Calls this app's Minimize() method
}
}

using UnityEngine;
using UnityEngine.EventSystems;

public class PolicyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator animator;

    void Start()
    {
        // Get the Animator component attached to the same GameObject
        animator = GetComponent<Animator>();
    }

    // This method is called when the mouse pointer enters the GameObject's area
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (animator != null)
        {
            animator.SetBool("hover", true); // Trigger hover animation
        }
    }

    // This method is called when the mouse pointer exits the GameObject's area
    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator != null)
        {
            animator.SetBool("hover", false); // Trigger idle animation
        }
    }
}

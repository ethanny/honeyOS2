using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PopupController : MonoBehaviour {

    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip showSound;
    [SerializeField] private AudioClip hideSound;

    void Start() {
        OffPopup();
    }

    public void Show() {
        gameObject.SetActive(true);
        _animator.Play("show");

        // Play the show sound effect
        if (_audioSource != null && showSound != null) {
            _audioSource.PlayOneShot(showSound);
        }
    }

    public void Hide() {
        _animator.Play("hide");

        // Play the hide sound effect
        if (_audioSource != null && hideSound != null) {
            _audioSource.PlayOneShot(hideSound);
        }

        // Start a coroutine to delay deactivation
        StartCoroutine(DeactivateAfterAnimation());
    }

    // Coroutine to deactivate the GameObject after animation finishes
    private IEnumerator DeactivateAfterAnimation()
    {
        // Wait for the length of the "hide" animation
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);

        // Deactivate the GameObject
        gameObject.SetActive(false);
    }

    public void OffPopup() {
        gameObject.SetActive(false);
    }

}
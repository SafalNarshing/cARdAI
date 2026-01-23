using UnityEngine;
using UnityEngine.InputSystem; // Required for New Input System

public class TapAnimation : MonoBehaviour
{
    [Header("Settings")]
    public Animator animator;
    public AudioSource audioSource;
    public string triggerName = "Roar";

    void Update()
    {
        // Check for Touch Input (Mobile)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
            CheckForHit(touchPos);
        }
        // Check for Mouse Input (Editor Testing)
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            CheckForHit(mousePos);
        }
    }

    void CheckForHit(Vector2 screenPos)
    {
        // Fire a Ray from camera to the input position
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Did we hit THIS specific object?
            if (hit.transform == this.transform)
            {
                PlayAnimationAndSound();
            }
        }
    }

    void PlayAnimationAndSound()
    {
        // Trigger the animation
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }

        // Play the sound
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}
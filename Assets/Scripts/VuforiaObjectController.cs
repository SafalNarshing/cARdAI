using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ARCollectionManager : MonoBehaviour
{
    [System.Serializable]
    public struct ARItem
    {
        public string name;
        public GameObject model;
        public AudioClip audioClip;
    }

    [Header("Collection Setup")]
    public List<ARItem> arItems;

    [Header("Global Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float rotationSpeed = 0.2f;

    private GameObject currentSelectedModel = null;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // 1. TOUCH INPUT
        if (Touch.activeTouches.Count > 0)
        {
            Touch primaryTouch = Touch.activeTouches[0];

            if (primaryTouch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                SelectObject(primaryTouch.screenPosition);
            }
            else if (primaryTouch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                if (currentSelectedModel != null)
                {
                    RotateSelected(primaryTouch.delta);
                }
            }
            else if (primaryTouch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                currentSelectedModel = null;
            }
        }
        // 2. MOUSE INPUT
        else if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                SelectObject(Mouse.current.position.ReadValue());
            }
            else if (Mouse.current.leftButton.isPressed && currentSelectedModel != null)
            {
                RotateSelected(Mouse.current.delta.ReadValue());
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                currentSelectedModel = null;
            }
        }
    }

    void SelectObject(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            foreach (var item in arItems)
            {
                if (item.model == null) continue;

                if (hit.transform == item.model.transform || hit.transform.IsChildOf(item.model.transform))
                {
                    currentSelectedModel = item.model;
                    PlayAudio(item.audioClip);
                    return;
                }
            }
        }
    }

    void RotateSelected(Vector2 delta)
    {
        if (currentSelectedModel != null)
        {
            // Only use horizontal drag (delta.x) to spin
            float rotationAmount = -delta.x * rotationSpeed;

            // Space.Self ensures it rotates around the object's local Y axis
            // (e.g., spinning like a top on the card surface)
            currentSelectedModel.transform.Rotate(Vector3.up, rotationAmount, Space.Self);
        }
    }

    void PlayAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
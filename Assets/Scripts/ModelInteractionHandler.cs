using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ModelInteractionHandle : MonoBehaviour
{
    [Header("References")]
    public Transform modelToControl;   // the actual placed model (parent, if circle is a child)
    public Collider circleCollider;    // a trigger collider matching the circle's shape
    public ARRaycastManager raycastManager;

    private bool isDraggingCircle = false;

    private float initialPinchDistance;
    private Vector3 initialScale;

    void OnEnable() => EnhancedTouchSupport.Enable();
    void OnDisable() => EnhancedTouchSupport.Disable();

    void Update()
    {
        HandleSingleTouchMove();
        HandlePinchScale();
    }

    void HandleSingleTouchMove()
    {
        if (Touchscreen.current == null) return;
        if (Touchscreen.current.touches.Count != 1) return;

        var touch = Touchscreen.current.primaryTouch;
        Vector2 screenPos = touch.position.ReadValue();

        if (touch.press.wasPressedThisFrame)
        {
            isDraggingCircle = IsTouchOnCircle(screenPos);
        }

        if (isDraggingCircle && touch.press.isPressed)
        {
            var hits = new List<ARRaycastHit>();
            if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
            {
                modelToControl.position = hits[0].pose.position;
            }
        }

        if (touch.press.wasReleasedThisFrame)
        {
            isDraggingCircle = false;
        }
    }

    void HandlePinchScale()
    {
        if (Touchscreen.current == null) return;
        if (Touchscreen.current.touches.Count < 2) return;

        var t1 = Touchscreen.current.touches[0];
        var t2 = Touchscreen.current.touches[1];

        float currentDistance = Vector2.Distance(t1.position.ReadValue(), t2.position.ReadValue());

        if (t1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began ||
            t2.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
        {
            initialPinchDistance = currentDistance;
            initialScale = modelToControl.localScale;
        }
        else
        {
            float scaleFactor = currentDistance / initialPinchDistance;
            modelToControl.localScale = initialScale * scaleFactor;
        }
    }

    bool IsTouchOnCircle(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.collider == circleCollider;
        }
        return false;
    }
}
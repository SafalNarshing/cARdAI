using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;

public class ARplace : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlacementUIHandler uiHandler; // <--- Drag UI Handler Here

    [Header("Model To Place")]
    [SerializeField] private GameObject modelToPlace; // set dynamically from combo selection UI

    [Header("Interaction Handle")]
    [SerializeField] private GameObject circleHandlePrefab; // flat ring prefab, spawned under the model
    [SerializeField] private float circleHandleYOffset = -1f;

    bool isPlacing = false;
    bool hasPlaced = false;   // 🔒 PLACE ONLY ONCE

    GameObject placedObject;
    GameObject activeCircleHandle;

    // --- Circle drag (move) state ---
    bool isDraggingCircle = false;

    // --- Circle pinch (scale) state ---
    float initialPinchDistance;
    Vector3 initialScale;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        Debug.Log("[ARplace] OnEnable — EnhancedTouchSupport enabled");
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        if (!raycastManager)
        {
            Debug.LogWarning("[ARplace] raycastManager is NULL — assign it in the Inspector!");
            return;
        }

        HandlePlacement();
        HandleCircleDragMove();
        HandleCirclePinchScale();

        // Continuous check — logs if the placed model becomes inactive/destroyed unexpectedly
        if (hasPlaced && placedObject != null)
        {
            if (!placedObject.activeSelf)
            {
                Debug.LogWarning("[ARplace] placedObject became INACTIVE this frame!");
            }
        }
        else if (hasPlaced && placedObject == null)
        {
            Debug.LogWarning("[ARplace] placedObject is NULL even though hasPlaced is true — it was destroyed!");
        }
    }

    // --- Called by UI Handler when close button is clicked ---
    public void ResetPlacement()
    {
        Debug.Log("[ARplace] ResetPlacement() called");
        hasPlaced = false;
        placedObject = null;

        if (activeCircleHandle != null)
        {
            Destroy(activeCircleHandle);
            activeCircleHandle = null;
        }
    }

    public void SetModelToPlace(GameObject newModel)
    {
        modelToPlace = newModel;
        Debug.Log($"[ARplace] Model to place set: {(newModel != null ? newModel.name : "NULL")}");
    }

    void HandlePlacement()
    {
        if (isPlacing) return;
        if (hasPlaced) return;   // 🔒 block future placements

        bool pressed = false;
        Vector2 screenPosition = default;

        if (Touchscreen.current != null)
        {
            var primary = Touchscreen.current.primaryTouch;
            if (primary.press.wasPressedThisFrame)
            {
                Debug.Log("[ARplace] Touch press detected (Touchscreen)");
                pressed = true;
                screenPosition = primary.position.ReadValue();
            }
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("[ARplace] Mouse click detected");
            pressed = true;
            screenPosition = Mouse.current.position.ReadValue();
        }

        if (pressed)
        {
            Debug.Log($"[ARplace] Attempting placement at screen position: {screenPosition}");
            isPlacing = true;
            PlaceObject(screenPosition);
        }
    }

    void PlaceObject(Vector2 touchPosition)
    {
        var rayHits = new List<ARRaycastHit>();
        raycastManager.Raycast(touchPosition, rayHits, TrackableType.PlaneWithinPolygon);

        Debug.Log($"[ARplace] Raycast hits found: {rayHits.Count}");

        if (rayHits.Count > 0)
        {
            Pose hitPose = rayHits[0].pose;

            if (modelToPlace == null)
            {
                Debug.LogWarning("[ARplace] No model selected to place yet.");
                StartCoroutine(SetIsPlacingToFalseWithDelay());
                return;
            }

            placedObject = Instantiate(
                modelToPlace,
                hitPose.position,
                hitPose.rotation
            );

            hasPlaced = true;
            Debug.Log($"[ARplace] Placed model: {placedObject.name}, active: {placedObject.activeSelf}, scale: {placedObject.transform.localScale}, pos: {hitPose.position}");

            SpawnCircleHandle(hitPose);

            // --- TEMPORARILY DISABLED FOR ISOLATION TEST ---
            // If the model stays visible with this commented out, uiHandler.ShowUIForModel()
            // is the cause of the disappearance, and we need to fix that method directly.
            /*
            if (uiHandler != null)
            {
                uiHandler.ShowUIForModel(placedObject);
                Debug.Log($"[ARplace] After ShowUIForModel, active: {placedObject.activeSelf}");
            }
            */
        }
        else
        {
            Debug.Log("[ARplace] No plane detected at tap location");
        }

        StartCoroutine(SetIsPlacingToFalseWithDelay());
    }

    void SpawnCircleHandle(Pose hitPose)
    {
        if (circleHandlePrefab == null)
        {
            Debug.LogWarning("[ARplace] No circle handle prefab assigned — skipping handle spawn.");
            return;
        }
        if (placedObject == null) return;

        Vector3 handlePos = hitPose.position + Vector3.up * circleHandleYOffset;

        activeCircleHandle = Instantiate(
            circleHandlePrefab,
            handlePos,
            circleHandlePrefab.transform.rotation
        );

        activeCircleHandle.transform.SetParent(null);

        Debug.Log($"[ARplace] Circle handle spawned at {activeCircleHandle.transform.position}, rotation: {activeCircleHandle.transform.eulerAngles}");
    }

    IEnumerator SetIsPlacingToFalseWithDelay()
    {
        yield return new WaitForSeconds(0.25f);
        isPlacing = false;
    }

    // --- Single-finger drag on the circle handle: moves the model ---
    void HandleCircleDragMove()
    {
        if (!hasPlaced || placedObject == null || activeCircleHandle == null) return;
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
                Pose hitPose = hits[0].pose;
                placedObject.transform.position = hitPose.position;
                activeCircleHandle.transform.position = hitPose.position + Vector3.up * circleHandleYOffset;
            }
        }

        if (touch.press.wasReleasedThisFrame)
        {
            isDraggingCircle = false;
        }
    }

    // --- Two-finger pinch, only while a finger is on the circle: scales the model ---
    void HandleCirclePinchScale()
    {
        if (!hasPlaced || placedObject == null) return;
        if (Touchscreen.current == null) return;
        if (Touchscreen.current.touches.Count < 2) return;

        var touch1 = Touchscreen.current.touches[0];
        var touch2 = Touchscreen.current.touches[1];

        Vector2 pos1 = touch1.position.ReadValue();
        Vector2 pos2 = touch2.position.ReadValue();

        bool eitherOnCircle = IsTouchOnCircle(pos1) || IsTouchOnCircle(pos2);
        if (!eitherOnCircle) return;

        float currentDistance = Vector2.Distance(pos1, pos2);

        if (touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began ||
            touch2.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
        {
            initialPinchDistance = currentDistance;
            initialScale = placedObject.transform.localScale;
        }
        else
        {
            float scaleFactor = currentDistance / initialPinchDistance;
            placedObject.transform.localScale = initialScale * scaleFactor;
        }
    }

    bool IsTouchOnCircle(Vector2 screenPos)
    {
        if (activeCircleHandle == null || Camera.main == null) return false;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.collider != null && hit.collider.gameObject == activeCircleHandle;
        }
        return false;
    }
}
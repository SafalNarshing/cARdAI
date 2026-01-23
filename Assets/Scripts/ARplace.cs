using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro; // TextMeshPro Dropdown

public class ARplace : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlacementUIHandler uiHandler; // Optional UI handler

    [Header("Placeable Prefabs")]
    [SerializeField] private List<GameObject> placeablePrefabs;
    [SerializeField] private int selectedPrefabIndex = 0; // default first prefab

    [Header("Settings")]
    [SerializeField] private bool allowMovement = false; // Currently disabled

    [Header("UI")]
    [SerializeField] private TMP_Dropdown prefabDropdown; // assign your TMP dropdown here

    private bool isPlacing = false;
    private bool hasPlaced = false;

    private GameObject placedObject;

    private float initialDistance;
    private Vector3 initialScale;

    private float initialAngle;
    private float currentYRotation;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();

        if (prefabDropdown != null)
        {
            prefabDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }
    }

    void Start()
    {
        if (prefabDropdown != null)
        {
            prefabDropdown.onValueChanged.AddListener(OnDropdownChanged);

            // Initialize selectedPrefabIndex with current dropdown value
            selectedPrefabIndex = prefabDropdown.value;
            Debug.Log("Initial prefab selected: " + placeablePrefabs[selectedPrefabIndex].name);
        }
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();

        if (prefabDropdown != null)
        {
            prefabDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }
    }

    private void OnDropdownChanged(int index)
    {
        if (index < 0 || index >= placeablePrefabs.Count) return;

        selectedPrefabIndex = index;

        // Remove any currently placed object so user can place the new prefab
        if (placedObject != null)
        {
            Destroy(placedObject);
            placedObject = null;
            hasPlaced = false; // allow placement again
        }
    }

    void Update()
    {
        if (!raycastManager) return;

        HandlePlacement();
        HandleScaleAndRotation();
    }

    public void ResetPlacement()
    {
        hasPlaced = false;
        placedObject = null;
    }

    void HandlePlacement()
    {
        if (isPlacing) return;
        if (hasPlaced && !allowMovement) return; // Locked

        bool pressed = false;
        Vector2 screenPosition = default;

        if (Touchscreen.current != null)
        {
            var primary = Touchscreen.current.primaryTouch;
            if (primary.press.wasPressedThisFrame)
            {
                pressed = true;
                screenPosition = primary.position.ReadValue();
            }
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            pressed = true;
            screenPosition = Mouse.current.position.ReadValue();
        }

        // Ignore touches over UI
        if (pressed && EventSystem.current.IsPointerOverGameObject()) return;

        if (pressed)
        {
            isPlacing = true;
            PlaceObject(screenPosition);
        }
    }

    void PlaceObject(Vector2 touchPosition)
    {
        var rayHits = new List<ARRaycastHit>();
        raycastManager.Raycast(touchPosition, rayHits, TrackableType.PlaneWithinPolygon);

        if (rayHits.Count > 0)
        {
            Pose hitPose = rayHits[0].pose;

            if (placedObject == null)
            {
                GameObject prefabToPlace = placeablePrefabs[selectedPrefabIndex];
                placedObject = Instantiate(prefabToPlace, hitPose.position, hitPose.rotation);
                hasPlaced = true;
            }
            else if (allowMovement)
            {
                placedObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            }

            if (uiHandler != null)
                uiHandler.ShowUIForModel(placedObject);
        }

        StartCoroutine(SetIsPlacingToFalseWithDelay());
    }

    IEnumerator SetIsPlacingToFalseWithDelay()
    {
        yield return new WaitForSeconds(0.25f);
        isPlacing = false;
    }

    void HandleScaleAndRotation()
    {
        if (!hasPlaced) return;
        if (placedObject == null) return;
        if (Touchscreen.current == null) return;
        if (Touchscreen.current.touches.Count < 2) return;

        var touch1 = Touchscreen.current.touches[0];
        var touch2 = Touchscreen.current.touches[1];

        if (touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began ||
            touch2.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
        {
            initialDistance = Vector2.Distance(
                touch1.position.ReadValue(),
                touch2.position.ReadValue()
            );

            initialScale = placedObject.transform.localScale;

            Vector2 dir = touch2.position.ReadValue() - touch1.position.ReadValue();
            initialAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            currentYRotation = placedObject.transform.eulerAngles.y;
        }

        if (touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved ||
            touch2.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            float currentDistance = Vector2.Distance(
                touch1.position.ReadValue(),
                touch2.position.ReadValue()
            );

            float scaleFactor = currentDistance / initialDistance;
            placedObject.transform.localScale = initialScale * scaleFactor;

            Vector2 dir = touch2.position.ReadValue() - touch1.position.ReadValue();
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float angleDelta = angle - initialAngle;

            placedObject.transform.rotation =
                Quaternion.Euler(0, currentYRotation - angleDelta, 0);
        }
    }
}

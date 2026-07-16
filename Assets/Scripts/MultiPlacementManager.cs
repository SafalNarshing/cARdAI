using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections;

public class MultiPlacementManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ComboLookup comboLookup;

    [Header("Combine UI")]
    [SerializeField] private GameObject combineButtonObject; // the "Combine" button GameObject

    [Header("Circle Handle (optional, reused per placed item)")]
    [SerializeField] private GameObject circleHandlePrefab;
    [SerializeField] private float circleHandleYOffset = -1f;

    // Currently "armed" model waiting to be placed on next tap
    private GameObject pendingModelPrefab;
    private string pendingCardId;
    private PlacementCardButton lastSelectedButton;

    // Tracks everything placed so far: cardId -> placed instance
    private Dictionary<string, GameObject> placedItems = new Dictionary<string, GameObject>();
    private List<GameObject> spawnedCircleHandles = new List<GameObject>();
    private GameObject combinedResultObject;

    bool isPlacing = false;

    void OnEnable() => EnhancedTouchSupport.Enable();
    void OnDisable() => EnhancedTouchSupport.Disable();

    void Start()
    {
        if (combineButtonObject != null)
            combineButtonObject.SetActive(false); // hidden by default
    }

    void Update()
    {
        if (!raycastManager) return;
        HandleTapToPlace();
    }

    // --- Called by each card button when tapped (arms it for placement) ---
    public void SelectCardToPlace(string cardId, GameObject modelPrefab, PlacementCardButton button)
    {
        // Un-highlight whichever card was selected before
        if (lastSelectedButton != null)
            lastSelectedButton.SetHighlighted(false);

        pendingCardId = cardId;
        pendingModelPrefab = modelPrefab;
        lastSelectedButton = button;
        button.SetHighlighted(true);

        Debug.Log($"[MultiPlacementManager] Armed for placement: {cardId}");
    }

    void HandleTapToPlace()
    {
        if (isPlacing) return;
        if (pendingModelPrefab == null) return; // nothing armed, ignore taps

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

        if (pressed)
        {
            isPlacing = true;
            PlaceArmedModel(screenPosition);
        }
    }

    void PlaceArmedModel(Vector2 touchPosition)
    {
        var rayHits = new List<ARRaycastHit>();
        raycastManager.Raycast(touchPosition, rayHits, TrackableType.PlaneWithinPolygon);

        if (rayHits.Count > 0)
        {
            Pose hitPose = rayHits[0].pose;

            // If this cardId was already placed before, remove the old instance first
            if (placedItems.ContainsKey(pendingCardId))
            {
                Destroy(placedItems[pendingCardId]);
                placedItems.Remove(pendingCardId);
            }

            GameObject newInstance = Instantiate(pendingModelPrefab, hitPose.position, hitPose.rotation);
            placedItems[pendingCardId] = newInstance;

            SpawnCircleHandle(hitPose);

            Debug.Log($"[MultiPlacementManager] Placed {pendingCardId} at {hitPose.position}");

            // Clear the armed state after placing
            pendingModelPrefab = null;
            pendingCardId = null;

            if (lastSelectedButton != null)
            {
                lastSelectedButton.SetHighlighted(false);
                lastSelectedButton = null;
            }

            CheckForCombo();
        }

        StartCoroutine(SetIsPlacingToFalseWithDelay());
    }

    void SpawnCircleHandle(Pose hitPose)
    {
        if (circleHandlePrefab == null) return;

        Vector3 handlePos = hitPose.position + Vector3.up * circleHandleYOffset;
        GameObject handle = Instantiate(circleHandlePrefab, handlePos, circleHandlePrefab.transform.rotation);
        handle.transform.SetParent(null);
        spawnedCircleHandles.Add(handle);
    }

    // --- Check every placed pair against ComboLookup ---
    void CheckForCombo()
    {
        List<string> placedIds = new List<string>(placedItems.Keys);

        for (int i = 0; i < placedIds.Count; i++)
        {
            for (int j = i + 1; j < placedIds.Count; j++)
            {
                ActionPair match = comboLookup.FindComboPair(placedIds[i], placedIds[j]);
                if (match != null)
                {
                    Debug.Log($"[MultiPlacementManager] Combo match found: {placedIds[i]} + {placedIds[j]}");
                    if (combineButtonObject != null)
                        combineButtonObject.SetActive(true);
                    return; // stop at first valid match
                }
            }
        }

        // No match yet — keep Combine hidden
        if (combineButtonObject != null)
            combineButtonObject.SetActive(false);
    }

    // --- Called by the Combine button's OnClick ---
    public void OnCombineClicked()
    {
        List<string> placedIds = new List<string>(placedItems.Keys);

        for (int i = 0; i < placedIds.Count; i++)
        {
            for (int j = i + 1; j < placedIds.Count; j++)
            {
                ActionPair match = comboLookup.FindComboPair(placedIds[i], placedIds[j]);
                if (match != null)
                {
                    SpawnCombinedModel(match, placedIds[i], placedIds[j]);
                    return;
                }
            }
        }
    }
    void SpawnCombinedModel(ActionPair match, string idA, string idB)
    {
        if (match.comboModel == null) return;

        Vector3 posA = placedItems[idA].transform.position;
        Vector3 posB = placedItems[idB].transform.position;
        Vector3 midpoint = Vector3.Lerp(posA, posB, 0.5f);

        Vector3 offset = new Vector3(0f, 0f, 0.15f);
        Vector3 spawnPos = midpoint + offset;

        if (combinedResultObject != null)
            Destroy(combinedResultObject);

        combinedResultObject = Instantiate(match.comboModel, spawnPos, Quaternion.identity);

        Debug.Log($"[MultiPlacementManager] Combined model spawned at {spawnPos}");

        // --- Remove the two individual models now that the combo exists ---
        if (placedItems.ContainsKey(idA))
        {
            Destroy(placedItems[idA]);
            placedItems.Remove(idA);
        }
        if (placedItems.ContainsKey(idB))
        {
            Destroy(placedItems[idB]);
            placedItems.Remove(idB);
        }

        // --- Also remove their circle handles, since those models are gone ---
        foreach (var handle in spawnedCircleHandles)
        {
            if (handle != null) Destroy(handle);
        }
        spawnedCircleHandles.Clear();

        if (combineButtonObject != null)
            combineButtonObject.SetActive(false);
    }

    // --- Called by the cross/reset button ---
    public void ResetAll()
    {
        foreach (var kvp in placedItems)
        {
            if (kvp.Value != null) Destroy(kvp.Value);
        }
        placedItems.Clear();

        foreach (var handle in spawnedCircleHandles)
        {
            if (handle != null) Destroy(handle);
        }
        spawnedCircleHandles.Clear();

        if (combinedResultObject != null)
        {
            Destroy(combinedResultObject);
            combinedResultObject = null;
        }

        pendingModelPrefab = null;
        pendingCardId = null;

        if (lastSelectedButton != null)
        {
            lastSelectedButton.SetHighlighted(false);
            lastSelectedButton = null;
        }

        if (combineButtonObject != null)
            combineButtonObject.SetActive(false);

        Debug.Log("[MultiPlacementManager] Reset complete — ready to place again");
    }

    IEnumerator SetIsPlacingToFalseWithDelay()
    {
        yield return new WaitForSeconds(0.25f);
        isPlacing = false;
    }
}
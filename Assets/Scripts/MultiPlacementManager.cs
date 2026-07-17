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
    public ARRaycastManager raycastManager;
    [SerializeField] private ComboLookup comboLookup;

    [Header("Combine UI")]
    [SerializeField] private GameObject combineButtonObject;

    [Header("Circle Handle (optional, reused per placed item)")]
    [SerializeField] private GameObject circleHandlePrefab;
    [SerializeField] private float circleHandleYOffset = -1f;

    private GameObject pendingModelPrefab;
    private string pendingCardId;
    private PlacementCardButton lastSelectedButton;

    private Dictionary<string, GameObject> placedItems = new Dictionary<string, GameObject>();
    private List<GameObject> spawnedCircleHandles = new List<GameObject>();
    private GameObject combinedResultObject;

    bool isPlacing = false;

    void OnEnable() => EnhancedTouchSupport.Enable();
    void OnDisable() => EnhancedTouchSupport.Disable();

    void Start()
    {
        if (combineButtonObject != null)
            combineButtonObject.SetActive(false);
    }

    void Update()
    {
        if (!raycastManager) return;
        HandleTapToPlace();
    }

    public void SelectCardToPlace(string cardId, GameObject modelPrefab, PlacementCardButton button)
    {
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
        if (pendingModelPrefab == null) return;

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

            if (placedItems.ContainsKey(pendingCardId))
            {
                Destroy(placedItems[pendingCardId]);
                placedItems.Remove(pendingCardId);
            }

            GameObject newInstance = Instantiate(pendingModelPrefab, hitPose.position, hitPose.rotation);
            placedItems[pendingCardId] = newInstance;

            // Assign the runtime raycast manager reference to this instance's interaction handle
            AssignRaycastManagerToHandle(newInstance);

            SpawnCircleHandle(hitPose);

            Debug.Log($"[MultiPlacementManager] Placed {pendingCardId} at {hitPose.position}");

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

    // Finds ModelInteractionHandle on the instantiated object (if present) and assigns raycastManager
    void AssignRaycastManagerToHandle(GameObject instance)
    {
        var handle = instance.GetComponent<ModelInteractionHandle>();
        if (handle != null)
        {
            handle.raycastManager = raycastManager;
        }
    }

    void SpawnCircleHandle(Pose hitPose)
    {
        if (circleHandlePrefab == null) return;

        Vector3 handlePos = hitPose.position + Vector3.up * circleHandleYOffset;
        GameObject handle = Instantiate(circleHandlePrefab, handlePos, circleHandlePrefab.transform.rotation);
        handle.transform.SetParent(null);
        spawnedCircleHandles.Add(handle);
    }

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
                    return;
                }
            }
        }

        if (combineButtonObject != null)
            combineButtonObject.SetActive(false);
    }

    public void OnCombineClicked()
    {
        Debug.Log($"[MultiPlacementManager] OnCombineClicked called. Items placed: {placedItems.Count}");
        foreach (var key in placedItems.Keys)
            Debug.Log($"  - placed: {key}");

        List<string> placedIds = new List<string>(placedItems.Keys);

        for (int i = 0; i < placedIds.Count; i++)
        {
            for (int j = i + 1; j < placedIds.Count; j++)
            {
                ActionPair match = comboLookup.FindComboPair(placedIds[i], placedIds[j]);
                Debug.Log($"[MultiPlacementManager] Checking {placedIds[i]}+{placedIds[j]} -> match found: {match != null}");
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
        if (match.comboModel == null)
        {
            Debug.LogWarning("[MultiPlacementManager] match.comboModel is NULL — check ComboLookup assignment!");
            return;
        }

        Vector3 posA = placedItems[idA].transform.position;
        Vector3 posB = placedItems[idB].transform.position;
        Vector3 midpoint = Vector3.Lerp(posA, posB, 0.5f);

        Vector3 offset = new Vector3(0f, 0f, 0.15f);
        Vector3 spawnPos = midpoint + offset;

        if (combinedResultObject != null)
            Destroy(combinedResultObject);

        combinedResultObject = Instantiate(match.comboModel, spawnPos, Quaternion.identity);

        // Assign raycast manager to the combined model's interaction handle too
        AssignRaycastManagerToHandle(combinedResultObject);

        Debug.Log($"[MultiPlacementManager] Combined model spawned: {combinedResultObject.name} at {spawnPos}, active: {combinedResultObject.activeSelf}");

        // Remove the two individual models now that the combo exists
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

        // Remove their circle handles too
        foreach (var handle in spawnedCircleHandles)
        {
            if (handle != null) Destroy(handle);
        }
        spawnedCircleHandles.Clear();

        if (combineButtonObject != null)
            combineButtonObject.SetActive(false);
    }

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
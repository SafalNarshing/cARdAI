using System.Collections.Generic;
using UnityEngine;

public class ActionPairManager : MonoBehaviour
{
    [SerializeField] private List<ActionPair> pairs = new List<ActionPair>();

    private HashSet<string> activeTargets = new HashSet<string>();
    private Dictionary<string, Transform> lastKnownTransform = new Dictionary<string, Transform>();

    public void OnTargetFound(string targetName, Transform targetTransform)
    {
        activeTargets.Add(targetName);
        lastKnownTransform[targetName] = targetTransform;
        CheckAllPairs();
    }

    public void OnTargetLost(string targetName)
    {
        activeTargets.Remove(targetName);

        foreach (var pair in pairs)
        {
            if (pair.targetA == targetName || pair.targetB == targetName)
            {
                if (pair.comboModel != null)
                    pair.comboModel.SetActive(false);
            }
        }
    }

    private void CheckAllPairs()
    {
        foreach (var pair in pairs)
        {
            bool bothPresent = activeTargets.Contains(pair.targetA) && activeTargets.Contains(pair.targetB);

            if (bothPresent && pair.comboModel != null)
            {
                pair.comboModel.SetActive(true);
            }
        }
    }

    // NEW: runs every frame, keeps active combo models glued to the midpoint
    private void Update()
    {
        foreach (var pair in pairs)
        {
            bool bothPresent = activeTargets.Contains(pair.targetA) && activeTargets.Contains(pair.targetB);

            if (bothPresent && pair.comboModel != null && pair.comboModel.activeSelf)
            {
                Vector3 posA = lastKnownTransform[pair.targetA].position;
                Vector3 posB = lastKnownTransform[pair.targetB].position;

                pair.comboModel.transform.position = Vector3.Lerp(posA, posB, 0.5f);
            }
        }
    }
}


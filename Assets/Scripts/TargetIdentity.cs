using UnityEngine;

public class TargetIdentity : MonoBehaviour
{
    public string targetName;
    public ActionPairManager manager;

    public void NotifyFound()
    {
        Debug.Log($"[TargetIdentity] Found: {targetName}");
        manager.OnTargetFound(targetName, transform);
    }

    public void NotifyLost()
    {
        manager.OnTargetLost(targetName);
    }
}
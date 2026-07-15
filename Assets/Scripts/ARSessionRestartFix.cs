using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;

public class ARSessionRestartFix : MonoBehaviour
{
    public ARSession arSession;

    void Awake()
    {
        if (arSession != null)
        {
            arSession.enabled = false;
        }
    }

    IEnumerator Start()
    {
        yield return null; // wait one frame
        yield return null; // wait a second frame for safety
        if (arSession != null)
        {
            arSession.enabled = true;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class ComboLookup : MonoBehaviour
{
    [SerializeField] private List<ActionPair> pairs = new List<ActionPair>();

    public ActionPair FindComboPair(string idA, string idB)
    {
        foreach (var pair in pairs)
        {
            bool matchesForward = pair.targetA.Trim().ToLower() == idA.Trim().ToLower()
                                && pair.targetB.Trim().ToLower() == idB.Trim().ToLower();

            bool matchesReversed = pair.targetA.Trim().ToLower() == idB.Trim().ToLower()
                                && pair.targetB.Trim().ToLower() == idA.Trim().ToLower();

            if (matchesForward || matchesReversed)
            {
                return pair;
            }
        }
        return null;
    }
}
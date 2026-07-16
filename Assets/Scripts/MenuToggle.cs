using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    [Header("References")]
    public GameObject menuPanel; // the panel containing the card buttons

    private bool isMenuOpen = false;

    void Start()
    {
        SetMenuState(false); // start closed
    }

    // Single method — called by the ONE visible open/close button
    public void ToggleMenu()
    {
        SetMenuState(!isMenuOpen);
    }

    void SetMenuState(bool open)
    {
        isMenuOpen = open;

        if (menuPanel != null)
            menuPanel.SetActive(open);

        Debug.Log($"[MenuToggle] Menu is now {(open ? "OPEN" : "CLOSED")}");
    }
}
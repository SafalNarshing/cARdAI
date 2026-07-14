using UnityEngine;

public class ConsentFlowManager : MonoBehaviour
{
    [Header("Checkbox States")]
    public GameObject checkboxUnchecked;
    public GameObject checkboxChecked;

    [Header("Continue Button States")]
    public GameObject continueGray;
    public GameObject continueBlack;

    private bool isChecked = false;

    void Start()
    {
        SetCheckedState(false);
    }

    // Wire BOTH checkbox_Unchecked's Button.onClick and checkbox_Checked's
    // Button.onClick to this same method in the Inspector.
    public void ToggleCheckbox()
    {
        SetCheckedState(!isChecked);
    }

    void SetCheckedState(bool checkedNow)
    {
        isChecked = checkedNow;

        checkboxUnchecked.SetActive(!isChecked);
        checkboxChecked.SetActive(isChecked);

        continueGray.SetActive(!isChecked);
        continueBlack.SetActive(isChecked);
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Localization.Settings;

public class ComboSelectionManager : MonoBehaviour
{
    [Header("Combo Preview (shown after Combine is pressed)")]
    public GameObject comboPreviewPanel;
    public Image comboPreviewImage;
    public TMP_Text comboPreviewLabel;

    [Header("Buttons")]
    public Button combineButton;
    public Button placeButton;

    [Header("Combo Lookup")]
    public ComboLookup comboLookup;

    private string selectedA = null;
    private string selectedB = null;
    private ComboCardButton lastSelectedButtonA;
    private ComboCardButton lastSelectedButtonB;
    private ActionPair pendingPair;

    void Start()
    {
        comboPreviewPanel.SetActive(false);
        combineButton.gameObject.SetActive(false);
        placeButton.gameObject.SetActive(false);

        combineButton.onClick.AddListener(OnCombineClicked);
        placeButton.onClick.AddListener(OnPlaceClicked);
    }

    public void SelectA(string cardId, Sprite cardSprite, ComboCardButton button)
    {
        if (lastSelectedButtonA != null) lastSelectedButtonA.SetHighlighted(false);
        selectedA = cardId;
        lastSelectedButtonA = button;
        button.SetHighlighted(true);
        CheckReadyToCombine();
    }

    public void SelectB(string cardId, Sprite cardSprite, ComboCardButton button)
    {
        if (lastSelectedButtonB != null) lastSelectedButtonB.SetHighlighted(false);
        selectedB = cardId;
        lastSelectedButtonB = button;
        button.SetHighlighted(true);
        CheckReadyToCombine();
    }

    void CheckReadyToCombine()
    {
        combineButton.gameObject.SetActive(false);
        placeButton.gameObject.SetActive(false);
        comboPreviewPanel.SetActive(false);
        pendingPair = null;

        if (selectedA == null || selectedB == null) return;

        ActionPair pair = comboLookup.FindComboPair(selectedA, selectedB);

        if (pair != null)
        {
            pendingPair = pair;
            combineButton.gameObject.SetActive(true);
        }
    }

    void OnCombineClicked()
    {
        if (pendingPair == null) return;

        bool isNepali = LocalizationSettings.SelectedLocale.Identifier.Code == "ne-NP";

        comboPreviewImage.sprite = pendingPair.previewSprite;
        if (comboPreviewLabel != null)
            comboPreviewLabel.text = isNepali ? pendingPair.resultNameNp : pendingPair.resultNameEn;

        comboPreviewPanel.SetActive(true);

        combineButton.gameObject.SetActive(false);
        placeButton.gameObject.SetActive(true);
    }

    void OnPlaceClicked()
    {
        if (pendingPair == null || string.IsNullOrEmpty(pendingPair.sceneToLoad)) return;

        Debug.Log($"[ComboSelectionManager] Loading scene: {pendingPair.sceneToLoad}");
        SceneManager.LoadScene(pendingPair.sceneToLoad, LoadSceneMode.Single);
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class ARPlacementUIHandler : MonoBehaviour
{
    [Header("References")]
    public ARplace arPlaceScript;          // Reference to the ARplace script

    [Header("Container")]
    [Tooltip("The parent object holding all these UI elements")]
    public GameObject uiContainer;

    [Header("Text Fields")]
    public TextMeshProUGUI nameText;       // Drag the Name Text object here
    public TextMeshProUGUI phonemeText;    // Drag the Phoneme Text object here

    [Header("Buttons")]
    public Button soundButton;             // Sound Icon Button
    public Button closeButton;             // Close (X) Button

    [Header("Audio")]
    public AudioSource audioSource;

    private ARModelData currentData;
    private GameObject currentModel;

    void Start()
    {
        if (uiContainer) uiContainer.SetActive(false); // Hide on start

        if (soundButton) soundButton.onClick.AddListener(PlaySound);
        if (closeButton) closeButton.onClick.AddListener(CloseAndReset);
    }

    public void ShowUIForModel(GameObject model)
    {
        var data = model.GetComponent<ARModelData>();
        if (data == null)
        {
            Debug.LogWarning("Model missing ARModelData script!");
            return;
        }
        // uses data.someField to populate UI
    }

    public void UpdateLanguageContent()
    {
        if (currentData == null) return;

        // Check Locale (Default to English)
        string currentLocale = LocalizationSettings.SelectedLocale?.Identifier.Code ?? "en";
        bool isNepali = currentLocale == "ne-NP" || currentLocale.Contains("ne");

        // 1. Update Name
        if (nameText)
            nameText.text = isNepali ? currentData.displayNameNp : currentData.displayNameEn;

        // 2. Update Phoneme
        if (phonemeText)
            phonemeText.text = isNepali ? currentData.phonemeNp : currentData.phonemeEn;

        // 3. Update Audio Clip
        AudioClip clip = isNepali ? currentData.audioClipNp : currentData.audioClipEn;
        if (audioSource) audioSource.clip = clip;
    }

    void PlaySound()
    {
        if (audioSource && audioSource.clip) audioSource.Play();
    }

    void CloseAndReset()
    {
        // Hide UI
        if (uiContainer) uiContainer.SetActive(false);

        // Destroy Model
        if (currentModel != null) Destroy(currentModel);

        // Allow placing again
        if (arPlaceScript != null) arPlaceScript.ResetPlacement();
    }
}
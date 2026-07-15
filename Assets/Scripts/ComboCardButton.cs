using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;

public class ComboCardButton : MonoBehaviour
{
    public string cardId;
    public Sprite cardSprite;
    public bool isSideA;
    public ComboSelectionManager manager;

    [Header("Highlight")]
    public Image backgroundImage;
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.7f, 1f, 0.7f);

    [Header("Shared Label For This Side")]
    public TMP_Text sharedLabel; // drag the SAME Label object (per side) into all 4 cards on that side
    public string nameEn;
    public string nameNp;

    public void OnClicked()
    {
        UpdateSharedLabel();

        if (isSideA)
            manager.SelectA(cardId, cardSprite, this);
        else
            manager.SelectB(cardId, cardSprite, this);
    }

    void UpdateSharedLabel()
    {
        if (sharedLabel == null) return;
        bool isNepali = LocalizationSettings.SelectedLocale.Identifier.Code == "ne-NP";
        sharedLabel.text = isNepali ? nameNp : nameEn;
    }

    public void SetHighlighted(bool state)
    {
        if (backgroundImage != null)
            backgroundImage.color = state ? selectedColor : normalColor;
    }
}
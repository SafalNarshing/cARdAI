using UnityEngine;
using UnityEngine.UI;

public class PlacementCardButton : MonoBehaviour
{
    public string cardId;
    public GameObject modelPrefab;
    public MultiPlacementManager manager;

    [Header("Highlight")]
    public Image backgroundImage;
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.7f, 1f, 0.7f);

    public void OnClicked()
    {
        manager.SelectCardToPlace(cardId, modelPrefab, this);
    }

    public void SetHighlighted(bool state)
    {
        if (backgroundImage != null)
            backgroundImage.color = state ? selectedColor : normalColor;
    }
}
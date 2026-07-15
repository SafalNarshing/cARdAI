using UnityEngine;

[System.Serializable]
public class ActionPair
{
    public string targetA;
    public string targetB;
    public GameObject comboModel;
    public Sprite previewSprite;
    public string resultNameEn;
    public string resultNameNp;
    public string sceneToLoad; // NEW — e.g. "Brushing_AR_Scene", "Studying_AR_Scene"
}
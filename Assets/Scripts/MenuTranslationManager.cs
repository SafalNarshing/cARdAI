using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class MenuTranslationManager : MonoBehaviour
{
    [Header("UI Text Fields to Translate")]
    public TextMeshProUGUI mainTitleText;     // "Pick a Learning Lesson"
    public TextMeshProUGUI scannerTitle;      // "AR Scanner"
    public TextMeshProUGUI scannerDesc;       // "Scan our card..."
    public TextMeshProUGUI placerTitle;       // "AR Placer"
    public TextMeshProUGUI placerDesc;        // "Place Objects..."
    public TextMeshProUGUI simTitle;          // "AR Simulation"
    public TextMeshProUGUI simDesc;           // "Visualize Activities..."
    public TextMeshProUGUI comboTitle;        // "Combo Cards"              // NEW
    public TextMeshProUGUI comboDesc;
    public TextMeshProUGUI customVoiceTitle;  // "Custom Voice"             // NEW
    public TextMeshProUGUI customVoiceObj;    // "Choose an object"         // NEW
    public TextMeshProUGUI tigerCustomVoice;
    public TextMeshProUGUI elephantCustomVoice;

    [Header("English Strings")]
    public string titleEn = "Pick a Learning Lesson";
    public string scannerTitleEn = "AR Scanner";
    public string scannerDescEn = "Scan our card to learn words";
    public string placerTitleEn = "AR Placer";
    public string placerDescEn = "Place Objects on your own Environment";
    public string simTitleEn = "AR Simulation";
    public string simDescEn = "Visualize Activities with interactions";
    public string comboTitleEn = "Combo Cards";              // NEW
    public string comboDescEn = "Combine two cards and see what happens"; // NEW
    public string customVoiceTitleEn = "Custom Voice";        // NEW
    public string customVoiceObjEn = "Choose an object";
    public string tigerCustomVoiceEn = "Tiger";
    public string elephantCustomVoiceEn = "Elephant";

    [Header("Nepali Strings")]
    public string titleNp = "एउटा पाठ छान्नुहोस्"; // "Pick a lesson"
    public string scannerTitleNp = "एआर स्क्यानर";  // "AR Scanner"
    public string scannerDescNp = "शब्दहरू सिक्न हाम्रो कार्ड स्क्यान गर्नुहोस्";
    public string placerTitleNp = "एआर प्लेसमेन्ट"; // "AR Placement"
    public string placerDescNp = "आफ्नो वातावरणमा वस्तुहरू राख्नुहोस्";
    public string simTitleNp = "एआर सिमुलेशन";   // "AR Simulation"
    public string simDescNp = "अन्तरक्रियाका साथ गतिविधिहरू हेर्नुहोस्";
    public string comboTitleNp = "संयोजन कार्डहरू";           // NEW — replace with your real translation
    public string comboDescNp = "दुई कार्डहरू मिलाएर के हुन्छ हेर्नुहोस्"; // NEW — replace with your real translation
    public string customVoiceTitleNp = "अनुकूलित आवाज";      // NEW — replace with your real translation
    public string customVoiceObjNp = "वस्तु छान्नुहोस्"; // NEW — replace with your real translation
    public string tigerCustomVoiceNp = "बाघ"; // NEW — replace with your real translation
    public string elephantCustomVoiceNp = "हात्ती"; // NEW — replace with your real translation

    IEnumerator Start()
    {
        // Wait for Localization system to initialize
        yield return LocalizationSettings.InitializationOperation;

        // update text immediately on start
        UpdateAllText();

        // Subscribe to event: Run this function whenever language changes
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent errors
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
    }

    void OnLanguageChanged(UnityEngine.Localization.Locale locale)
    {
        UpdateAllText();
    }

    void UpdateAllText()
    {
        // Check current code (e.g., "en" or "ne-NP")
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;
        bool isNepali = (code == "ne-NP");

        // Update Title
        if (mainTitleText) mainTitleText.text = isNepali ? titleNp : titleEn;

        // Update Cards
        if (scannerTitle) scannerTitle.text = isNepali ? scannerTitleNp : scannerTitleEn;
        if (scannerDesc) scannerDesc.text = isNepali ? scannerDescNp : scannerDescEn;

        if (placerTitle) placerTitle.text = isNepali ? placerTitleNp : placerTitleEn;
        if (placerDesc) placerDesc.text = isNepali ? placerDescNp : placerDescEn;

        if (simTitle) simTitle.text = isNepali ? simTitleNp : simTitleEn;
        if (simDesc) simDesc.text = isNepali ? simDescNp : simDescEn;

        if (comboTitle) comboTitle.text = isNepali ? comboTitleNp : comboTitleEn; // NEW
        if (comboDesc) comboDesc.text = isNepali ? comboDescNp : comboDescEn;     // NEW

        if (customVoiceTitle) customVoiceTitle.text = isNepali ? customVoiceTitleNp : customVoiceTitleEn; // NEW

        if (customVoiceObj) customVoiceObj.text = isNepali ? customVoiceObjNp : customVoiceObjEn;         // NEW
        
        if (tigerCustomVoice) tigerCustomVoice.text = isNepali ? tigerCustomVoiceNp : tigerCustomVoiceEn; // NEW

        if (elephantCustomVoice) elephantCustomVoice.text = isNepali ? elephantCustomVoiceNp : elephantCustomVoiceEn; // NEW
    }


}
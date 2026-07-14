using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class Translation_TandC : MonoBehaviour
{
    [Header("UI Text Fields to Translate")]
    public TextMeshProUGUI mainTitleText;   // "Terms and Conditions"
    public TextMeshProUGUI termsDesc;       // Full consent body text
    public TextMeshProUGUI agreeConsent;    // Checkbox agreement line
    public TextMeshProUGUI continueButton;  // "Continue"

    [Header("English Strings")]
    public string mainTitleTextEn = "Terms and Conditions";
    public string termsDescEn =
        "Before Your Child Uses cARd AI\r\n\r\n" +
        "This app should be used with adult supervision. By continuing, you confirm you are the child's\r\n" +
        "parent, legal guardian, or an authorized therapist/educator, and you\r\n" +
        "agree on behalf of both yourself and the child.\r\n\r\n" +
        "CAMERA: Used only to scan physical vocabulary cards in real time.\r\n" +
        "Nothing is recorded or saved from the camera.\r\n\r\n" +
        "MICROPHONE: Used only if you choose to record your own voice for a\r\n" +
        "card (\"My Voice\"). Recordings are stored on this device only — never\r\n" +
        "uploaded or shared.\r\n\r\n" +
        "WHAT WE COLLECT: Your child's age group, and, if you choose to add it,\r\n" +
        "a developmental classification to personalize content. This\r\n" +
        "information is never sold, shared, or used for advertising.\r\n\r\n" +
        "NOT A MEDICAL DEVICE: cARd AI is a vocabulary-learning tool. It does\r\n" +
        "not diagnose, treat, or replace professional speech, occupational, or\r\n" +
        "behavioral therapy.\r\n\r\n" +
        "Full Terms & Privacy Policy: synaptic.framer.ai/legal";
    public string agreeConsentEn = "I am the parent, legal guardian, or authorized professional for this child, and I agree to the above.\r\n";
    public string continueButtonEn = "Continue";

    [Header("Nepali Strings")]
    public string mainTitleTextNp = "नियम र शर्तहरू";
    public string termsDescNp =
        "तपाईंको बच्चाले cARd AI प्रयोग गर्नु अघि\r\n\r\n" +
        "यो एप वयस्क निगरानीमा प्रयोग गरिनुपर्छ। जारी राख्नुभएर, तपाईं पुष्टि गर्नुहुन्छ कि तपाईं बच्चाको\r\n" +
        "अभिभावक, कानुनी संरक्षक, वा अधिकृत चिकित्सक/शिक्षक हुनुहुन्छ, र\r\n" +
        "तपाईं आफ्नो र बच्चा दुवैको तर्फबाट सहमत हुनुहुन्छ।\r\n\r\n" +
        "क्यामेरा: वास्तविक समयमा भौतिक शब्दावली कार्डहरू स्क्यान गर्न मात्र प्रयोग गरिन्छ।\r\n" +
        "क्यामेराबाट केही पनि रेकर्ड वा सेभ गरिँदैन।\r\n\r\n" +
        "माइक्रोफोन: कार्डको लागि तपाईंको आफ्नै आवाज रेकर्ड गर्न रोज्नुभयो भने मात्र प्रयोग गरिन्छ\r\n" +
        "(\"माई भ्वाइस\")। रेकर्डिङहरू यस उपकरणमा मात्र भण्डारण गरिन्छ — कहिल्यै\r\n" +
        "अपलोड वा साझा गरिँदैन।\r\n\r\n" +
        "हामीले के संकलन गर्छौं: तपाईंको बच्चाको उमेर समूह, र, तपाईंले थप्न रोज्नुभयो भने,\r\n" +
        "सामग्री व्यक्तिगत बनाउन विकासात्मक वर्गीकरण। यो\r\n" +
        "जानकारी कहिल्यै बेचिँदैन, साझा गरिँदैन, वा विज्ञापनको लागि प्रयोग गरिँदैन।\r\n\r\n" +
        "मेडिकल उपकरण होइन: cARd AI एक शब्दावली-सिकाइ उपकरण हो। यसले\r\n" +
        "व्यावसायिक वाणी, व्यावसायिक, वा व्यावहारिक थेरापीको निदान, उपचार, वा प्रतिस्थापन गर्दैन।\r\n\r\n" +
        "पूर्ण नियम र गोपनीयता नीति: synaptic.framer.ai/legal";
    public string agreeConsentNp = "म यस बच्चाको अभिभावक, कानुनी संरक्षक, वा अधिकृत पेशेवर हुँ, र माथिको कुरामा सहमत छु।\r\n";
    public string continueButtonNp = "जारी राख्नुहोस्";

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

        if (mainTitleText) mainTitleText.text = isNepali ? mainTitleTextNp : mainTitleTextEn;
        if (termsDesc) termsDesc.text = isNepali ? termsDescNp : termsDescEn;
        if (agreeConsent) agreeConsent.text = isNepali ? agreeConsentNp : agreeConsentEn;
        if (continueButton) continueButton.text = isNepali ? continueButtonNp : continueButtonEn;
    }
}
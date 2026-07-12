using UnityEngine;
using Vuforia;

[RequireComponent(typeof(ScannerUIHandler))]
public class TigerVoiceOverride : MonoBehaviour
{
    [Header("Card Identity")]
    public string cardId = "tiger";

    [Header("Audio")]
    public AudioSource playbackAudioSource;

    private ScannerUIHandler scannerHandler;
    private ObserverBehaviour observerBehaviour;
    private bool pendingOverride = false;

    private void Start()
    {
        scannerHandler = GetComponent<ScannerUIHandler>();
        observerBehaviour = GetComponent<ObserverBehaviour>();

        if (observerBehaviour)
        {
            observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    private void OnDestroy()
    {
        if (observerBehaviour)
        {
            observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        bool isTracked =
            targetStatus.Status == Status.TRACKED ||
            targetStatus.Status == Status.EXTENDED_TRACKED;

        if (isTracked)
        {
            // Wait one frame so ScannerUIHandler's own OnTargetFound()
            // has already run and wired its default listener first.
            pendingOverride = true;
        }
    }

    private void Update()
    {
        if (pendingOverride)
        {
            pendingOverride = false;
            OverrideSoundButton();
        }
    }

    private void OverrideSoundButton()
    {
        if (scannerHandler == null || scannerHandler.soundButton == null) return;

        scannerHandler.soundButton.onClick.RemoveAllListeners();
        scannerHandler.soundButton.onClick.AddListener(() =>
        {
            // Check language fresh every time the button is tapped,
            // not just once when tracking started
            bool isNepali = UnityEngine.Localization.Settings.LocalizationSettings
                .SelectedLocale?.Identifier.Code == "ne-NP";
            string lang = isNepali ? "nep" : "en";

            if (VoiceRecorder.Instance.HasRecording(cardId, lang))
            {
                VoiceRecorder.Instance.PlayRecording(cardId, lang, playbackAudioSource);
            }
            else
            {
                Debug.Log($"No custom recording for {cardId} ({lang})");
            }
        });
    }
}
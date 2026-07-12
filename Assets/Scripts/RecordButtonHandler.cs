using UnityEngine;
using UnityEngine.UI;

public class RecordButtonHandler : MonoBehaviour
{
    [Header("Card Identity")]
    public string cardId = "tiger";       // hardcoded for demo, generalize later
    public string language = "en";        // "en" or "nep"

    [Header("UI References")]
    public Image cardImage;               // the whole card's Image component
    public Sprite recordingCardSprite;    // only the "Recording..." version needed

    private Sprite originalSprite;        // captured automatically at start
    private bool isCurrentlyRecording = false;

    private void Start()
    {
        originalSprite = cardImage.sprite; // whatever is already showing becomes "idle"
    }

    public void OnCardClicked()
    {
        Debug.Log($"[RecordButtonHandler] Clicked: {cardId} ({language}), isRecording={isCurrentlyRecording}");

        if (!isCurrentlyRecording)
        {
            StartRecordingUI();
        }
        else
        {
            StopRecordingUI();
        }
    }

    private void StartRecordingUI()
    {
        VoiceRecorder.Instance.StartRecording(cardId, language);
        isCurrentlyRecording = true;

        cardImage.sprite = recordingCardSprite;
    }

    private void StopRecordingUI()
    {
        VoiceRecorder.Instance.StopRecording(cardId, language);
        isCurrentlyRecording = false;

        cardImage.sprite = originalSprite;
    }
}
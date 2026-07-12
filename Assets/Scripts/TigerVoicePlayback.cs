using UnityEngine;

public class TigerVoicePlayback : MonoBehaviour
{
    public AudioSource audioSource;
    public string cardId = "tiger";
    public string language = "en";

    public void OnTigerFound()
    {
        if (VoiceRecorder.Instance.HasRecording(cardId, language))
        {
            VoiceRecorder.Instance.PlayRecording(cardId, language, audioSource);
        }
        else
        {
            Debug.Log("No custom voice recorded for tiger — falling back to default.");
        }
    }
}
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class VoiceRecorder : MonoBehaviour
{
    public static VoiceRecorder Instance;

    private AudioClip recordingClip;
    private string currentMicDevice;
    private bool isRecording = false;
    private const int MAX_RECORD_SECONDS = 10;
    private const int SAMPLE_RATE = 44100;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool IsRecording => isRecording;

    public void StartRecording(string cardId, string language)
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("No microphone found on this device.");
            return;
        }

        currentMicDevice = Microphone.devices[0];
        recordingClip = Microphone.Start(currentMicDevice, false, MAX_RECORD_SECONDS, SAMPLE_RATE);
        isRecording = true;
        Debug.Log($"[VoiceRecorder] Started recording for {cardId} ({language})");
    }

    public void StopRecording(string cardId, string language)
    {
        if (!isRecording) return;

        int lastSamplePos = Microphone.GetPosition(currentMicDevice);
        Microphone.End(currentMicDevice);
        isRecording = false;

        // Trim clip to actual recorded length
        AudioClip trimmedClip = TrimClip(recordingClip, lastSamplePos);
        SaveWav(trimmedClip, cardId, language);

        Debug.Log($"[VoiceRecorder] Stopped and saved recording for {cardId} ({language})");
    }

    private AudioClip TrimClip(AudioClip clip, int samplePos)
    {
        if (samplePos <= 0) samplePos = clip.samples;

        float[] data = new float[samplePos * clip.channels];
        clip.GetData(data, 0);

        AudioClip trimmed = AudioClip.Create(clip.name, samplePos, clip.channels, clip.frequency, false);
        trimmed.SetData(data, 0);
        return trimmed;
    }

    private void SaveWav(AudioClip clip, string cardId, string language)
    {
        string folder = Path.Combine(Application.persistentDataPath, "voices");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        string fileName = $"{cardId}_{language}.wav";
        string path = Path.Combine(folder, fileName);

        WavUtility.Save(path, clip);
    }

    public bool HasRecording(string cardId, string language)
    {
        string path = Path.Combine(Application.persistentDataPath, "voices", $"{cardId}_{language}.wav");
        return File.Exists(path);
    }

    public void PlayRecording(string cardId, string language, AudioSource source)
    {
        string path = Path.Combine(Application.persistentDataPath, "voices", $"{cardId}_{language}.wav");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"No recording found for {cardId} ({language})");
            return;
        }

        StartCoroutine(LoadAndPlay(path, source));
    }

    private IEnumerator LoadAndPlay(string path, AudioSource source)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                source.clip = clip;
                source.Play();
            }
            else
            {
                Debug.LogError($"Failed to load recording: {www.error}");
            }
        }
    }
}
using System;
using System.IO;
using UnityEngine;

public class TBRecorder : MonoBehaviour
{
    [SerializeField]
    private AudioSource currentAudioSource;
    [SerializeField] 
    private TBRecorderConfig recorderConfig;
    [SerializeField] 
    private bool enableDebug = false;

    public void Start()
    {
        if (currentAudioSource != null) return;
        currentAudioSource = GetComponent<AudioSource>();
    }

    public void StartRecording()
    {
        Debug.Log("Start Recording");
        for (int i = 0; i < Microphone.devices.Length; i++)
        {
            Debug.Log(Microphone.devices[i]);
        }
        Microphone.GetDeviceCaps(null,out var frequency, out var maxFrequency);
        frequency = maxFrequency < recorderConfig.defaultFrequency ? maxFrequency : recorderConfig.defaultFrequency;
        currentAudioSource.clip = Microphone.Start(null, true, recorderConfig.recordingDuration, frequency);
        currentAudioSource.Play();
    }

    public void StopRecording()
    {
        if(!Microphone.IsRecording(null)) return;
        Debug.Log("Stop Recording");
        Microphone.End(null);
    }

    public void StartPlaying()
    {
        Debug.Log("Start Playing");
        currentAudioSource.Play();
    }

    public void SaveFile(string filename)
    {
        string filePath = Path.Combine(Application.persistentDataPath, $"{filename}.wav");
        Debug.Log($"Saving file to: {filePath}");
        try
        {
            SavWav.Save(filePath, currentAudioSource.clip);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Fail to save file. {exception}");
        }
    }
    
    void OnGUI()   
    {
        if(!enableDebug) return;
        if (Microphone.IsRecording(null))
        {
            if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), "Stop and Play!"))
            {
                StopRecording();
                SaveFile(recorderConfig.filename);
                StartPlaying();
            }
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 25, 200, 50), "Recording in progress...");
            return;
        }
        
        if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), "Record"))
        {
            StartRecording();
        }
    }  
}

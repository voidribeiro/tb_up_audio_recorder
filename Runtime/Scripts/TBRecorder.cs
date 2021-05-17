using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class TBRecorder : MonoBehaviour
{
    [SerializeField]
    private AudioSource currentAudioSource;
    [SerializeField] 
    private TBRecorderConfig recorderConfig;
    [SerializeField] 
    private bool enableDebug = false;

    public UnityEvent OnFinishPlaying;
    private bool _isPlaying;
    private float _startRecordingTime;

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
        currentAudioSource.clip = Microphone.Start(null, false, recorderConfig.recordingDuration, frequency);
        currentAudioSource.Play();
        _startRecordingTime = Time.time;
    }

    public void StopRecording()
    {
        if(!Microphone.IsRecording(null)) return;
        Debug.Log("Stop Recording");
        Microphone.End(null);
        float deltaTime = Time.time - _startRecordingTime;
        
        if (deltaTime >= recorderConfig.recordingDuration) return;
        
        AudioClip newClip = currentAudioSource.clip;
        float lengthL = newClip.length;
        float samplesL = newClip.samples;
        float samplesPerSec = samplesL/lengthL;
        float[] samples = new float[(int)(samplesPerSec * deltaTime)];
        newClip.GetData(samples,0);
            
        currentAudioSource.clip = AudioClip.Create("RecordedSound",(int)(deltaTime*samplesPerSec),1,recorderConfig.defaultFrequency,false,false);
        currentAudioSource.clip.SetData(samples,0);
    }

    public void StartPlaying()
    {
        Debug.Log("Start Playing");
        Invoke("OnEndPlaying", currentAudioSource.clip.length);
        currentAudioSource.Play();
        _isPlaying = true;
    }

    public void StopPlaying()
    {
        if(!_isPlaying) return;
        Debug.Log("Stop Playing");
        currentAudioSource.Stop();
        OnEndPlaying();
    }

    public void OnEndPlaying()
    {
        if(!_isPlaying) return;
        _isPlaying = false;
        OnFinishPlaying?.Invoke();
    }

    public void LoadFile(bool play = true)
    {
        LoadFile(recorderConfig.filename, play);
    }

    public void LoadFile(string filename, bool play = true)
    {
        string filePath = Path.Combine(Application.persistentDataPath, $"{filename}.wav");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Fail to find file in path {filePath}");
            return;
        }
        StartCoroutine(loadAudio($"file://{filePath}", play));
    }
    
    IEnumerator loadAudio(string path, bool play)
    {
        using (UnityWebRequest loadFileRequest = UnityWebRequestMultimedia.GetAudioClip(path,AudioType.WAV))
        {
            yield return loadFileRequest.SendWebRequest();                
            if (loadFileRequest.result == UnityWebRequest.Result.Success){
                currentAudioSource.clip = DownloadHandlerAudioClip.GetContent(loadFileRequest);
                if (play)
                {
                    StartPlaying();
                }
                yield break;
                
            }
            Debug.Log($"Fail to load audio file. {loadFileRequest.error}");
        }        
    }

    public void SaveFile()
    {
        SaveFile(recorderConfig.filename);
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
        if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 4 - 25, 200, 50), "Play"))
        {
            LoadFile();
            OnFinishPlaying.AddListener(DebugFinishPlaying);
        }
        
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

    private void DebugFinishPlaying()
    {
        Debug.Log("Debug Finish Playing");
    }
}

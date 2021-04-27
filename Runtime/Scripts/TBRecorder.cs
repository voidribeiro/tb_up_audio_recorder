using UnityEngine;

public class TBRecorder : MonoBehaviour
{
    [SerializeField]
    private AudioSource currentAudioSource;
    private const int RecordingDuration = 10;
    private const int DefaultFrequency = 44100;
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
        int frequency;
        int maxFrequency;
        Microphone.GetDeviceCaps(null,out frequency, out maxFrequency);
        frequency = maxFrequency < DefaultFrequency ? maxFrequency : DefaultFrequency;
        currentAudioSource.clip = Microphone.Start(null, true, RecordingDuration, frequency);
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
    
    void OnGUI()   
    {
        if(!enableDebug) return;
        if (Microphone.IsRecording(null))
        {
            if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), "Stop and Play!"))
            {
                StopRecording();
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

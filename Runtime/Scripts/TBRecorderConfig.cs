using UnityEngine;

[CreateAssetMenu(fileName = "TBRecorderConfig", menuName = "Config/TBRecorderConfig")]
public class TBRecorderConfig : ScriptableObject
{
    public int recordingDuration = 10;
    public int defaultFrequency = 44100;
    public string filename;
}

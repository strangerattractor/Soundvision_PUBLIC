using cylvester;
using UnityEngine;
using UnityEngine.UI;

public class MidiSyncCounter : MonoBehaviour
{

    [SerializeField] private Text counter;

    private int count_ = 0;

    public void OnSyncReceived(MidiSync midiSync)
    {
        if (midiSync == MidiSync.Clock)
        {
            count_++;
            var tick = count_ % 24;
            var beat = count_ / 24;
            var measure = beat / 4;

            counter.text = (measure + 1) + ":" + (beat % 4 + 1) + ":" + tick;
        }
        else if (midiSync == MidiSync.Start)
        {
            Debug.Log("playback started");
            count_ = 0;
        }
        else if (midiSync == MidiSync.Stop)
        {
            Debug.Log("playback ended");
        }
    }
}

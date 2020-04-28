using cylvester;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MidiSyncCounterTMP : MonoBehaviour
{

    [SerializeField] private TextMeshPro counter;
    


    public void OnSyncReceived(MidiSync midiSync, int count)
    {
        if (midiSync == MidiSync.Clock)
        {
            var tick = count % 24;
            var beat = count / 24;
            var measure = beat / 4;

            counter.text = (measure + 1) + ":" + (beat % 4 + 1) + ":" + tick;
        }
        else if (midiSync == MidiSync.Start)
        {
            Debug.Log("playback started");
        }
        else if (midiSync == MidiSync.Stop)
        {
            Debug.Log("playback ended");
        }
    }
}


using UnityEngine;
using UnityEngine.UI;

public class MidiClockCounter : MonoBehaviour
{

    [SerializeField] private Text counter;

    private int count_ = 0;

    public void onClockReceived()
    {
        count_++;
        var tick = count_ % 24;
        var beat = count_ / 24;
        var measure = beat / 4;

        counter.text = (measure+1) + ":" + (beat%4+1) + ":" + tick;
    }
}

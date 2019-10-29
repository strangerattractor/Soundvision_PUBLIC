using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [System.Serializable]
    public class TimbreIdEvent : UnityEvent<int, int>
    {
    }
    
    public class PdTimbreIdBind : MonoBehaviour
    {
        [SerializeField] private TimbreIdEvent onTimbreIdReceived;
        [SerializeField, Range(1, 16)] private int channel = 1;
        
        public void OnMidiMessageReceived(MidiMessage midiMessage)
        {
            if (159 + channel != midiMessage.Status)
                return;

            onTimbreIdReceived.Invoke(midiMessage.Data1, midiMessage.Data2);
        }
    }
}

using UnityEngine;

namespace cylvester
{
    public class MidiLogger : MonoBehaviour
    {
        [SerializeField] private bool logAll;
        [SerializeField] private bool logFiltered;
        [SerializeField, Range(128, 255)] private int filterStatusByte = 128;
        
        public void OnMidiMessageReceived(MidiMessage mes)
        {

            if(logAll)
                Debug.Log("MIDI Received: " + mes);

            if(logFiltered)
            {
                if (mes.Status == filterStatusByte)
                {
                    Debug.Log("Filtered MIDI Received: " + mes);
                }
            }
        }
    }
}

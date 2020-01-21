using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    public class MidiSyncedLoop : MonoBehaviour
    {
        [SerializeField, Range(1, 128)] private int loopLengthInBeat = 1;
        [SerializeField] private UnityEvent loopStarted;

        public void OnSyncReceived(MidiSync midiSync, int counter)
        {
            var loopLengthTicks = loopLengthInBeat * 24;
            if(counter % loopLengthTicks == 0)
                loopStarted.Invoke();
        }
    }

}



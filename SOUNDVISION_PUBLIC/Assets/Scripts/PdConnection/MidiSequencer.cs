using System;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [Serializable]
    class UnitySequenceEvent : UnityEvent<int>
    { }

    public class MidiSequencer : MonoBehaviour
    {
        [SerializeField] private bool[] sequence = new bool[24]; // max 6 beat
        [SerializeField] private int time = 4;
        [SerializeField] private UnitySequenceEvent triggered;
        
        private void OnValidate()
        {
            if(sequence.Length != 24)
                Array.Resize(ref sequence, 24);
        }

        public void OnSyncReceived(MidiSync midiSync, int count)
        {
            if (count % 6 != 0) // 24 ppqn (pulse per quater note). %6 means every 16th note because 24/4
                return;
            
            var index = count /6 % (time * 4);
            if (sequence[index])
                triggered.Invoke(index);
        }
    }
}

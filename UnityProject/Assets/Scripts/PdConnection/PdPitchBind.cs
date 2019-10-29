using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [System.Serializable]
    public class PitchEvent : UnityEvent<float>
    {
    }
    
    public class PdPitchBind : MonoBehaviour
    {
        [SerializeField] private PdBackend pdbackend;
        [SerializeField, Range(1, 16)] private int channel = 1;
        [SerializeField] private LevelEvent pitchChanged;
        private float pitch_;
        
        void Update()
        {
            var pitch = pdbackend.PitchArray.Data[channel - 1];

            if (pitch_ != pitch)
            {
                pitch_ = pitch;
                pitchChanged.Invoke(pitch_);
            }
        }
    }
}
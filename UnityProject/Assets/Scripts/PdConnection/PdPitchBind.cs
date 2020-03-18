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
    
    public class PdPitchBind : PdBaseBind
    {
        [SerializeField] private LevelEvent pitchChanged;
        [SerializeField] bool logPitch;
        private float pitch_;
        
        void Update()
        {
            var pitch = pdbackend.PitchArray.Data[channel - 1];

            if (pitch_ != pitch)
            {
                pitch_ = pitch;
                pitchChanged.Invoke(pitch_);
                if (logPitch)
                {
                    Debug.Log("Pitch: " + pitch);
                }
            }
        }
    }
}
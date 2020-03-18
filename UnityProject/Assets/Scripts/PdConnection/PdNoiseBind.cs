using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [System.Serializable]
    public class NoiseEvent : UnityEvent<float>
    {
    }
    
    public class PdNoiseBind : PdBaseBind
    {
        [SerializeField] private NoiseEvent noiseLevelChanged;
        private float noise_;
        
        void Update()
        {
            var noise = pdbackend.NoiseArray.Data[channel - 1];

            if (noise_ != noise)
            {
                noise_ = noise;
                noiseLevelChanged.Invoke(noise_);
            }
        }
    }
}
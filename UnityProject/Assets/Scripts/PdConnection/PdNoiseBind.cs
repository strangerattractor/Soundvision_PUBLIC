using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [System.Serializable]
    public class NoiseEvent : UnityEvent<float>
    {
    }
    
    public class PdNoiseBind : MonoBehaviour
    {
        [SerializeField] private PdBackend pdbackend;
        [SerializeField, Range(1, 16)] private int channel = 1;
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
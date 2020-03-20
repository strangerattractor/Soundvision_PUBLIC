using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [System.Serializable]
    public class FrequencyBandEvent : UnityEvent<float>
    {
    }
    
    public class PdFrequencyBandBind : PdBaseBind
    {
        [SerializeField] private FrequencyBandEvent frequencyBandChanged;
        [SerializeField] private int numBins = 8;
        [SerializeField] private int targetBin = 0;
        [SerializeField] bool logLevel;
        private float level_;

        private IPdArray spectrumArray_;

        void Update()
        {
            spectrumArray_ = pdbackend.SpectrumArrayContainer[channel - 1];
            float total = 0;
            float[] bins = new float[numBins];
            for (int i = 0; i < bins.Length; i++)
            {
                bins[i] = 0;
            }
            for (int i = 0; i < spectrumArray_.Data.Length; i++)
            {
                bins[i * bins.Length / spectrumArray_.Data.Length] += spectrumArray_.Data[i] / spectrumArray_.Data.Length;
            }
            for (int i = 0; i < bins.Length; i++)
            {
                total += bins[i];
            }
            for (int i = 0; i < bins.Length; i++)
            {
                bins[i] /= total;
            }

            level_ = bins[targetBin];
            frequencyBandChanged.Invoke(level_);

            if (logLevel)
            { 
            Debug.Log("Level:" + level_);
            }
        }
    }
}
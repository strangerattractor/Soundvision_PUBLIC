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
    
    public class PdFrequencyBandBind : MonoBehaviour
    {
        [SerializeField] private PdBackend pdBackend = null;
        [SerializeField] private EnergyChangeEvent energyChanged = null;
        [SerializeField] private int channel = 0;
        [SerializeField] private FrequencyBandEvent frequencyBandChanged;
        [SerializeField] private int numBins = 8;
        [SerializeField] private int targetBin = 0;
        [SerializeField, Range(0, 255)] private int inputMin = 0;
        [SerializeField, Range(0, 255)] private int inputMax = 255;
        [SerializeField] bool renderSpectrum = false;
        private float level_;

        private IFrequencyBandGenerator spectrumGenerator_;
        private IPdArraySelector arraySelector_;

        public int TextureWidth { get; } = 512;
        public int TextureHeight { get; } = 256;
        public Texture2D Spectrum => spectrumGenerator_.Spectrum;
        public float Energy { get; private set; }

        private IPdArray spectrumArray_;

        private void Start()
        {
            if (pdBackend == null)
            {
                var pdBackendObjects = FindObjectsOfType<PdBackend>();
                if (pdBackendObjects.Length > 0)
                {
                    var g = pdBackendObjects[0].gameObject;
                    pdBackend = g.GetComponent<PdBackend>();
                }
            }
            arraySelector_ = new PdArraySelector(pdBackend.SpectrumArrayContainer);
            spectrumGenerator_ = new FrequencyBandGeneratorPlayMode(TextureWidth, TextureHeight, arraySelector_);
        }

        void Update()
        {
            spectrumArray_ = pdBackend.SpectrumArrayContainer[channel];
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

            arraySelector_.Selection = channel;
            if (renderSpectrum)
            {
                spectrumGenerator_.SetBins(bins);
                int rectw = TextureWidth / bins.Length;
                int rectx = targetBin * rectw;
                int recth = ((TextureHeight-inputMin) - (TextureHeight - inputMax));
                int recty = (TextureHeight - inputMax) - 0;
                // flip top and bottom
                //int recty = recty_ + recth_;
                //int recth = -recty_;
                spectrumGenerator_.Update(rectx, recty, rectw, recth);
            }

            level_ = Mathf.Clamp01((bins[targetBin] * TextureHeight - inputMin) / (inputMax - inputMin));
            frequencyBandChanged.Invoke(level_);
            Energy = level_;
        }
    }
}
// using System.Collections;
// using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [System.Serializable]
    class FrequencyBandEvent : UnityEvent<float> { }

    public class PdFrequencyBandBind : MonoBehaviour
    {
        [SerializeField] private PdBackend pdBackend = null;
        [SerializeField] private Rect selection = Rect.zero;
        [SerializeField] private FrequencyBandEvent energyChanged = null;
        [SerializeField] private int channel = 0;
        [SerializeField] bool renderSpectrum = false;
        [SerializeField] bool logScale = false;
        [SerializeField, Range(1, 100)] float gain = 20.0f;

        private IFrequencyBandGenerator spectrumGenerator_;
        private IPdArraySelector arraySelector_;

        public int TextureWidth { get; } = 512;
        public int TextureHeight { get; } = 256;
        public Texture2D Spectrum => spectrumGenerator_.Spectrum;
        public int Energy { get; private set; }

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

        private void Update()
        {
            arraySelector_.Selection = channel;
            var energy = spectrumGenerator_.Update(selectionRect: selection, drawFullSpectrum: renderSpectrum, gain: gain, logScale: logScale);
            if (energy == Energy)
                return;
            Energy = energy;
            energyChanged.Invoke(Energy);
        }
    }
}

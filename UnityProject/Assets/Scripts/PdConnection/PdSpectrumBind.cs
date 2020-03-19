using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [System.Serializable]
    class EnergyChangeEvent : UnityEvent<float> { }
    
    public class PdSpectrumBind : MonoBehaviour
    {
        [SerializeField] protected PdBackend pdbackend;
        [SerializeField, Range(1, 16)] protected int channel = 1;
        [SerializeField] private Rect selection = Rect.zero;
        [SerializeField] private EnergyChangeEvent energyChanged = null;

        private ISpectrumGenerator spectrumGenerator_;
        private IPdArraySelector arraySelector_;

        public int TextureWidth { get; } = 512;
        public int TextureHeight { get; } = 256;
        public Texture2D Spectrum => spectrumGenerator_.Spectrum;
        public int Energy { get; private set; }

        private void Start()
        {
            //base.Start();
            arraySelector_ = new PdArraySelector(pdbackend.SpectrumArrayContainer);
            spectrumGenerator_ = new SpectrumGeneratorPlayMode(TextureWidth, TextureHeight, arraySelector_);
        }
        
        private void Update()
        {
            arraySelector_.Selection = channel - 1;
            var energy = spectrumGenerator_.Update(selection);
            if (energy == Energy)
                return;
            Energy = energy;
            energyChanged.Invoke(Energy);
        }
    }
}
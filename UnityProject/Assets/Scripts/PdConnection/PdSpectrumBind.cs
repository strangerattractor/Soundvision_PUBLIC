using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [System.Serializable]
    class UnityFloatEvent : UnityEvent<float> { }
    

    public class PdSpectrumBind : MonoBehaviour
    {
        [SerializeField] private PdBackend pdBackend;
        [SerializeField] private Rect selection;
        [SerializeField] private UnityFloatEvent energyChanged;
        [SerializeField] private int channel = 0;

        private ISpectrumGenerator spectrumGenerator_;
        private ISpectrumArraySelector arraySelector_;

        public int TextureWidth { get; } = 512;
        public int TextureHeight { get; } = 256;
        public Texture2D Spectrum => spectrumGenerator_.Spectrum;
        public int Energy { get; private set; }

        private void Start()
        {
            arraySelector_ = new SpectrumArraySelector(pdBackend.spectrumArrayContainer);
            spectrumGenerator_ = new SpectrumGeneratorPlayMode(TextureWidth, TextureHeight, arraySelector_);
        }
        
        private void Update()
        {
            arraySelector_.Selection = channel;
            var energy = spectrumGenerator_.Update(selection);
            if (energy == Energy)
                return;
            Energy = energy;
            energyChanged.Invoke(Energy);
        }
    }
}
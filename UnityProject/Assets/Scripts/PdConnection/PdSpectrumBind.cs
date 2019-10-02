using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [System.Serializable]
    class UnityFloatEvent : UnityEvent<float> { }
    
    public interface IPdSpectrumBind
    {
        int Channel { get; set; }
        int Energy { get; }
        int TextureWidth { get; }
        int TextureHeight { get; }
        Texture2D Spectrum { get; }
    }
    
    public class PdSpectrumBind : MonoBehaviour, IPdSpectrumBind
    {
        [SerializeField] private PdBackend pdBackend;
        [SerializeField] private Rect selection;
        [SerializeField] private UnityFloatEvent energyChanged;

        private ISpectrumGenerator spectrumGenerator_;

        private void Start()
        {
            var spectrumArray = pdBackend.fftArrayContainer[Channel];
            spectrumGenerator_ = new SpectrumGeneratorPlayMode(TextureWidth, TextureHeight, spectrumArray);
        }

        public int TextureWidth { get; } = 512;
        public int TextureHeight { get; } = 256;
        public Texture2D Spectrum => spectrumGenerator_.Spectrum;
        public int Channel { get; set; }
        public int Energy { get; private set; }

        private void Update()
        {
            var energy = spectrumGenerator_.Update(selection);
            if (energy == Energy)
                return;
            Energy = energy;
            energyChanged.Invoke(Energy);
        }
    }
}
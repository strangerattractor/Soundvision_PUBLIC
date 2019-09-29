using UnityEngine;

namespace cylvester
{
    public interface IPdSpectrumBind
    {
        IPdArray PdArray { get; }
        int Energy { get; set; }
    }
    
    [ExecuteInEditMode]
    public class PdSpectrumBind : MonoBehaviour, IPdSpectrumBind
    {
        public int channel;
        public int startBin;
        public int endBin;
        public float topClip;
        public float bottomClip;
        
        private PdArray pdArray_;

        private void Awake()
        {
            pdArray_ = new PdArray("fft_" + channel, 512);
        }
        
        public IPdArray PdArray => pdArray_;
        
        public int Energy { get; set; }
    }
}
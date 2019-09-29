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
        public Rect rectangularSelection;
        private PdArray pdArray_;

        private void Awake()
        {
            pdArray_ = new PdArray("fft_" + channel, 512);
        }
        
        public IPdArray PdArray => pdArray_;
        
        public int Energy { get; set; }
    }
}
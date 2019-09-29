using UnityEngine;

namespace cylvester
{
    public interface IPdSpectrumBind
    {
        IPdArray PdArray { get; }
        float TrimmedEnergy { get; }
    }
    
    [ExecuteInEditMode]
    public class PdSpectrumBind : MonoBehaviour, IPdSpectrumBind
    {
        public int channel;
        public int startBin;
        public int endBin;
        public float topClip;
        public float bottomClip;
        public float trimmedEnergy = 0f;

        public float TrimmedEnergy => trimmedEnergy;

        private PdArray pdArray_;
        public IPdArray PdArray => pdArray_;

        private void Awake()
        {
            pdArray_ = new PdArray("fft_" + channel, 512);
        }


    }
}
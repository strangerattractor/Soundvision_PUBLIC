using UnityEngine;

namespace cylvester
{
    public interface IPdSpectrumBind
    {
        IPdArray GetPdArray(int index);
        int Channel { get; set; }
        int Energy { get; set; }
    }
    
    public class PdSpectrumBind : MonoBehaviour, IPdSpectrumBind
    {
        [SerializeField] private PdBackend pdBackend;
        [SerializeField] private Rect selection;
        
        public IPdArray GetPdArray(int index)
        {
            return pdBackend.fftArrayContainer[index];
        }

        public int Channel { get; set; }
        public int Energy { get; set; }
    }
}
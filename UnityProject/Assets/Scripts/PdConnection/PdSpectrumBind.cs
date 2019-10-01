using UnityEngine;

namespace cylvester
{
    public interface IPdSpectrumBind
    {
        IPdArray GetPdArray(int index);
        int Channel { get; set; }
        ref Rect Selection { get; }
        int Energy { get; set; }
    }
    
    public class PdSpectrumBind : MonoBehaviour, IPdSpectrumBind
    {
        [SerializeField] private PdBackend pdBackend;
        private Rect selection_;
        
        public IPdArray GetPdArray(int index)
        {
            return pdBackend.fftArrayContainer[index];
        }

        public int Channel { get; set; }
        public ref Rect Selection => ref selection_;
        public int Energy { get; set; }
    }
}
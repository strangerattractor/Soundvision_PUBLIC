using UnityEngine;

namespace cylvester
{
    public class WaveformTexture : MonoBehaviour
    {
        [SerializeField] PdBackend PdBackend;
        [SerializeField, Range(1, 16)] private int channel = 1;

         
        private Texture2D texture2D_;

        private void Start()
        {
            texture2D_ = new Texture2D(PdConstant.FftSize, PdConstant.FftSize, TextureFormat.R16, false);
        }

        private void Update()
        {
            var spectrum = PdBackend.SpectrumArrayContainer[channel];
            
            
            texture2D_.Apply();
        }
    }
}
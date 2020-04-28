using UnityEngine;
using UnityEngine.VFX;


namespace cylvester
{ 
    public class SpectrogramVFXBind : MonoBehaviour
    {
        [SerializeField] private Spectrogram spectrogram;
        [SerializeField] private VisualEffect visualEffect;

        public void Update()
        {
            visualEffect.SetTexture("Spectrogram", spectrogram.Texture);
            visualEffect.SetInt("Spectrogram_Index", spectrogram.Index);
        }
    }

}

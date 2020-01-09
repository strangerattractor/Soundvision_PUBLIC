using UnityEngine;
using UnityEngine.Experimental.VFX;


namespace cylvester
{ 
    public class WaveFormVFXBind : MonoBehaviour
    {
        [SerializeField] private Waveform waveform;
        [SerializeField] private VisualEffect visualEffect;

        public void Update()
        {
            visualEffect.SetTexture("Waveform", waveform.Texture);
        }
    }

}

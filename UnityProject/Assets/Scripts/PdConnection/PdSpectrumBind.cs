using UnityEngine;

namespace cylvester
{
    public interface IPdSpectrumBind
    {
        float TrimmedEnergy { get; }
    }
    
    public class PdSpectrumBind : MonoBehaviour, IPdSpectrumBind
    {
        public int channel;
        public int startBin;
        public int endBin;
        [SerializeField] private float topClip;
        [SerializeField] private float bottomClip;
        [SerializeField] private float trimmedEnergy = 0f;
        
        public float TrimmedEnergy => trimmedEnergy;

        private void Start()
        {
            
        }

        private void Update()
        {
            
        }
    }
}
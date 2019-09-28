using UnityEngine;

namespace cylvester
{
    public class WaveformVisualizerBehaviour : MonoBehaviour
    {
        #pragma warning disable 649
        [SerializeField] private string pdArrayName;
        [SerializeField] private int pdArraySize;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField, Range(0f, 10f)] private float scale = 1f;
        #pragma warning restore 649

        private PdArray pdArray_;
        
        void Start()
        {
            pdArray_ = new PdArray(pdArrayName, pdArraySize);
        }

        void Update()
        {
            pdArray_.Update();
            for(var i = 0; i < pdArray_.Data.Length; i++)
            {
                var posX = (i / 20f) - 0.5f;
                lineRenderer.SetPosition(i, new Vector3(posX, pdArray_.Data[i] * scale, 0));
            }
        }
    }
}

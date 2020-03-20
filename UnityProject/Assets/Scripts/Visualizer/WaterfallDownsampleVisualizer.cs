using UnityEngine;

namespace cylvester
{
    public class WaterfallDownsampleVisualizer : MonoBehaviour
    {
        private const int historySize = 32;

        [SerializeField] private PdBackend pdBackend = null;
        [SerializeField] private GameObject spectrumPrefab = null;
        [SerializeField, Range(1, 16)] private int channel = 1;
        [SerializeField] private int numBins = 8;

        private IPdArray spectrumArray_;
        private ISpectrumVisualizer[] visualizers_;
        private Transform[] transforms_;
        private int head_;
        
        private void Start()
        {
            visualizers_ = new ISpectrumVisualizer[historySize];
            transforms_ = new Transform[historySize];

            for (var i = 0; i < historySize; ++i)
            {
                var instance = Instantiate(spectrumPrefab, gameObject.transform, true);
                transforms_[i] = instance.transform;
                visualizers_[i] = instance.GetComponent<SpectrumVisualizer>();
            }
        }
        
        public void Update()
        {
            spectrumArray_ = pdBackend.SpectrumArrayContainer[channel-1];
            float[] averaged = new float[spectrumArray_.Data.Length];
            float[] bins = new float[numBins];
            for (int i = 0; i < bins.Length; i++)
            {
                bins[i] = 0;
            }
            for (int i = 0; i < averaged.Length; i++)
            {
                bins[i * bins.Length / averaged.Length] += spectrumArray_.Data[i] / averaged.Length;
            }
            for (int i = 0; i < averaged.Length; i++)
            {
                averaged[i] = bins[i * bins.Length / averaged.Length];
            }
            visualizers_[head_].Spectrum = averaged;
            head_++;
            head_ %= historySize;
            
            for (var i = 0; i < historySize; ++i)
            {
                var index = (head_ + i) % historySize;
                transforms_[index].localPosition = new Vector3(0, 0, 1f * i);
            }
        }
    }
}
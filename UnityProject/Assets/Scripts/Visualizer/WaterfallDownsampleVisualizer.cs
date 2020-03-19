using UnityEngine;

namespace cylvester
{
    public class WaterfallDownsampleVisualizer : MonoBehaviour
    {
        private const int historySize = 32;

        [SerializeField] private PdBackend pdBackend = null;
        [SerializeField] private GameObject spectrumPrefab = null;
        [SerializeField, Range(1, 16)] private int channel = 1;

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
            float[] a = new float[spectrumArray_.Data.Length];
            float[] b = new float[8];
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = 0;
            }
            for (int i = 0; i < a.Length; i++)
            {
                b[i * b.Length / a.Length] += spectrumArray_.Data[i] / a.Length;
            }
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = b[i * b.Length / a.Length];
            }
            visualizers_[head_].Spectrum = a;
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
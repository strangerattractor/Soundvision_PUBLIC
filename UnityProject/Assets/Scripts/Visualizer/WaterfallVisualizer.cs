using UnityEngine;

namespace cylvester
{
    public class WaterfallVisualizer : MonoBehaviour
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
            spectrumArray_ = pdBackend.spectrumArrayContainer[channel-1];
            visualizers_[head_].Spectrum = spectrumArray_.Data;
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
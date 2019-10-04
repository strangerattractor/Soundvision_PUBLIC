using System.Linq;
using UnityEngine;

namespace cylvester
{
    public class RmsAnalyzer : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource = default;
        private const int BufSize = 256;
        private float[] audioSamples_;
        private float rms_;

        public float RMS => rms_;

        private void Start()
        {
            audioSamples_ = new float[BufSize];
        }

        private void Update()
        {
            audioSource.GetOutputData(audioSamples_, 0);
            rms_ = GetRms(audioSamples_);
        }

        private static float GetRms(float[] buffer)
        {
            return Mathf.Sqrt(buffer.Sum(sample => sample * sample) / BufSize);
        }
    }
}

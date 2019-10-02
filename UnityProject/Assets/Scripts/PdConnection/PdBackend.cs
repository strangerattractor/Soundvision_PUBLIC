using System;
using UnityEngine;

namespace cylvester
{
    public class PdBackend : MonoBehaviour
    {
        public string mainPatch = "analyzer.pd";
        public int samplePlayback;
        public PdArray levelMeterArray;
        public IFftArrayContainer fftArrayContainer;

        private IChangeObserver<int> samplePlaybackObserver_;
        private Action onSamplePlaybackChanged_;
        private IPdSocket pdSocket_;

        private void Awake()
        {
            PdProcess.Instance.Start(mainPatch);
            levelMeterArray = new PdArray("levelmeters", PdConstant.NumMaxInputChannels);
            fftArrayContainer = new FftArrayContainer();
            pdSocket_ = new PdSocket(PdConstant.ip, PdConstant.port);
            
            samplePlaybackObserver_ = new ChangeObserver<int>(samplePlayback);

            onSamplePlaybackChanged_ = () =>
            {
                var bytes = new[]{(byte)PdMessage.SampleSound, (byte)samplePlayback};
                pdSocket_.Send(bytes);
            };
            
            samplePlaybackObserver_.ValueChanged += onSamplePlaybackChanged_;
        }

        private void OnDestroy()
        {
            PdProcess.Instance.Stop();
            levelMeterArray?.Dispose();
            pdSocket_?.Dispose();
            samplePlaybackObserver_.ValueChanged -= onSamplePlaybackChanged_;
        }

        public void Update()
        {
            if(PdProcess.Instance.Running)
                levelMeterArray.Update();

            fftArrayContainer.Update();
            samplePlaybackObserver_.Value = samplePlayback;
        }
    }
}
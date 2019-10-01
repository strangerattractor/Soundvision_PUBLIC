using System;
using UnityEngine;

namespace cylvester
{
    public class PdBackend : MonoBehaviour
    {
        public string mainPatch = "analyzer.pd";
        public int inchannels = 2;
        public int samplePlayback = 0;
        public PdArray levelMeterArray;
        public FftArrayContainer fftArrayContainer;

        private IChangeObserver<int> samplePlaybackObserver_;
        private Action onSamplePlaybackChanged_;
        private IPdSocket pdSocket_;
        
        
        private void Start()
        {
            PdProcess.Instance.Start(mainPatch, inchannels);
            levelMeterArray = new PdArray("levelmeters", PdConstant.NumMaxInputChannels);
            fftArrayContainer = new FftArrayContainer();
            pdSocket_ = new PdSocket(PdConstant.ip, PdConstant.port);
            
            samplePlaybackObserver_ = new ChangeObserver<int>(samplePlayback);

            onSamplePlaybackChanged_ = () =>
            {
                var bytes = new byte[]{(byte)PdMessage.SampleSound, (byte)samplePlayback};
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
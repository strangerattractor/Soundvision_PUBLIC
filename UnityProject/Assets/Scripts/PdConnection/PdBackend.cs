using System;
using UnityEngine;

namespace cylvester
{
    public class PdBackend : MonoBehaviour
    {
        public int samplePlayback;
        public ISpectrumArrayContainer spectrumArrayContainer;

        private IChangeObserver<int> samplePlaybackObserver_;
        private Action onSamplePlaybackChanged_;
        private IPdSocket pdSocket_;
        private IDspController dspController_;

        private void Awake()
        {
            spectrumArrayContainer = new SpectrumArrayContainer();
            pdSocket_ = new PdSocket(PdConstant.ip, PdConstant.port);
            dspController_ = new DspController(pdSocket_);

            samplePlaybackObserver_ = new ChangeObserver<int>(samplePlayback);

            onSamplePlaybackChanged_ = () =>
            {
                pdSocket_.Send(new[]{(byte)PdMessage.SampleSound, (byte)samplePlayback});
            };
            
            samplePlaybackObserver_.ValueChanged += onSamplePlaybackChanged_;
            dspController_.State = true;

        }
        
        private void OnDestroy()
        {
            dspController_.State = false;
            pdSocket_?.Dispose();
            samplePlaybackObserver_.ValueChanged -= onSamplePlaybackChanged_;
        }

        public void Update()
        {
            spectrumArrayContainer.Update();
            samplePlaybackObserver_.Value = samplePlayback;
        }
        
  
    }
}
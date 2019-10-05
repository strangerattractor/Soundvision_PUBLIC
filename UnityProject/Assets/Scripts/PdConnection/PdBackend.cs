using System;
using UnityEngine;

namespace cylvester
{

    public interface IPdBackend
    {
        ISpectrumArrayContainer SpectrumArrayContainer{ get; }
    }
    
    public class PdBackend : MonoBehaviour, IPdBackend
    {
        public int samplePlayback;
        private IUpdater spectrumArrayUpdater_;

        private IChangeObserver<int> samplePlaybackObserver_;
        private Action onSamplePlaybackChanged_;
        private IPdSocket pdSocket_;
        private IDspController dspController_;

        public ISpectrumArrayContainer SpectrumArrayContainer { get; private set; }

        private void Awake()
        {
            SpectrumArrayContainer = new SpectrumArrayContainer();
            spectrumArrayUpdater_ = (IUpdater) SpectrumArrayContainer;
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
            spectrumArrayUpdater_.Update();
            samplePlaybackObserver_.Value = samplePlayback;
        }


    }
}
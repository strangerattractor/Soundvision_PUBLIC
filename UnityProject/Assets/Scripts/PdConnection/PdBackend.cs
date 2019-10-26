using System;
using System.Collections.Generic;
using UnityEngine;

namespace cylvester
{

    public interface IPdBackend
    {
        IPdArray LevelArray { get; }
        IPdArrayContainer SpectrumArrayContainer{ get; }
        IPdArrayContainer WaveformArrayContainer{ get; }
    }
    
    public class PdBackend : MonoBehaviour, IPdBackend
    {
        [SerializeField] UnityControlEvent midiMessageReceived = null;
        
        public int samplePlayback;

        private IChangeObserver<int> samplePlaybackObserver_;
        
        private IPdSender pdSender_;
        private IPdReceiver pdReceiver_;
        private IMidiParser midiParser_;
        private IDspController dspController_;
        
        public IPdArray LevelArray { get; private set; }
        public IPdArrayContainer SpectrumArrayContainer { get; private set; }
        public IPdArrayContainer WaveformArrayContainer { get; private set; }
        
        private List<IUpdater> updaters_;
        
        private Action onSamplePlaybackChanged_;
        private Action<MidiMessage> onMidiMessageReceived_;

        private void Awake()
        {
            SpectrumArrayContainer = new PdArrayContainer("fft_");
            WaveformArrayContainer = new PdArrayContainer("wave_");
            LevelArray = new PdArray("level", PdConstant.NumMaxInputChannels);

            updaters_ = new List<IUpdater>
            {(IUpdater) LevelArray, (IUpdater) SpectrumArrayContainer, (IUpdater) WaveformArrayContainer};

            pdSender_ = new PdSender(PdConstant.ip, PdConstant.sendPort);
            pdReceiver_ = new PdReceiver(PdConstant.receivedPort);
            midiParser_ = new MidiParser(pdReceiver_);
            
            dspController_ = new DspController(pdSender_);

            samplePlaybackObserver_ = new ChangeObserver<int>(samplePlayback);

            onSamplePlaybackChanged_ = () => { pdSender_.Send(new[]{(byte)PdMessage.SampleSound, (byte)samplePlayback}); };

            onMidiMessageReceived_ = (message) => {
                midiMessageReceived.Invoke(message); 
            };
            
            samplePlaybackObserver_.ValueChanged += onSamplePlaybackChanged_;
            midiParser_.MidiMessageReceived += onMidiMessageReceived_;

            dspController_.State = true;
        }
        
        private void OnDestroy()
        {
            dspController_.State = false;
            pdSender_?.Dispose();
            samplePlaybackObserver_.ValueChanged -= onSamplePlaybackChanged_;
            midiParser_.MidiMessageReceived -= onMidiMessageReceived_;
        }

        public void Update()
        {
            pdReceiver_.Update();
            foreach (var updater in updaters_)
                updater.Update();
            
            samplePlaybackObserver_.Value = samplePlayback;
        }
    }
}
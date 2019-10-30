using System;
using System.Collections.Generic;
using UnityEngine;

namespace cylvester
{
    public interface IPdBackend
    {
        IPdArray LevelArray { get; }
        IPdArray NoiseArray { get; }
        IPdArray PitchArray { get; }
        IPdArrayContainer SpectrumArrayContainer{ get; }
        IPdArrayContainer WaveformArrayContainer{ get; }
        void SendState(IStateManager manager);
    }
    
    public class PdBackend : MonoBehaviour, IPdBackend
    {
        [SerializeField] UnityMidiEvent midiMessageReceived = null;
        [SerializeField] UnitySyncEvent midiSyncReceived = null;

        public int samplePlayback;

        private IChangeObserver<int> samplePlaybackObserver_;
        
        private IPdSender pdSender_;
        private IPdReceiver pdReceiver_;
        private IMidiParser midiParser_;
        private IDspController dspController_;
        
        public IPdArray LevelArray { get; private set; }
        public IPdArray NoiseArray { get; private set; }
        public IPdArray PitchArray { get; private set; }
        public IPdArrayContainer SpectrumArrayContainer { get; private set; }
        public IPdArrayContainer WaveformArrayContainer { get; private set; }
        
        private List<IUpdater> updaters_;
        
        private Action onSamplePlaybackChanged_;
        private Action<MidiMessage> onMidiMessageReceived_;
        private Action<MidiSync, int> onMidiSyncReceived_;

        private void Awake()
        {
            LevelArray = new PdArray("level", PdConstant.NumMaxInputChannels);
            NoiseArray = new PdArray("noise", PdConstant.NumMaxInputChannels);
            PitchArray = new PdArray("pitch", PdConstant.NumMaxInputChannels);
            SpectrumArrayContainer = new PdArrayContainer("fft_");
            WaveformArrayContainer = new PdArrayContainer("wave_");
            
            updaters_ = new List<IUpdater>
            {
                (IUpdater) LevelArray, 
                (IUpdater) NoiseArray,
                (IUpdater) PitchArray,
                (IUpdater) SpectrumArrayContainer, 
                (IUpdater) WaveformArrayContainer
            };

            pdSender_ = new PdSender(PdConstant.ip, PdConstant.sendPort);
            pdReceiver_ = new PdReceiver(PdConstant.receivedPort);
            midiParser_ = new MidiParser(pdReceiver_);
            
            dspController_ = new DspController(pdSender_);

            samplePlaybackObserver_ = new ChangeObserver<int>(samplePlayback);

            onSamplePlaybackChanged_ = () =>
            {
                pdSender_.Send("sample " + samplePlayback);
            };

            onMidiMessageReceived_ = (message) => { midiMessageReceived.Invoke(message); };
            onMidiSyncReceived_ = (sync, count) => { midiSyncReceived.Invoke(sync, count); };
            
            samplePlaybackObserver_.ValueChanged += onSamplePlaybackChanged_;
            midiParser_.MidiMessageReceived += onMidiMessageReceived_;
            midiParser_.MidiSyncReceived += onMidiSyncReceived_;
            dspController_.State = true;
        }
        
        private void OnDestroy()
        {
            dspController_.State = false;
            pdSender_?.Dispose();
            samplePlaybackObserver_.ValueChanged -= onSamplePlaybackChanged_;
            midiParser_.MidiMessageReceived -= onMidiMessageReceived_;
            midiParser_.MidiSyncReceived -= onMidiSyncReceived_;

        }

        public void Update()
        {
            pdReceiver_.Update();
            foreach (var updater in updaters_)
                updater.Update();
            
            samplePlaybackObserver_.Value = samplePlayback;
        }

        public void SendState(IStateManager stateManager)
        {
            pdSender_.Send("state previous " + stateManager.PreviousState);
            pdSender_.Send("state current " + stateManager.CurrentState);
            pdSender_.Send("state next " + stateManager.NextState);
        }

    }
}
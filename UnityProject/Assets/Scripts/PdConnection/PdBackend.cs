using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{

    public interface IPdBackend
    {
        IPdArray LevelArray { get; }
        IPdArrayContainer SpectrumArrayContainer{ get; }
        IPdArrayContainer WaveformArrayContainer{ get; }
        void Message(string message);
    }
    
    public class PdBackend : MonoBehaviour, IPdBackend
    {
        [SerializeField] UnityMidiEvent midiMessageReceived = null;
        [SerializeField] UnityEvent midiClockReceived = null;

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
        private Action onMidiClockReceived_;

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

            onSamplePlaybackChanged_ = () =>
            {
                pdSender_.Send("sample " + samplePlayback);
            };

            onMidiMessageReceived_ = (message) => { midiMessageReceived.Invoke(message); };
            onMidiClockReceived_ = () => { midiClockReceived.Invoke(); };
            
            samplePlaybackObserver_.ValueChanged += onSamplePlaybackChanged_;
            midiParser_.MidiMessageReceived += onMidiMessageReceived_;
            midiParser_.MidiClockReceived += onMidiClockReceived_;
            dspController_.State = true;
        }
        
        private void OnDestroy()
        {
            dspController_.State = false;
            pdSender_?.Dispose();
            samplePlaybackObserver_.ValueChanged -= onSamplePlaybackChanged_;
            midiParser_.MidiMessageReceived -= onMidiMessageReceived_;
            midiParser_.MidiClockReceived -= onMidiClockReceived_;

        }

        public void Update()
        {
            pdReceiver_.Update();
            foreach (var updater in updaters_)
                updater.Update();
            
            samplePlaybackObserver_.Value = samplePlayback;
        }

        public void Message(string message)
        {
            pdSender_.Send("message " + message);
        }

    }
}
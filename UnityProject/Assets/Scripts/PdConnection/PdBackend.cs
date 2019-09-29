using System;
using UnityEngine;

namespace cylvester
{
    public interface IPdBackend
    {
        string MainPatch { get; set; }
        int NumInputChannels { get; set;}
        
        IPdArray LevelMeterArray { get; }
        IFftArrayContainer FFTArrayContainer { get; }
    }
    
    public class PdBackend : MonoBehaviour, IPdBackend
    {
        public string mainPatch = "analyzer.pd";
        public int inchannels = 2;
        
        private Action onToggled_;
        private PdArray levelMeterArray_;
        private FftArrayContainer fftArrayContainer_;
        
        private const int NumMaxInputChannels = 16;
        
        public IPdArray LevelMeterArray => levelMeterArray_;
        public IFftArrayContainer FFTArrayContainer => fftArrayContainer_;

        public string MainPatch { get => mainPatch; set => mainPatch = value; }
        public int NumInputChannels { get => inchannels -1; set => inchannels = value + 1; }


        private void Start()
        {
            PdProcess.Instance.Start(mainPatch, inchannels);
            levelMeterArray_ = new PdArray("levelmeters", NumMaxInputChannels);
            fftArrayContainer_ = new FftArrayContainer();
        }

        private void OnDestroy()
        {
            PdProcess.Instance.Stop();
            levelMeterArray_?.Dispose();
        }

        public void Update()
        {
            if(PdProcess.Instance.Running)
                levelMeterArray_.Update();

            fftArrayContainer_.Update();
        }
    }
}
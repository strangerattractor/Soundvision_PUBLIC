using System;
using System.Threading;
using UnityEngine;

namespace cylvester
{
    public interface IPdBackend
    {
        string MainPatch { get; set; }
        int NumInputChannels { get; set;}
        
        bool State { get; set; }
        void UpdateShmem();
        IPdArray LevelMeterArray { get; }
    }
    
    [ExecuteInEditMode]
    public class PdBackend : MonoBehaviour, IPdBackend
    {
        public string mainPatch = "analyzer.pd";
        public int inchannels = 2;
        
        private Action onToggled_;
        private PdArray levelMeterArray_;
        private UdpSender udpSender_;
        private bool state_;
        
        private const int NumMaxInputChannels = 16;
        
        public IPdArray LevelMeterArray => levelMeterArray_;

        public string MainPatch { get => mainPatch; set => mainPatch = value; }
        public int NumInputChannels { get => inchannels -1; set => inchannels = value + 1; }

        public bool State
        {
            get => state_;
            set
            {
                if (state_ == value)
                    return;
                
                var bytes = new byte[1];
                bytes[0] = state_ ? (byte)0 : (byte)1;
                udpSender_.SendBytes(bytes);
                state_ = value;
            }
        }

        private void OnEnable()
        {
            PdProcess.Instance.Start(mainPatch, inchannels);
            
            levelMeterArray_ = new PdArray("levelmeters", NumMaxInputChannels);
            udpSender_ = new UdpSender("127.0.0.1", 54637);
        }

        private void OnDisable()
        {
            PdProcess.Instance.Stop();
            levelMeterArray_?.Dispose();
            udpSender_?.Dispose();
        }

        public void UpdateShmem()
        {
            if(PdProcess.Instance.Running)
                levelMeterArray_.Update();
        }
    }
}
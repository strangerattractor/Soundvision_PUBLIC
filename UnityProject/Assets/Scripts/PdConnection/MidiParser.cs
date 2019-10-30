using System;
using UnityEngine.Events;

namespace cylvester
{
    [Serializable]
    public class UnityMidiEvent : UnityEvent<MidiMessage>
    {}

    [Serializable]
    public class UnitySyncEvent : UnityEvent<MidiSync>
    {}
    
    public enum MidiSync
    {
        Start = 250,
        Stop = 252,
        Clock = 248,
    }
    
    public struct MidiMessage
    {
        public byte Status;
        public byte Data1;
        public byte Data2;

        public override string ToString()
        {
            var statusStr = Status.ToString();
            var data1Str = Data1.ToString();
            var data2Str = Data2.ToString();
            return statusStr + " " +data1Str + " " +data2Str;
        }
    }
    public interface IMidiParser : IDisposable
    {
        event Action<MidiMessage> MidiMessageReceived;
        event Action<MidiSync> MidiSyncReceived;
    }
    
    public class MidiParser : IMidiParser 
    {
        private enum Accept
        {
            StatusByte,
            DataByte1,
            DataByte2
        }

        private MidiMessage message_;
        private Accept accept_ = Accept.StatusByte;
        private readonly IPdReceiver pdReceiver_;
        private readonly Action<byte[]> onDataReceived_;

        public MidiParser(IPdReceiver pdReceiver)
        {
            pdReceiver_ = pdReceiver;

            onDataReceived_ = (bytes) =>
            {
                foreach (var element in bytes)
                {
                    if (ProcessSyncByte(element))
                        continue;

                    if (element >= 128)
                    {
                        message_ = new MidiMessage {Status = element};
                        accept_ = Accept.DataByte1;
                        continue;
                    }

                    if (accept_ == Accept.DataByte1 && element <= 128)
                    {
                        message_.Data1 = element;
                        accept_ = Accept.DataByte2;
                    }
                    else if (accept_ == Accept.DataByte2 && element <= 128)
                    {
                        message_.Data2 = element;
                        MidiMessageReceived?.Invoke(message_);
                        accept_ = Accept.StatusByte;
                    }
                }
            };
            
            pdReceiver_.DataReceived += onDataReceived_;
        }

        public void Dispose()
        {
            pdReceiver_.DataReceived -= onDataReceived_;
        }

        private bool ProcessSyncByte(byte element)
        {
            if (element != (byte)MidiSync.Clock && 
                element != (byte)MidiSync.Start && 
                element != (byte)MidiSync.Stop)
                return false;

            MidiSyncReceived?.Invoke((MidiSync)element);
            return true;
        }
        
        public event Action<MidiMessage> MidiMessageReceived;
        public event Action<MidiSync> MidiSyncReceived;
    }
}
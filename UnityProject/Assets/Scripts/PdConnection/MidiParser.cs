using System;
using UnityEngine.Events;

namespace cylvester
{
    [Serializable]
    public class UnityControlEvent : UnityEvent<ControlMessage>
    {}
    
    public struct ControlMessage
    {
        public byte Channel;
        public byte ControlNumber;
        public byte ControlValue;
    }
    
    public interface IMidiParser : IDisposable
    {
        event Action<ControlMessage> ControlMessageReceived;
    }
    
    public class MidiParser : IMidiParser 
    {
        private enum Accept
        {
            StatusByte,
            DataByte1,
            DataByte2
        }

        private struct MidiMessage
        {
            public byte Status;
            public byte Data1;
            public byte Data2;
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
                        Invoke();
                        accept_ = Accept.StatusByte;
                    }
                }
            };
            
            pdReceiver_.DataReceived += onDataReceived_;
        }

        private void Invoke()
        {
            if (176 <= message_.Status || message_.Status <= 191)
            {
                var controlMessage = new ControlMessage
                {
                    Channel = (byte) (message_.Status - 176),
                    ControlNumber = message_.Data1,
                    ControlValue = message_.Data2
                };

                ControlMessageReceived?.Invoke(controlMessage);
            }
        }
        
        public void Dispose()
        {
            pdReceiver_.DataReceived -= onDataReceived_;
        }
        
        public event Action<ControlMessage> ControlMessageReceived;

    }
}
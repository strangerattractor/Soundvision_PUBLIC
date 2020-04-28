using System;
using System.Net;
using System.Net.Sockets;

namespace cylvester
{
    public interface IPdReceiver
    {
        event Action<byte[]> DataReceived;
        void Update();
    }
    
    public class PdReceiver : IPdReceiver
    {
        private readonly UdpClient udpClient_;
        private IPEndPoint remote_;
        
        public PdReceiver(int port)
        {
            udpClient_ = new UdpClient(port);
            remote_ = new IPEndPoint(IPAddress.Any, port);
        }

        public void Update()
        {
            while (udpClient_.Available > 0)
            {
                var receivedData = udpClient_.Receive(ref remote_);
                DataReceived?.Invoke(receivedData);
            }
        }

        public event Action<byte[]> DataReceived;
    }
}
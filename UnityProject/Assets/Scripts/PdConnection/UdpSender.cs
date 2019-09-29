using System;

namespace cylvester
{
    interface IUdpSender : IDisposable
    {
        void SendBytes(byte[] data);
    }

    public class UdpSender : IDisposable
    {
        private readonly string remoteHost_;
        private readonly int remotePort_;
        private System.Net.Sockets.UdpClient udpClient_;

        public UdpSender(string remoteHost, int remotePort)
        {
            remoteHost_ = remoteHost;
            remotePort_ = remotePort;
            udpClient_ = new System.Net.Sockets.UdpClient();
        }

        public void SendBytes(byte[] data)
        {
            udpClient_.Send(data, data.Length, remoteHost_, remotePort_);
        }

        public void Dispose()
        {
            udpClient_.Close();
            udpClient_ = null;
        }
    }
}
using System;
using System.Net;
using System.Net.Sockets;

namespace cylvester
{
    public interface IPdSender : IDisposable
    {
        void Send(byte[] bytes);
    }
    
    public class PdSender : IPdSender
    {
        private Socket socket_;

        public PdSender(string ip, int port)
        { 
            socket_ = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket_.Connect(IPAddress.Parse(ip), port);
        }

        public void Send(byte[] bytes)
        {
            socket_.Send(bytes);
        }

        public void Dispose()
        {
            socket_.Close();
        }
    }
}
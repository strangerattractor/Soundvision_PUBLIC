using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace cylvester
{
    public interface IPdSender : IDisposable
    {
        void Send(string str);
    }
    
    public class PdSender : IPdSender
    {
        private Socket socket_;

        public PdSender(string ip, int port)
        { 
            socket_ = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket_.Connect(IPAddress.Parse(ip), port);
        }

        public void Send(string str)
        {
            socket_.Send(Encoding.ASCII.GetBytes(str + "\n"));
        }

        public void Dispose()
        {
            socket_.Close();
        }
    }
}
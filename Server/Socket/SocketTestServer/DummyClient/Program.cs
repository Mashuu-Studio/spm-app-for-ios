using System.Net.Sockets;
using System.Net;
using System.Text;
using ServerCore;

namespace DummyClient
{
    class Packet
    {
        public ushort size;
        public ushort id;
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            var ipHost = Dns.GetHostEntry(host);
            var ipAddr = ipHost.AddressList[0];
            var endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new ServerSession(); });
            while (true) ;
        }
    }
}

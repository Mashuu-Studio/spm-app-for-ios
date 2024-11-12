using System.Net;
using System.Text;
using ServerCore;

namespace Server
{
    internal class Program
    {
        static Listner _listener;

        internal static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            var ipHost = Dns.GetHostEntry(host);
            var ipAddr = ipHost.AddressList[0];
            var endPoint = new IPEndPoint(ipAddr, 7777);

            _listener = new Listner();

            Console.WriteLine("Listening...");
            _listener.Init(endPoint, () => { return new ClientSession(); });
            while (true)
            {
            }
        }
    }
}

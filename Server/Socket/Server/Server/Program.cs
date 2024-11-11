using ServerCore;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;

namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = Dns.GetHostName();
            var ipHost = Dns.GetHostEntry(host);
            var ipAddr = ipHost.AddressList[0];
            var port = 8888;
            var endPoint = new IPEndPoint(IPAddress.Any, port);
            var backlog = 100;

            var tcpListener = new TcpListener(endPoint);
            tcpListener.Start(backlog);

            while (true)
            {
                var client = await tcpListener.AcceptTcpClientAsync();
                Session session = new Session(client);
                session.StartReceieve();

                Console.WriteLine("Client Connected");

            }
        }
    }
}
using System.Net.Sockets;
using System.Net;
using System.Text;
using ServerCore;

namespace Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = Dns.GetHostName();
            var ipHost = Dns.GetHostEntry(host);
            var ipAddr = ipHost.AddressList[0];
            var port = 8888;
            var endPoint = new IPEndPoint(ipAddr, port);

            for (int i = 0; i < 1; i++)
            {
                Test(endPoint);
            }
            while (true) ;
        }

        static async void Test(IPEndPoint endPoint)
        {
            var client = new TcpClient();
            try
            {
                Console.WriteLine("Connecting");
                await client.ConnectAsync(endPoint);
                Console.WriteLine($"Connected : {endPoint}");
                Session session = new Session(client);
                session.StartReceieve();
                Random rand = new Random();
                List<Vector2[]> list = new List<Vector2[]>()
                {
                    new Vector2[]{
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),

                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),

                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),

                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),

                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble())
                    },
                     new Vector2[]{
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),

                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),

                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),

                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),

                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble())
                    },
                     new Vector2[]{
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),

                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),

                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),

                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),

                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()),
                        new Vector2((float)rand.NextDouble(), (float)rand.NextDouble())
                    }
                };

                List<byte[]> bytes = new List<byte[]>()
                {
                    new byte[2]{0, 1 },
                    new byte[2]{0, 1 },
                    new byte[2]{1, 0 },
                    new byte[2]{0, 1 },
                    new byte[2]{1, 1 },
                    new byte[2]{0, 1 },
                    new byte[2]{0, 0 },
                };
                ExerciseDataPacket packet = new ExerciseDataPacket(8, list, bytes, 100, 50);

                session.RegisterSend(packet);
            }
            catch (Exception e)
            {
                client.Dispose();
            }
        }
    }

}
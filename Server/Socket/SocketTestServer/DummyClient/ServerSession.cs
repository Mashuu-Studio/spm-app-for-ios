using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected {endPoint}");

            Packet packet = new Packet() { size = 4, id = 10 };

            for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> segment = SendBufferHelper.Open(4096);
                byte[] buffer = BitConverter.GetBytes(packet.size);
                byte[] buffer2 = BitConverter.GetBytes(packet.id);

                Array.Copy(buffer, 0, segment.Array, segment.Offset, buffer.Length);
                Array.Copy(buffer2, 0, segment.Array, segment.Offset + buffer.Length, buffer2.Length);
                var sendBuffer = SendBufferHelper.Close(packet.size);

                Send(sendBuffer);
            }
        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");

            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transfered {numOfBytes}");
        }
    }

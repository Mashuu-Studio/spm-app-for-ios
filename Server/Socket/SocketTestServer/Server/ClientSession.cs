using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Packet
    {
        public ushort size;
        public ushort id;
    }

    class PlayerInfoReq : Packet
    {
        public long playerId;
    }

    class PlayerInfoOk : Packet 
    {
        public int hp;
        public int attack;
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2
    }

    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected {endPoint}");
            /*
            Packet packet = new Packet(){ size = 4, id = 10 };

            ArraySegment<byte> segment = SendBufferHelper.Open(4096);
            byte[] buffer = BitConverter.GetBytes(packet.size);
            byte[] buffer2 = BitConverter.GetBytes(packet.id);

            Array.Copy(buffer, 0, segment.Array, segment.Offset, buffer.Length);
            Array.Copy(buffer2, 0, segment.Array, segment.Offset + buffer.Length, buffer2.Length);
            var sendBuffer = SendBufferHelper.Close(packet.size);
            Send(sendBuffer);
            */
            Thread.Sleep(1000);

            Disconnect();
        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transfered {numOfBytes}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);
            Console.WriteLine($"{size} : {id}");
        }
    }
}

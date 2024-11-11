using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace ServerCore
{
    public class Session
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected;

        private object sendLock = new object();
        private bool sendFlush = false;
        private Queue<Func<Task>> sendQueue = new Queue<Func<Task>>();

        public Session(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
            isConnected = true;
        }

        #region Receieve
        public async void StartReceieve()
        {
            try
            {
                while (isConnected)
                {
                    // SendQueue를 통해서 전송시에 순서를 보장해줄 것임.
                    byte[] sizeBuffer = new byte[4];
                    await stream.ReadAsync(sizeBuffer, 0, sizeBuffer.Length);
                    int size = BitConverter.ToInt32(sizeBuffer, 0);
                    Console.WriteLine(size);

                    byte[] packetBuffer = new byte[size];
                    await stream.ReadAsync(packetBuffer, 0, packetBuffer.Length);
                    // 패킷의 맨 앞에 타입이 정의되어있음.
                    ushort type = BitConverter.ToUInt16(packetBuffer, 0);

                    PacketType packetType = (PacketType)type;
                    switch (packetType)
                    {
                        case PacketType.EXERCISE_DATA:
                            var packet = new ExerciseDataPacket();
                            Console.WriteLine("Start Handle");
                            packet.HandlePacket(packetBuffer);
                            Console.WriteLine("End Handle");
                            StringBuilder str = new StringBuilder();
                            str.AppendLine(packet.exercise);
                            foreach (var arr in packet.joints)
                            {
                                str.AppendLine();
                                foreach (var joint in arr)
                                {
                                    str.AppendLine($"{joint.x}, {joint.y}");
                                }
                            }
                            Console.WriteLine(str);

                            // 일단은 바로 result를 쏴주는 식으로 세팅.
                            // 테스트용
                            Random rand = new Random();
                            var list = new List<List<float>>();
                            for (int i = 0; i < ExerciseData.ExerciseState[ExerciseData.ExerciseNames[packet.exerIndex]].Count;  i++)
                            {
                                list.Add(new List<float>()
                                {
                                    (float)rand.NextDouble(),
                                    (float)rand.NextDouble(),
                                    (float)rand.NextDouble(),
                                });
                            }
                                ExerciseResultPacket result = new ExerciseResultPacket(packet.exerIndex, list);
                            _ = Task.Run(() => RegisterSend(result));
                            break;
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
        #endregion

        #region Send
        public void RegisterSend<T>(T packet) where T : Packet
        {
            bool flush = false;
            lock (sendLock)
            {
                sendQueue.Enqueue(async () => Send(packet));
                // 현재 flush를 안 하고 있다면 Flush를 진행하도록 해줌.
                if (sendFlush == false) flush = sendFlush = true;
            }

            if (flush) Flush();
        }

        private async void Send<T>(T packet) where T : Packet
        {
            // 패킷을 시리얼라이즈 해준 뒤 사이즈를 먼저 보내고 이 후에 데이터 전송.
            var stream = client.GetStream();
            byte[] data = packet.Serialize();
            byte[] sizeData = BitConverter.GetBytes(data.Length);

            await stream.WriteAsync(sizeData, 0, sizeData.Length);
            await stream.WriteAsync(data, 0, data.Length);
        }

        private async void Flush()
        {
            while (true)
            {
                var action = Pop();
                if (action == null) break;
                await action.Invoke();
            }
            sendFlush = false;
        }

        private Func<Task> Pop()
        {
            lock (sendLock)
            {
                if (sendQueue.Count == 0) return null;
                else return sendQueue.Dequeue();
            }
        }
        #endregion

        public void Close()
        {
            isConnected = false;
            client.Close();
        }

    }
}

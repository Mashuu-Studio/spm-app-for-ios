using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace ServerCore
{
    public class Session
    {
        private TcpClient client;
        private NetworkStream stream;
        public bool IsConnected { get { return isConnected; } }
        private bool isConnected = false;

        private object sendLock = new object();
        private bool sendFlush = false;
        private Queue<Func<Task>> sendQueue = new Queue<Func<Task>>();

        private int connectCount;
        private ConnectingPacket connectingPacket;

        public Session(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
            isConnected = true;
            // _ = ConnectingCheck();
        }

        public async Task ConnectingCheck()
        {
            try
            {
                connectCount = 0;
                connectingPacket = new ConnectingPacket();
                while (connectCount < 200) // 임시로 세팅. 비동기적으로 작동하지 않는 것은 이후에 처리.
                {
                    await Task.Delay(1000);
                    RegisterSend(connectingPacket); // 이쪽에서도 연결이 제대로 되어있다고 보내줘야 함.
                    connectCount++;
                }
                Close();
            }
            catch (Exception e)
            {

            }
            finally
            {
                Close();
            }
        }

        #region Receieve
        public async Task StartReceieve()
        {
            try
            {
                while (isConnected)
                {
                    // SendQueue를 통해서 전송시에 순서를 보장해줄 것임.
                    byte[] sizeBuffer = new byte[4];
                    await stream.ReadAsync(sizeBuffer, 0, sizeBuffer.Length);
                    int size = BitConverter.ToInt32(sizeBuffer, 0);

                    byte[] packetBuffer = new byte[size];
                    await stream.ReadAsync(packetBuffer, 0, packetBuffer.Length);
                    // 패킷의 맨 앞에 타입이 정의되어있음.
                    ushort type = BitConverter.ToUInt16(packetBuffer, 0);

                    PacketType packetType = (PacketType)type;
                    switch (packetType)
                    {
                        case PacketType.CONNECTING:
                            connectCount = 0; // 초기화
                            break;

                        case PacketType.EXERCISE_RESULT:
                            // 간단한 테스트용
                            var packet = new ExerciseResultPacket();
                            packet.HandlePacket(packetBuffer);
                            //UIController.Instance.RecvResult(true, packet.result); // 임시로 여기에 세팅
                            break;
                            /*
                            case PacketType.TESTPACKET:
                                var packet = new TestPacket();
                                packet.HandlePacket(packetBuffer);
                                break;
                            */
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
                sendQueue.Enqueue(async () => await Send(packet));
                // 현재 flush를 안 하고 있다면 Flush를 진행하도록 해줌.
                if (sendFlush == false) flush = sendFlush = true;
            }

            if (flush) Flush();
        }

        private async Task Send<T>(T packet) where T : Packet
        {
            try
            {
                // 패킷을 시리얼라이즈 해준 뒤 사이즈를 먼저 보내고 이 후에 데이터 전송.
                var stream = client.GetStream();
                byte[] data = packet.Serialize();
                byte[] sizeData = BitConverter.GetBytes(data.Length);

                await stream.WriteAsync(sizeData, 0, sizeData.Length);
                await stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception e)
            {
            }
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
            Debug.Log("Closed");
        }
    }
}

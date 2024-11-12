using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;
        // [size(ushort(2))][id(ushort(2))]
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;
            while (true)
            {
                // 적어도 패킷의 크기에 대한 정보는 필요함.
                if (buffer.Count < HeaderSize) break;

                int dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize) break; // 패킷이 덜왔음.

                // 패킷 처리
                OnRecvPacket(buffer.Slice(0, dataSize));

                processLen += dataSize;
                // 패킷을 처리한 만큼 정리.
                buffer = buffer.Slice(dataSize, buffer.Count - dataSize);
            }
            return processLen;
        }

        // 패킷을 조립해서 정리.
        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        Socket _socket;
        int _disconected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(1024);

        object _sendLock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _sendPendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Send(ArraySegment<byte> sendBuffer)
        {
            lock (_sendLock)
            {
                _sendQueue.Enqueue(sendBuffer);
                if (_sendPendingList.Count == 0) RegisterSend();
            }
        }

        public void Disconnect()
        {
            // Disconnect 여러번 방지.
            if (Interlocked.Exchange(ref _disconected, 1) == 1) return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region Network

        void RegisterSend()
        {
            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> sendBuffer = _sendQueue.Dequeue();
                _sendPendingList.Add(sendBuffer);
            }
            _sendArgs.BufferList = _sendPendingList;

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false) OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_sendLock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _sendPendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        if (_sendQueue.Count > 0) RegisterSend();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnRecvCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        void RegisterRecv()
        {
            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false) OnRecvCompleted(null, _recvArgs);
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // Write 진행. 버퍼를 읽음.
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    int processLength = OnRecv(_recvBuffer.ReadSegment);
                    // 음수거나 처리량이 실제 버퍼보다 크면 문제가 있음.
                    if (processLength < 0 || _recvBuffer.DataSize < processLength)
                    {
                        Disconnect();
                        return;
                    }

                    // 읽은 만큼 처리.
                    if (_recvBuffer.OnRead(processLength) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}

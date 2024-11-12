using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listner
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            _sessionFactory += sessionFactory;

            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(endPoint);
            _listenSocket.Listen(10);

            // Async 등록
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);

            // 대기 시작.
            RegisterAccept(args);
        }

        // Accept 대기
        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // 초기화
            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false) OnAcceptCompleted(null, args);
        }

        // Accept 완료
        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();

                session.Start(args.AcceptSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
            else Console.WriteLine(args.SocketError.ToString());

            // 완료 후 재등록
            RegisterAccept(args);
        }
    }
}

using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public class SocketServerManager
{
    public static SocketServerManager Instance
    {
        get
        {
            if (instance == null) instance = new SocketServerManager();
            return instance;
        }
    }

    private static SocketServerManager instance;
    Session session = null;

    public async void Connect(string host)
    {
        if (session != null && session.IsConnected) return;

        //var host = "localhost";
        var ipHost = Dns.GetHostEntry(host);
        var ipAddr = ipHost.AddressList[0];
        var port = 8888;
        var endPoint = new IPEndPoint(ipAddr, port);

        try
        {
            var client = new TcpClient();
            await client.ConnectAsync(host, port);

            if (client.Connected)
            {
                session = new Session(client);
                _ = session.StartReceieve();
                _ = ConnectingCheck();

                //UIController.Instance.ChangeScene(UIController.SceneName.SELECT_EXERCISE);
            }
        }
        catch (Exception e)
        {

        }
    }

    public async Task ConnectingCheck()
    {
        while (session.IsConnected) await Task.Yield();
        UIController.Instance.ChangeScene(UIController.SceneName.LOGIN); // 연결창으로 이동
    }

    public void Disconnect()
    {
        session.Close();
    }

    public void Send(Packet packet)
    {
        try
        {
            session.RegisterSend(packet);
        }
        catch (Exception e)
        {

        }
    }
}

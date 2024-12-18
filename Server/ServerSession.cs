using System.Net.Sockets;
using System.Text;
using NetCoreServer;
using Newtonsoft.Json;
using PEUtils;
using RedBlue_Server.Msg;

namespace RedBlue_Server.Server;

public class ChatServer : WsServer
{
    public ServerSession session;

    public ChatServer(string address, int port) : base(address, port)
    {
        PELog.ColorLog(LogColor.Magenta, $"服务器的ID为: {Id} ");
    }

    protected override TcpSession CreateSession()
    {
        return new ServerSession(this);
    }

    protected override void OnError(SocketError error)
    {
        PELog.ColorLog(LogColor.Magenta, $" 服务器错误: {error}");
    }
}

public class ServerSession : WsSession
{
    public static ChatServer chatServer;

    public ServerSession(WsServer server) : base(server)
    {
        chatServer = (ChatServer)server;
    }

    /// <summary>
    ///     连接建立
    /// </summary>
    /// <param name="request"></param>
    public override void OnWsConnected(HttpRequest request)
    {
        PELog.ColorLog(LogColor.Magenta, $"连接一个客户端， Id 为{Id} ");
        PELog.ColorLog(LogColor.Magenta, $"目前有{Server.Sessions.Count}个客户端连接 ");
        var message = $"你好客户端服务器已连接,这是你的GUID：{Id}请收好";
        var MSG = new C2SMsg
        {
            c2SMsgID = C2SMsgID.SendID,
            zbGuid = Id
        };
        var str = JsonConvert.SerializeObject(MSG);
        var bytes = Encoding.UTF8.GetBytes(str);
        ServerManager.Instance.AddMsgQue(Server.Sessions[Id], str);

        // var msg = JsonConvert.SerializeObject(s2cMsg);
        // chatServer.singlecastText(chatServer.Sessions[Id], Encoding.UTF8.GetBytes(msg), 0, Encoding.UTF8.GetBytes(msg).Length);
        ServerManager.Instance.heartBeatDic.Add(Id, 0);
    }

    /// <summary>
    ///     断开连接
    /// </summary>
    public override void OnWsDisconnected()
    {
        PELog.ColorLog(LogColor.Magenta, $"断开一个客户端， Id 为 {Id} ");
    }

    /// <summary>
    ///     接收到消息
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    public override void OnWsReceived(byte[] buffer, long offset, long size)
    {
        var str = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
        //PELog.ColorLog(LogColor.Magenta, $"收到客户端消息， Id 为 {Id}， 消息为 {str}");
        try
        {
            //添加到消息队列处理
            ServerManager.Instance.AddMsgQue(Server.Sessions[Id], str);
        }
        catch (Exception e)
        {
            PELog.ColorLog(LogColor.Red, $"发生错误， Id 为 {Id}， 错误为 {e.Message}");
        }
    }

    /// <summary>
    ///     错误处理
    /// </summary>
    /// <param name="error"></param>
    protected override void OnError(SocketError error)
    {
        PELog.ColorLog(LogColor.Magenta, $"发生错误， Id 为 {Id}， 错误码为 {error}");
    }
}
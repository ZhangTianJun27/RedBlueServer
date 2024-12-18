using System.Text;
using Newtonsoft.Json;
using PEUtils;
using RedBlue_Server.Server;

namespace RedBlue_Server;

public class SystemRoot<T> : SingletonBase<T> where T : new()
{
    public ServerManager serverMsg;

    public override void Init()
    {
        base.Init();
        serverMsg = ServerManager.Instance;
    }

    public override void Update()
    {
        base.Update();
    }


    //发送单人消息
    public virtual void SendSingleMsg<T>(T s2cMsg, Guid guid, string log)
    {
        try
        {
            PELog.ColorLog(LogColor.Blue, log);
            var jsonData = JsonConvert.SerializeObject(s2cMsg);
            var bytes = Encoding.UTF8.GetBytes(jsonData);
            serverMsg.server.singlecastText(serverMsg.server.Sessions[guid], bytes, 0, bytes.Length);
        }
        catch (Exception e)
        {
            SendNoGuid(guid);
        }
    }

    public bool SendNoGuid(Guid guid)
    {
        if (guid == new Guid("00000000-0000-0000-0000-000000000000"))
        {
            PELog.ColorLog(LogColor.Red, $"客户端{guid}，GUID为空，应该重新链接");
            return true;
        }
        return false;
    }

    //发送双人消息
    public virtual void SendDoubleMsg<T>(T s2cMsg, Guid guid, Guid guid2, string log)
    {
        try
        {
            PELog.ColorLog(LogColor.Blue, log);
            var jsonData = JsonConvert.SerializeObject(s2cMsg);
            var bytes = Encoding.UTF8.GetBytes(jsonData);
            serverMsg.server.singlecastText(serverMsg.server.Sessions[guid], bytes, 0, bytes.Length);
            serverMsg.server.singlecastText(serverMsg.server.Sessions[guid2], bytes, 0, bytes.Length);
        }
        catch (Exception e)
        {
            SendNoGuid(guid);
        }
    }
}
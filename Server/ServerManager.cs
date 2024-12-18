using System.Text;
using NetCoreServer;
using Newtonsoft.Json;
using PEUtils;
using RedBlue_Server.Msg;
using RedBlue_Server.System;

namespace RedBlue_Server.Server;

/// <summary>
///     组装消息包
/// </summary>
public class MsgPack
{
    public MsgPack(TcpSession tcpSession, string msgStr)
    {
        Session = tcpSession;
        this.msgStr = msgStr;
    }

    public TcpSession Session { get; set; }

    public string msgStr { get; set; }
}

public class ServerManager : SingletonBase<ServerManager>
{
    public static readonly string pkgque_lock = "pkgque_lock";
    private readonly Queue<MsgPack> msgPackQue = new();
    public Dictionary<Guid, int> heartBeatDic = new();
    public ChatServer server;

    public int TimeNum = 10000;


    public override void Init()
    {
        base.Init();
        var address = GameData.serverIPPath;
        var port = GameData.serverPort;
        var www = "../../../../../www/ws";
        PELog.ColorLog(LogColor.Magenta, $"WebSocket服务器端口: {port}");
        PELog.ColorLog(LogColor.Magenta, $"WebSocket服务器静态内容路径: {www}");
        PELog.ColorLog(LogColor.Magenta, $"WebSocket服务器网站: http://localhost:{port}/chat/index.html");
        // 创建一个WebSocket服务器
        server = new ChatServer(address, port);
        server.AddStaticContent(www, "/chat");
        Console.WriteLine($"WebSocket 服务器正在监听 {address}:{port}");
        // 启动服务器
        PELog.ColorLog(LogColor.Magenta, "服务器启动中...");
        server.Start();
        PELog.ColorLog(LogColor.Magenta, "完成");


        //Tools.Instance.TimerExample((uint)TimeNum, CheckHeartBeat, -1, 0);
    }


    /// <summary>
    ///     添加消息
    /// </summary>
    /// <param name="tcpSession"></param>
    /// <param name="msgStr"></param>
    public void AddMsgQue(TcpSession tcpSession, string msgStr)
    {
        lock (pkgque_lock)
        {
            msgPackQue.Enqueue(new MsgPack(tcpSession, msgStr));
        }
    }

    public override void Update()
    {
        base.Update();
        while (msgPackQue.Count > 0)
        {
            MsgPack msg;
            lock (pkgque_lock)
            {
                if (msgPackQue.Count == 0)
                    break;
                msg = msgPackQue.Dequeue();
                HandoutMsg(msg);
            }
        }
    }

    /// <summary>
    ///     检测心跳
    /// </summary>
    public void CheckHeartBeat(int id)
    {
        if (heartBeatDic.Count > 0)
            foreach (var session in server.Sessions)
            {
                if (heartBeatDic.ContainsKey(session.Key))
                {
                    PELog.ColorLog(LogColor.Red, $"检测{session.Key}心跳{heartBeatDic[session.Key]}次");
                    heartBeatDic[session.Key] += 1;
                }

                if (heartBeatDic[session.Key] == 3)
                {
                    heartBeatDic.Remove(session.Key);
                    session.Value.Disconnect();
                    PELog.ColorLog(LogColor.Red, $"一个客户端心跳包超时，已移除{session.Key}，还有{server.Sessions.Count}个客户端");
                }
            }
    }

    /// <summary>
    ///     更新心跳
    /// </summary>
    /// <param name="id"></param>
    public void UpdateHeartBeat(Guid id)
    {
        if (heartBeatDic.ContainsKey(id))
        {
            heartBeatDic[id] = 0;
            PELog.ColorLog(LogColor.Red, $"更新{id}心跳一次");
        }
    }


    /// <summary>
    ///     分发消息
    /// </summary>
    /// <param name="pack"></param>
    private void HandoutMsg(MsgPack pack)
    {
        if (pack.msgStr == "")
        {
            PELog.ColorLog(LogColor.Red, $"ID为{pack.Session.Id}的消息字符串为空");
            return;
        }

        var tempMsg = JsonConvert.DeserializeObject<dynamic>(pack.msgStr);
        if (tempMsg == null)
        {
            PELog.ColorLog(LogColor.Red, $"ID为{pack.Session.Id}的解析为空");
            return;
        }

        var messageId = (C2SMsgID)tempMsg.c2SMsgID;
        C2SMsg msg;
        switch (messageId)
        {
            case C2SMsgID.SendID:
                PELog.ColorLog(LogColor.Magenta, $"收到一个请求ID， Id 为{pack.Session.Id} ，目前还有{server.Sessions.Count}个玩家在线");
                msg = JsonConvert.DeserializeObject<C2SMsg>(pack.msgStr);
                var s2cMsg = new S2CMsg
                {
                    s2CMsgID = S2CMsgID.SendGUID,
                    zbguid = msg.zbGuid
                };
                SendSingleText(s2cMsg, msg.zbGuid);
                break;
            case C2SMsgID.HeartBeat:
                PELog.ColorLog(LogColor.Magenta, $"收到一个心跳包， Id 为{pack.Session.Id} ，目前还有{server.Sessions.Count}个玩家在线");
                SendHeartBeat(pack);
                break;
            case C2SMsgID.CreateCode:
                PELog.ColorLog(LogColor.Magenta, $"收到一个创建邀请码的请求， Id 为{pack.Session.Id} ");
                S2CCreateCode(pack);
                break;
            case C2SMsgID.Matchcode:
                PELog.ColorLog(LogColor.Magenta, $"收到一个匹配验证码的请求， Id 为{pack.Session.Id} ");
                C2SMatchCode(pack);
                break;
            case C2SMsgID.CreateRoom:
                PELog.ColorLog(LogColor.Magenta, $"收到一个创建房间的请求， Id 为{pack.Session.Id} ");
                S2CCreateRoom(pack);
                break;
            case C2SMsgID.AddPlayer:
                PELog.ColorLog(LogColor.Magenta, $"收到一个加入玩家请求， Id 为{pack.Session.Id} ");
                C2SAddPlayer(pack);
                break;
            case C2SMsgID.SendGifts:
                PELog.ColorLog(LogColor.Magenta, $"收到一个送礼请求， Id 为{pack.Session.Id} ");
                C2SSendGfit(pack);
                break;
            case C2SMsgID.SwitchGeneral:
                PELog.ColorLog(LogColor.Magenta, $"收到一个切换武将请求， Id 为{pack.Session.Id} ");
                C2SSwitchGeneral(pack);
                break;
            case C2SMsgID.AccordCityHP:
                PELog.ColorLog(LogColor.Magenta, $"收到一个主城改变HP的请求， Id 为{pack.Session.Id} ");
                C2SDecreaseAccordCityHP(pack);
                break;
            case C2SMsgID.SoldierDeath:
                PELog.ColorLog(LogColor.Magenta, $"收到一个士兵死亡的请求， Id 为{pack.Session.Id} ");
                C2SUpdateSolider(pack);
                break;
        }
    }

    /// <summary>
    ///     发送错误数据
    /// </summary>
    /// <param name="log"></param>
    /// <param name="guid"></param>
    public void SendSingleError(string log, Guid guid)
    {
        PELog.ColorLog(LogColor.Red, $"客户端{guid}，{log}");
        var s2cMsg = new S2CMsg
        {
            s2CMsgID = S2CMsgID.OnError,
            str = log
        };
        var jsonData = JsonConvert.SerializeObject(s2cMsg);
        var bytes = Encoding.UTF8.GetBytes(jsonData);
        server.singlecastText(server.Sessions[guid], bytes, 0, bytes.Length);
    }

    /// <summary>
    ///     发送单个会话消息
    /// </summary>
    /// <param name="s2cMsg"></param>
    /// <param name="guid"></param>
    /// <typeparam name="T"></typeparam>
    public void SendSingleText<T>(T s2cMsg, Guid guid)
    {
        var jsonData = JsonConvert.SerializeObject(s2cMsg);
        var bytes = Encoding.UTF8.GetBytes(jsonData);
        server.singlecastText(server.Sessions[guid], bytes, 0, bytes.Length);
    }

    /// <summary>
    ///     心跳
    /// </summary>
    /// <param name="pack"></param>
    public void SendHeartBeat(MsgPack pack)
    {
        var guid = pack.Session.Id;
        // 将 MsgPack 中的 c2sMsg 转换为具体的消息类型
        var c2sMsg = JsonConvert.DeserializeObject<C2SHeartBeat>(pack.msgStr);
        // 根据具体的 C2SMsg 类型选择处理逻辑
        if (c2sMsg != null)
        {
            var s2cMsg = new S2CHeartBeat
            {
                s2CMsgID = S2CMsgID.HeartBeat,
                str = $"{guid}心跳已更新"
            };
            //个人单播
            SendSingleText(s2cMsg, guid);
            //更新心跳
            UpdateHeartBeat(guid);
        }
        else
        {
            // 处理错误，不能处理该类型消息
            SendSingleError("转换消息类型C2SCreateRoom失败", guid);
        }
    }

    /// <summary>
    ///     创建邀请码
    /// </summary>
    /// <param name="pack"></param>
    public void S2CCreateCode(MsgPack pack)
    {
        var guid = pack.Session.Id;
        var c2sMsg = JsonConvert.DeserializeObject<C2SCreateCode>(pack.msgStr);
        if (c2sMsg != null)
        {
            PELog.ColorLog(LogColor.Magenta, $"Id 为{guid}的玩家要创建邀请码");
            MatchSys.Instance.CreateInvitationCode(6, c2sMsg);
        }
        else
        {
            SendSingleError("转换消息类型C2SJoinRoom失败", guid);
        }
    }

    /// <summary>
    ///     匹配验证码
    /// </summary>
    /// <param name="pack"></param>
    public void C2SMatchCode(MsgPack pack)
    {
        var guid = pack.Session.Id;
        var c2sMsg = JsonConvert.DeserializeObject<C2SMatchjoin>(pack.msgStr);
        if (c2sMsg != null)
        {
            if (MatchSys.Instance.zbDataDic.ContainsKey(c2sMsg.code))
            {
                PELog.ColorLog(LogColor.Magenta, $"Id 为{c2sMsg.code} 的房间存在");
                MatchSys.Instance.MatchJoin(c2sMsg);
            }
            else
            {
                SendSingleError($"邀请码{c2sMsg.code}不存在", guid);
            }
        }
        else
        {
            SendSingleError("转换消息类型C2SJoinRoom失败", guid);
        }
    }

    /// <summary>
    ///     创建房间
    /// </summary>
    /// <param name="pack"></param>
    public void S2CCreateRoom(MsgPack pack)
    {
        var guid = pack.Session.Id;
        var c2sMsg = JsonConvert.DeserializeObject<C2SCreateRoom>(pack.msgStr);
        if (c2sMsg != null)
        {
            if (MatchSys.Instance.zbDataDic.ContainsKey(c2sMsg.roomId))
            {
                PELog.ColorLog(LogColor.Magenta, $"Id 为{c2sMsg.roomId} 的房间存在");
                RoomSys.Instance.CreateRoom(c2sMsg);
            }
            else
            {
                SendSingleError($"邀请码{c2sMsg.roomId}不存在", guid);
            }
        }
        else
        {
            SendSingleError("转换消息类型C2SCreateRoom失败", guid);
        }
    }

    /// <summary>
    ///     添加玩家
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="msg"></param>
    public void C2SAddPlayer(MsgPack pack)
    {
        var guid = pack.Session.Id;
        var c2sMsg = JsonConvert.DeserializeObject<C2SAddPlayer>(pack.msgStr);
        if (c2sMsg != null)
        {
            if (RoomSys.Instance.pvpRoomDic.ContainsKey(c2sMsg.RoomId))
            {
                PELog.ColorLog(LogColor.Magenta, $"Id 为{c2sMsg.RoomId} 的房间存在,要添加玩家");
                RoomSys.Instance.FindRoomAddPlayer(c2sMsg);
            }
            else
            {
                SendSingleError($"{guid}房间不存在", guid);
            }
        }
        else
        {
            SendSingleError("转换消息类型C2SAddPlayer失败", guid);
        }
    }

    /// <summary>
    ///     送礼物
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="msg"></param>
    public void C2SSendGfit(MsgPack pack)
    {
        var guid = pack.Session.Id;
        var c2sMsg = JsonConvert.DeserializeObject<C2SGiveGifts>(pack.msgStr);
        if (c2sMsg != null)
        {
            if (RoomSys.Instance.pvpRoomDic.ContainsKey(c2sMsg.RoomId))
            {
                PELog.ColorLog(LogColor.Magenta, $"Id 为{c2sMsg.RoomId} 的房间存在,玩家{guid}开始送礼物");
                RoomSys.Instance.FindRoomAddGift(c2sMsg);
            }
            else
            {
                SendSingleError($"{guid}房间不存在", guid);
            }
        }
        else
        {
            SendSingleError("转换消息类型C2SGiveGifts失败", guid);
        }
    }


    /// <summary>
    ///     更换武将
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="msg"></param>
    public void C2SSwitchGeneral(MsgPack msg)
    {
        var guid = msg.Session.Id;
        var c2sMsg = JsonConvert.DeserializeObject<C2SSwitchGeneral>(msg.msgStr);
        if (c2sMsg != null)
        {
            if (RoomSys.Instance.pvpRoomDic.ContainsKey(c2sMsg.RoomId))
            {
                PELog.ColorLog(LogColor.Red, $"玩家{c2sMsg.uid}要切换武将{c2sMsg.type}");
                RoomSys.Instance.FindRoomSwitchGeneral(c2sMsg);
            }
            else
            {
                SendSingleError($"{guid}房间不存在", guid);
            }
        }
        else
        {
            SendSingleError("转换消息类型C2SAccordCityHPData失败", guid);
        }
    }


    /// <summary>
    ///     主城减血
    /// </summary>
    /// <param name="msg"></param>
    public void C2SDecreaseAccordCityHP(MsgPack msg)
    {
        var guid = msg.Session.Id;
        var c2sMsg = JsonConvert.DeserializeObject<C2SAccordCityHPData>(msg.msgStr);
        if (c2sMsg != null)
        {
            if (RoomSys.Instance.pvpRoomDic.ContainsKey(c2sMsg.RoomId))
            {
                PELog.ColorLog(LogColor.Red, $"客户端{guid}，主城的掉血  阵营为{c2sMsg.accordCityHPData.campType}");
                RoomSys.Instance.FindRoomCityHP(c2sMsg);
            }
            else
            {
                SendSingleError($"{guid}房间不存在", guid);
            }
        }
        else
        {
            SendSingleError("转换消息类型C2SAccordCityHPData失败", guid);
        }
    }


    /// <summary>
    ///     士兵死亡
    /// </summary>
    /// <param name="msg"></param>
    public void C2SUpdateSolider(MsgPack msg)
    {
        var guid = msg.Session.Id;
        var c2sMsg = JsonConvert.DeserializeObject<C2SUpdateSoldierDeath>(msg.msgStr);
        if (c2sMsg != null)
        {
            if (RoomSys.Instance.pvpRoomDic.ContainsKey(c2sMsg.RoomId))
            {
                PELog.ColorLog(LogColor.Red, $"客户端{guid}，{c2sMsg.campType}方的士兵死亡，类型为{c2sMsg.unitType}");
                RoomSys.Instance.FindRoomUpdateSolider(c2sMsg);
            }
            else
            {
                SendSingleError($"{guid}房间不存在", guid);
            }
        }
        else
        {
            SendSingleError("转换消息类型C2SAccordCityHPData失败", guid);
        }
    }
}
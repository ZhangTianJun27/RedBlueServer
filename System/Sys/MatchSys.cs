using RedBlue_Server.Msg;

namespace RedBlue_Server.System;

/// <summary>
///     匹配系统
/// </summary>
public class MatchSys : SystemRoot<MatchSys>
{
    private readonly Random random = new();
    private readonly List<string> GetRoomCode = new();
    private readonly Dictionary<string, int> GetRoomCodeDic = new();

    /// <summary>
    ///     邀请码字典
    /// </summary>
    public Dictionary<string, List<ZBData>> zbDataDic = new();


    public override void Init()
    {
        base.Init();
    }


    public override void Update()
    {
        base.Update();
    }


    /// <summary>
    ///     房主创建邀请码
    /// </summary>
    public void CreateInvitationCode(int num, C2SCreateCode msg)
    {
        var code = GenerateInvitationCode(num);
        zbDataDic.Add(code, new List<ZBData> { msg.zbData });
        var s2c = new S2CInvitationCode
        {
            s2CMsgID = S2CMsgID.CreateCode,
            sendInvitationCode = new SendInvitationCode
            {
                hostMonst = true,
                code = code
            }
        };
        SendSingleMsg(s2c, msg.zbData.guid, $"向ID为  {msg.zbData.guid}  发送创建邀请码信息,邀请码为{code}");
    }


    /// <summary>
    ///     第二人匹配加入
    /// </summary>
    public void MatchJoin(C2SMatchjoin msg)
    {
        //匹配的人
        zbDataDic[msg.code].Add(msg.zbData);
        var s2c = new S2CMatchInfo
        {
            s2CMsgID = S2CMsgID.MatchSuccess,
            masterData = zbDataDic[msg.code][0],
            matchData = zbDataDic[msg.code][1]
        };
        SendDoubleMsg(s2c, zbDataDic[msg.code][0].guid, zbDataDic[msg.code][1].guid, "匹配成功，推送二人数据");
    }


    /// <summary>
    ///     获取邀请码
    /// </summary>
    public string GenerateInvitationCode(int length)
    {
        const string chars = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        string invitationCode;
        do
        {
            invitationCode = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        } while (GetRoomCode.Contains(invitationCode));

        GetRoomCode.Add(invitationCode);
        GetRoomCodeDic.Add(invitationCode, 0); // 假设每个邀请码初始值为0
        return invitationCode;
    }

    /// <summary>
    ///     移除邀请码
    /// </summary>
    /// <param name="invitationCode"></param>
    public void RecycleInvitationCode(string invitationCode)
    {
        if (GetRoomCode.Contains(invitationCode))
        {
            GetRoomCode.Remove(invitationCode);
            GetRoomCodeDic.Remove(invitationCode);
            Console.WriteLine($"邀请码 {invitationCode} 已被回收。");
        }
        else
        {
            Console.WriteLine($"邀请码 {invitationCode} 不存在，无法回收。");
        }
    }
}
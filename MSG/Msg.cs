using RedBlue_Server.System;

namespace RedBlue_Server.Msg;

#region C2S

/// <summary>
///     消息类型
/// </summary>
[Serializable]
public enum C2SMsgID
{
    OnWsConnected,
    OnWsDisconnected,
    OnWsReceived,
    OnError,

    //心跳
    HeartBeat,

    //ID
    SendID,

    //创建邀请码
    CreateCode,

    //匹配验证码
    Matchcode,

    //创建房间
    CreateRoom,

    //添加玩家
    AddPlayer,

    //礼物
    SendGifts,

    //更新士兵死亡
    SoldierDeath,

    //同步主城血量
    AccordCityHP,

    //切换武将
    SwitchGeneral,

    //武将释放技能
    GeneralSkill,

    //武将死亡
    GeneralDeath,


    //特殊玩法
    SpecialIdeas,

    //组合技
    CombinationSkill,


    //对局时间结束
    GameTimeEnd,


}

/// <summary>
///     C2S消息基类
/// </summary>
[Serializable]
public class C2SMsg
{
    public C2SMsgID c2SMsgID;
    public string str;
    public Guid zbGuid;
}

/// <summary>
///     心跳
/// </summary>
[Serializable]
public class C2SHeartBeat : C2SMsg
{
}

/// <summary>
///     创建邀请码
/// </summary>
[Serializable]
public class C2SCreateCode : C2SMsg
{
    public ZBData zbData;
}

/// <summary>
///     匹配验证码
/// </summary>
[Serializable]
public class C2SMatchjoin : C2SMsg
{
    public string code;
    public ZBData zbData;
}

/// <summary>
///     匹配验证码
/// </summary>
[Serializable]
public class C2SJoinRoom : C2SMsg
{
    public JoinRoom joinRoom;
}

/// <summary>
///     创建房间
/// </summary>
public class C2SCreateRoom : C2SMsg
{
    public Guid blueId;
    public GameModel gameModel;
    public Guid redId;
    public string roomId;
}

/// <summary>
///     添加玩家
/// </summary>
[Serializable]
public class C2SAddPlayer : C2SMsg
{
    public AddPlayerData addPlayer;
    public string RoomId;
}

/// <summary>
///     赠送礼物
/// </summary>
[Serializable]
public class C2SGiveGifts : C2SMsg
{
    public GiftData giftData;
    public string RoomId;
}

/// <summary>
///     更新士兵死亡
/// </summary>
[Serializable]
public class C2SUpdateSoldierDeath : C2SMsg
{
    public CampType campType;
    public int count;
    public string RoomId;
    public GiftConvertSoldiers soldierData;
    public UnitType unitType;
}

/// <summary>
///     更新主城血量
/// </summary>
public class C2SAccordCityHPData : C2SMsg
{
    public AccordCityHPData accordCityHPData;
    public string RoomId;
}

/// <summary>
///     切换武将
/// </summary>
public class C2SSwitchGeneral : C2SMsg
{
    public string RoomId;
    public int playerindex;
    public string uid;
    public GeneralType type;
    public CampType campType;
}

#endregion

//=================================================================================================================================================================================

#region S2C

/// <summary>
///     消息类型
/// </summary>
[Serializable]
public enum S2CMsgID
{
    OnWsConnected,
    OnWsDisconnected,
    OnWsReceived,
    OnError,

    //心跳
    HeartBeat,

    //ID
    SendGUID,

    //创建邀请码
    CreateCode,

    //匹配成功
    MatchSuccess,

    //房间开始
    RoomStart,

    //添加玩家
    AddPlayer,

    //礼物转换为士兵
    GiftToSoldier,

    //召唤武将
    CallGeneral,

    //更新士兵死亡
    UpdateSoldierDeath,

    //同步主城血量
    AccordCityHPData,

    //主城血量阶段
    CityHPStage,


    //更换武将
    SwitchGeneral,


    //获得随机装备
    RandomEquipment,

    //武将升级
    GeneralUpGrade,

    //武器获得经验/升级
    
    
    //对局结束
    PVPRoomEnd,
    
}

/// <summary>
///     S2C消息基类
/// </summary>
[Serializable]
public class S2CMsg
{
    public S2CMsgID s2CMsgID;
    public string str;
    public Guid zbguid;
}

/// <summary>
///     回应心跳
/// </summary>
[Serializable]
public class S2CHeartBeat : S2CMsg
{
}

/// <summary>
///     发送GUID
/// </summary>
[Serializable]
public class S2CSendGUID : S2CMsg
{
    public Guid guid;
}

/// <summary>
///     发送邀请码
/// </summary>
[Serializable]
public class S2CInvitationCode : S2CMsg
{
    public SendInvitationCode sendInvitationCode;
}

/// <summary>
///     发送成功匹配玩家的信息
/// </summary>
[Serializable]
public class S2CMatchInfo : S2CMsg
{
    public ZBData masterData;
    public ZBData matchData;
}

/// <summary>
///     发送房间成功开始
/// </summary>
[Serializable]
public class S2CRoomStart : S2CMsg
{
    public bool isSuccess;
}

/// <summary>
///     发送添加玩家
/// </summary>
[Serializable]
public class S2CAddPlayer : S2CMsg
{
    public AddPlayerData addPlayer;
    public string RoomId;
}

/// <summary>
///     推送礼物转换为士兵数据
/// </summary>
[Serializable]
public class S2CGiftToSoldier : S2CMsg
{
    public GiftConvertSoldiers soldierData;
}

/// <summary>
///     召唤武将
/// </summary>
[Serializable]
public class S2CCallGeneral : S2CMsg
{
    public General generalData;
}

/// <summary>
///     更新士兵死亡
/// </summary>
public class S2CUpdateSoldierDeath : S2CMsg
{
    public CampType campType;
    public int count;
    public UnitType unitType;
}

/// <summary>
///     更新主城血量
/// </summary>
public class S2CAccordCityHPData : S2CMsg
{
    public AccordCityHPData accordCityHPData;
}

/// <summary>
///     切换武将
/// </summary>
public class S2CSwitchGeneral : S2CMsg
{
    public General data;
}

/// <summary>
///     更新主城血量阶段
/// </summary>
public class S2CCityHPStage : S2CMsg
{
    public bool CallBackBuffer; //反击Buff
    public CampType campType;
    public CityState state;
}

/// <summary>
///     随机装备
/// </summary>
public class S2CRandomEquipment : S2CMsg
{
    public WeaponBase weapon;
}



/// <summary>
/// 对局结束
/// </summary>
public class S2CGameEnd :S2CMsg
{
    
}

#endregion

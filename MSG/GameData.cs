namespace RedBlue_Server.Msg;

public class GameData
{
    public const string serverIPPath = "127.0.0.1";
    public const int serverPort = 8080;
    public const string linkMySQLPath = "Server=127.0.0.1;Database=mynettestlib;User ID=root;Password=123456;Port=3306;SslMode=none;";

    
    public const string weekRank = "本周排行";
    public const string monthRank = "本月排行";
    public const string allPlayerData = "所有玩家数据";
    public const string GeneralRank = "本月武将排行";
    
    
    
    
    
    
}

/// <summary>
///     添加的玩家的数据
/// </summary>
public class AddPlayerData
{
    public string avatar;
    public string content;
    public string nickname;
    public string uid;
}

/// <summary>
///     阵营
/// </summary>
public enum CampType
{
    Red,
    Blue
}

/// <summary>
///     单位类型
/// </summary>
public enum UnitType
{
    Unit_dzb_1,
    Unit_zs_2,
    Unit_gjs_3,
    Unit_ck_4,
    Unit_qb_5,
    Unit_pao_6,
    Unit_Dun_7,
    Unit_shou_8,
    Unit_xiuluo_9,
    Unit_Hero,
    UnitBoss
}

/// <summary>
///     礼物类型
/// </summary>
public enum GiftEnum
{
    点赞,
    评论666,
    仙女棒,
    能量药丸,
    魔法镜,
    甜甜圈,
    能量电池,
    恶魔炸弹,
    神秘空投,
    超能喷射
}

/// <summary>
///     礼物数据
/// </summary>
public class GiftData
{
    public string avatar;
    public CampType campType;
    public int count;
    public GiftEnum gift;
    public int giftvalue;
    public string nickName;
    public string uid;
}

/// <summary>
///     主播数据
/// </summary>
public class ZBData
{
    public byte[] avatar;
    public CampType campType;
    public Guid guid;
    public string Nickname;
}

/// <summary>
///     礼物转化
/// </summary>
public class GiftConvertSoldiers
{
    public string avatar;
    public CampType campType;
    public GiftEnum giftType;
    public int giftvalue;
    public int scoreNum;
    public int soldiersCount;
    public UnitType unitType;
    public string WhoSoldiers;
}

/// <summary>
///     士兵数据
/// </summary>
public class SoldiersData
{
    public int soldiersCount;
    public List<SoldiersInfomation> soldiersList;
}

/// <summary>
///     士兵信息
/// </summary>
public class SoldiersInfomation
{
    public float[] float3;
    public bool isDeath;
    public uint soldiersId;
}

/// <summary>
///     游戏模式
/// </summary>
public enum GameModel
{
    Easy,
    Normal,
    Hard
}

/// <summary>
///     创建房间
/// </summary>
public class CreateRoom
{
    public GameModel gameModel;
    public ZBData zbdata;
}

/// <summary>
///     加入房间
/// </summary>
public class JoinRoom
{
    public string GetInvitationCode;
    public ZBData zbdata;
}

/// <summary>
///     游戏开始
/// </summary>
public class RoomStart
{
    public string code;
    public ZBData ONE;
    public ZBData TOW;
}

/// <summary>
///     主播Guid
/// </summary>
public class ZBGUID
{
    public Guid guid;
}

/// <summary>
///     发送创建房间邀请码
/// </summary>
public class SendInvitationCode
{
    public string code;
    public bool hostMonst;
}

/// <summary>
///     房间开始
/// </summary>
public class SendRoomStart
{
}

/// <summary>
///     同步主城血量数据
/// </summary>
public class AccordCityHPData
{
    public float attackHp;
    public CampType campType;
    public float HP;
}

/// <summary>
///     每局排行
/// </summary>
public class EveryRankData
{
    public List<PlayerRankData> data;
    public string ZBname;
}

/// <summary>
///     每周排行
/// </summary>
public class WeekRankData
{
    public List<PlayerRankData> data;
}

/// <summary>
///     每月排行
/// </summary>
public class MonthRankData
{
    public List<PlayerRankData> data;
}

/// <summary>
///     用户排行数据
/// </summary>
public class PlayerRankData
{
    public int lastRankId;
    public string name;
    public int rankId;
    public float score;
}
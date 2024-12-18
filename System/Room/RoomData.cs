using RedBlue_Server.Msg;
using RedBlue_Server.System;

namespace RedBlue_Server;

/// <summary>
///     房间数据
/// </summary>
public class RoomData
{
    public ZBData Blue;

    public BlueCity BlueCity;

    public Dictionary<UnitType, int> BlueUnitDic;

    public int BlueUnitNum;

    public int BlueWinsPool = 0;

    /// <summary>
    ///     本局双方玩家
    /// </summary>
    public Dictionary<CampType, List<PlayerData>> CampPlayerListDic;

    /// <summary>
    ///     本局游戏最大血量
    /// </summary>
    public float cityMaxHP;

    public CampType Loser;

    /// <summary>
    ///     本局玩家列表
    /// </summary>
    public List<PlayerData> PlayerList;

    /// <summary>
    ///     本局玩家排行
    /// </summary>
    public List<PlayerRankData> PlayerRankList;

    /// <summary>
    ///     主播
    /// </summary>
    public ZBData Red;


    /// <summary>
    ///     双方主城
    /// </summary>
    public RedCity RedCity;

    /// <summary>
    ///     双方兵力
    /// </summary>
    public Dictionary<UnitType, int> RedUnitDic;

    /// <summary>
    ///     双方兵力总数
    /// </summary>
    public int RedUnitNum;

    /// <summary>
    ///     连胜池
    /// </summary>
    public int RedWinsPool = 0;

    /// <summary>
    ///     房间ID
    /// </summary>
    public string RoomID;

    /// <summary>
    ///     本局游戏的积分池
    /// </summary>
    public int ScorePool = 0;

    /// <summary>
    ///     主城阶段改变阈值
    /// </summary>
    public double ThresholdValue_One = 0.666;

    public double ThresholdValue_Tow = 0.333;

    /// <summary>
    ///     记时差
    /// </summary>
    public int TimeLimit;

    /// <summary>
    ///     本局可争夺连胜数
    /// </summary>
    public int WinLimit;

    //赢家
    public CampType Winner;

    /// <summary>
    ///     限时时间
    /// </summary>
    public DateTime BeginTime { get; set; }

    public DateTime EndTime { get; set; }
    
    //对局结束
    public bool IsOver { get; set; }


    public void Init(GameModel gameModel, string id)
    {
        CampPlayerListDic = new Dictionary<CampType, List<PlayerData>>
        {
            { CampType.Red, new List<PlayerData>() },
            { CampType.Blue, new List<PlayerData>() }
        };
        PlayerList = new List<PlayerData>();
        RedUnitDic = new Dictionary<UnitType, int>
        {
            { UnitType.Unit_dzb_1, 0 },
            { UnitType.Unit_zs_2, 0 },
            { UnitType.Unit_gjs_3, 0 },
            { UnitType.Unit_ck_4, 0 },
            { UnitType.Unit_qb_5, 0 },
            { UnitType.Unit_pao_6, 0 },
            { UnitType.Unit_Dun_7, 0 },
            { UnitType.Unit_shou_8, 0 },
            { UnitType.Unit_xiuluo_9, 0 },
            { UnitType.Unit_Hero, 0 }
        };
        BlueUnitDic = new Dictionary<UnitType, int>
        {
            { UnitType.Unit_dzb_1, 0 },
            { UnitType.Unit_zs_2, 0 },
            { UnitType.Unit_gjs_3, 0 },
            { UnitType.Unit_ck_4, 0 },
            { UnitType.Unit_qb_5, 0 },
            { UnitType.Unit_pao_6, 0 },
            { UnitType.Unit_Dun_7, 0 },
            { UnitType.Unit_shou_8, 0 },
            { UnitType.Unit_xiuluo_9, 0 },
            { UnitType.Unit_Hero, 0 }
        };
        Red = new ZBData();
        Blue = new ZBData();
        RedCity = new RedCity();
        BlueCity = new BlueCity();
        PlayerRankList = new List<PlayerRankData>();
        RoomID = id;
        SelectMode(gameModel);
    }

    /// <summary>
    ///     选择模式
    /// </summary>
    /// <param name="gameModel"></param>
    public void SelectMode(GameModel gameModel)
    {
        switch (gameModel)
        {
            case GameModel.Easy:
                cityMaxHP = 10000;
                TimeLimit = 30;
                break;
            case GameModel.Normal:
                cityMaxHP = 18888;
                TimeLimit = 60;
                break;
            case GameModel.Hard:
                cityMaxHP = 66666;
                TimeLimit = 90;
                break;
        }
    }

    /// <summary>
    ///     清理房间数据
    /// </summary>
    public void Clear()
    {
        CampPlayerListDic.Clear();
        PlayerList.Clear();
        RedUnitDic.Clear();
        BlueUnitDic.Clear();
        Red = null;
        Blue = null;
        RedCity = null;
        BlueCity = null;
        PlayerRankList.Clear();
    }
}
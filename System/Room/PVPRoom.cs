using System.Text;
using Newtonsoft.Json;
using PEUtils;
using RedBlue_Server.Msg;
using RedBlue_Server.Server;
using RedBlue_Server.System;

namespace RedBlue_Server;

public abstract class RoomState
{
    protected PVPRoom room;

    protected RoomState(PVPRoom room)
    {
        this.room = room;
    }

    public abstract void Start();
    public abstract void Update();
    public abstract void Clear();
}

/// <summary>
///     等待
/// </summary>
public class RoomStateWait : RoomState
{
    public RoomStateWait(PVPRoom room) : base(room)
    {
    }

    public override void Start()
    {
        // 处理开始逻辑
        room.roomData.RedCity.currentState = CityState.start;
        room.roomData.BlueCity.currentState = CityState.start;
        room.roomData.RedCity.HP = room.roomData.cityMaxHP;
        room.roomData.BlueCity.HP = room.roomData.cityMaxHP;
        room.roomData.BeginTime = DateTime.Now;
        room.roomData.EndTime = DateTime.Now.AddMinutes(room.roomData.TimeLimit);
        PELog.ColorLog(LogColor.Blue, $"{room.roomData.RoomID}等待房间开始，初始化了一些房间数据");
    }

    public override void Update()
    {
        //第一个玩家进入之后变为战斗状态
    }

    public override void Clear()
    {
        PELog.ColorLog(LogColor.Blue, $"{room.roomData.RoomID}等待房间结束");
    }
}

/// <summary>
///     战斗
/// </summary>
public class RoomStateFight : RoomState
{
    public RoomStateFight(PVPRoom room) : base(room)
    {
    }

    public override void Start()
    {
        PELog.ColorLog(LogColor.Blue, $"{room.roomData.RoomID}战斗房间开始");
    }

    public override void Update()
    {
        //任意一方主城血量清零 变为结束状态
    }

    public override void Clear()
    {
        PELog.ColorLog(LogColor.Blue, $"{room.roomData.RoomID}战斗房间结束");
    }
}

/// <summary>
///     结束
/// </summary>
public class RoomStateEnd : RoomState
{
    public RoomStateEnd(PVPRoom room) : base(room)
    {
    }

    public override void Start()
    {
        PELog.ColorLog(LogColor.Blue, $"{room.roomData.RoomID}结束房间开始");
        //输赢
        if (room.roomData.RedCity.HP <= 0)
        {
            room.roomData.Winner = CampType.Blue;
            room.roomData.Loser = CampType.Red;
        }
        else if (room.roomData.BlueCity.HP <= 0)
        {
            room.roomData.Loser = CampType.Blue;
            room.roomData.Winner = CampType.Red;
        }

        //争夺连胜
        room.WinningStreak();
        //玩家瓜分积分
        room.CarveUpScore();
        //排行榜排序
        room.RankPlayer();
        Console.WriteLine(room.roomData.PlayerList);
        //每局积分排行榜
        DataSys.Instance.ScoreEveryDayRank(room.roomData.PlayerRankList);
        //实时更新周积分
        DataSys.Instance.UpdateWeekRank(room.roomData.PlayerRankList);
        //月积分榜
        DataSys.Instance.UpdateMonthRank(room.roomData.PlayerRankList);
        //开启战利品，随机获得一件装备   装备获得经验，前三名高额经验，展示经验提升和装备品质和等级   
        room.GetRoodomWeapon();
        //更新武将战力  ，以及武将世界排名     武将战力  =  装备（品质+等级）*个数  +  （武将等级+将魂）*个数
        List<GeneralRankData> generalRank = GeneralSys.Instance.CalcuteGeneralScore(room.roomData.PlayerList);
        DataSys.Instance.UpdateGeneralRank(GameData.GeneralRank, generalRank);

        //更新所有玩家数据
        var list = new List<MySQLPlayerData>();
        foreach (var item in room.roomData.PlayerList) list.Add(item.mySQLPlayerData);
        DataSys.Instance.UpdateAllPlayerData(GameData.allPlayerData, list);
    

        //发送对局结束消息
        room.GameEnd();

        room.roomData.IsOver = true;
    }

    public override void Update()
    {
    }

    public override void Clear()
    {
        PELog.ColorLog(LogColor.Blue, $"{room.roomData.RoomID}结束房间清理");
    }
}

public class PVPRoom
{
    private RoomState currentState;
    public bool isStart;
    public RoomData roomData;

    public PVPRoom(string roomID, GameModel gameModel)
    {
        roomData = new RoomData();
        roomData.Init(gameModel, roomID);
        currentState = new RoomStateWait(this); // 初始状态为等待
    }

    /// <summary>
    ///     开始房间
    /// </summary>
    public void StartRoom()
    {
        PELog.ColorLog(LogColor.Yellow, $"id为{roomData.RoomID}的房间开始了，现在为等待状态，等待第一个玩家加入进入战斗状态");
        currentState.Start();
        isStart = true;

        //开启定时器，每10秒自动点赞
        Tools.Instance.TimerExample((uint)10000, AutoLike, -1, 0);
    }

    /// <summary>
    ///     更新房间
    /// </summary>
    public void UpdateRoom()
    {
        if (isStart)
        {
            //如果玩家数量大于0且当前状态为等待状态，则改变为战斗状态
            if (roomData.PlayerList.Count > 0 && currentState is RoomStateWait) ChangeState(new RoomStateFight(this));

            //如果战斗状态，任意一方主城血量清零 变为结束状态
            if ((roomData.RedCity.HP <= 0 && currentState is RoomStateFight) ||
                (roomData.BlueCity.HP <= 0 && currentState is RoomStateFight))
                ChangeState(new RoomStateEnd(this));

            //PELog.ColorLog(LogColor.Yellow, $"现在时间   {DateTime.Now}    结束时间{roomData.EndTime}");
            //如果战斗状态,时间归零没有分出胜负 ，血量少的一方失败,血量相同谁的积分池高谁赢
            if (DateTime.Now >= roomData.EndTime) ChangeState(new RoomStateEnd(this));

            currentState.Update();
        }
    }

    /// <summary>
    /// 送礼物的自动点赞
    /// </summary>
    /// <param name="num"></param>
    public void AutoLike(int num)
    {
        //本局送礼  自动点赞的人
        foreach (var item in roomData.PlayerList)
        {
            if (item.IsOnLike)
            {
                GiftData gift = new GiftData()
                {
                    avatar = "",
                    campType = item.campType,
                    count = 1,
                    gift = GiftEnum.点赞,
                    giftvalue = 0,
                    nickName = item.mySQLPlayerData.Nickname,
                    uid = item.mySQLPlayerData.Uid.ToString(),
                };
                GiftConversionSolider(gift);
            }
        }

        PELog.ColorLog(LogColor.Yellow, $"自动送礼更新一次");
    }


    public void AutoLike60(int num)
    {
        GiftData gift = new GiftData()
        {
            avatar = "",
            campType = roomData.PlayerList.Last().campType,
            count = 1,
            gift = GiftEnum.点赞,
            giftvalue = 0,
            nickName = roomData.PlayerList.Last().mySQLPlayerData.Nickname,
            uid = roomData.PlayerList.Last().mySQLPlayerData.Uid.ToString(),
        };
        GiftConversionSolider(gift);
        PELog.ColorLog(LogColor.Yellow, $"{num}{num}{num}{num}{num}{num}{num}{num}{num}{num}{num}{num}{num}{num}{num}{num}{num}{num}");
        PELog.ColorLog(LogColor.Yellow, $"60000000000000000000000000000000");
    }

    /// <summary>
    ///     结束房间
    /// </summary>
    public void ClearRoom()
    {
        PELog.ColorLog(LogColor.Yellow, $"id为{roomData.RoomID}的房间结束了");
        // 清理房间状态
        //currentState.Clear();
        currentState = null;
        // 清理房间数据
        roomData.Clear();
    }

    /// <summary>
    ///     改变房间状态
    /// </summary>
    /// <param name="newState"></param>
    public void ChangeState(RoomState newState)
    {
        PELog.ColorLog(LogColor.Yellow, $"id为{roomData.RoomID}改变了房间状态,现在是{newState}");
        currentState = newState;
        currentState.Start();
    }


    /// <summary>
    ///     添加玩家
    /// </summary>
    /// <param name="playerData"></param>
    public void AddPlayer(AddPlayerData playerData)
    {
        //如果是参与过的玩家从数据库中获取     没有则创建  
        PlayerData data;
        var mysqlData = PlayerSys.Instance.QueryPlayer(GameData.allPlayerData, playerData.uid);
        if (mysqlData.Uid == 0)
            //创建新玩家
            data = new PlayerData
            {
                campType = (CampType)int.Parse(playerData.content),
                mySQLPlayerData = new MySQLPlayerData
                {
                    AvatarUrl = playerData.avatar,
                    Nickname = playerData.nickname,
                    Uid = int.Parse(playerData.uid)
                }
            };
        else
            //取出老玩家
            data = new PlayerData
            {
                campType = (CampType)int.Parse(playerData.content),
                mySQLPlayerData = mysqlData,
            };
        
        //玩家每日登录
        PlayerSys.Instance.EveryDayLogin(data);


        //判断玩家加入阵营
        if (Enum.TryParse<CampType>(playerData.content, out var campType))
        {
            roomData.CampPlayerListDic[campType].Add(data);
            roomData.PlayerList.Add(data);
            PELog.ColorLog(LogColor.Yellow, $"id为{playerData.nickname}的玩家加入了{campType}方");
        }
        else
        {
            PELog.ColorLog(LogColor.Red, $"玩家{playerData.nickname}的加入{campType}方失败");
            return;
        }

        PELog.ColorLog(LogColor.Yellow, $"目前红方有{roomData.CampPlayerListDic[CampType.Red].Count},蓝方有{roomData.CampPlayerListDic[CampType.Blue].Count}");


        var msg = new S2CAddPlayer
        {
            s2CMsgID = S2CMsgID.AddPlayer,
            RoomId = roomData.RoomID,
            addPlayer = new AddPlayerData
            {
                uid = playerData.uid,
                content = playerData.content,
                nickname = playerData.nickname,
                avatar = playerData.avatar
            }
        };
        RoomSys.Instance.SendDoubleMsg(msg, roomData.Red.guid, roomData.Blue.guid, $"{roomData.RoomID}房间添加了一个玩家");


        //Tools.Instance.TimerExample((uint)1000, AutoLike60, 60, 0);
    }

    /// <summary>
    ///     移除玩家
    /// </summary>
    /// <param name="playerData"></param>
    public void RemovePlayer(AddPlayerData playerData)
    {
        //roomData.CampPlayerListDic[playerData.Ctype].Remove(playerData);
    }

    /// <summary>
    ///     更新某个玩家的数据
    /// </summary>
    /// <param name="type"></param>
    /// <param name="uid"></param>
    /// <param name="giftData"></param>
    /// <param name="scoreNum"></param>
    public void UpdatePlayerData(CampType type, string uid, GiftData giftData, int scoreNum)
    {
        foreach (var item in roomData.CampPlayerListDic[type])
            if (item.mySQLPlayerData.Uid == int.Parse(uid))
            {
                //个人积分
                item.score += scoreNum;
                //礼物记录
                item.mySQLPlayerData.giftDic[giftData.gift] += giftData.count;
                PELog.ColorLog(LogColor.Magenta, $"{item.mySQLPlayerData.Nickname}  刷了一个礼物 {giftData.gift}  积分增加了{scoreNum}，这个礼物已经赠送了{item.mySQLPlayerData.giftDic[giftData.gift]}个了");
                //是否解锁武将
                foreach (var i in item.mySQLPlayerData.giftDic) CreateGeneral(i.Key, i.Value, item, giftData);
            }
    }


    /// <summary>
    ///     添加士兵
    /// </summary>
    /// <param name="unitType"></param>
    /// <param name="campType"></param>
    /// <param name="count"></param>
    public void AddSoldiers(GiftConvertSoldiers soldiers)
    {
        if (soldiers.campType == CampType.Red)
        {
            roomData.RedUnitDic[soldiers.unitType] += soldiers.soldiersCount;
            roomData.RedUnitNum += soldiers.soldiersCount;
        }
        else
        {
            roomData.BlueUnitDic[soldiers.unitType] += soldiers.soldiersCount;
            roomData.BlueUnitNum += soldiers.soldiersCount;
        }

        PELog.ColorLog(LogColor.Yellow, $"{soldiers.campType}方 添加了{soldiers.unitType} 类型的兵种 存储了{soldiers.soldiersCount}个 ");
    }

    /// <summary>
    ///     移除士兵
    /// </summary>
    /// <param name="unitType"></param>
    /// <param name="campType"></param>
    /// <param name="count"></param>
    public void RemoveSoldiers(UnitType unitType, CampType campType, int count)
    {
        var unitDic = campType == CampType.Red ? roomData.RedUnitDic : roomData.BlueUnitDic;
        if (unitDic.ContainsKey(unitType))
            unitDic[unitType] -= count;
        PELog.ColorLog(LogColor.Cyan, $"{roomData.RoomID}房间移除了{campType}阵营的{unitType}士兵");

        var msg = new S2CUpdateSoldierDeath
        {
            s2CMsgID = S2CMsgID.UpdateSoldierDeath,
            campType = campType,
            unitType = unitType,
            count = count
        };
        RoomSys.Instance.SendDoubleMsg(msg, roomData.Red.guid, roomData.Blue.guid, $"向{roomData.RoomID}房间 发送了移除士兵的数据");
    }


    /// <summary>
    ///     减少主城血量
    /// </summary>
    /// <param name="cityHpData"></param>
    public void DecreaseCityHP(AccordCityHPData cityHpData)
    {
        if (cityHpData.campType == CampType.Red)
            ChangeCityHP(roomData.RedCity, cityHpData);
        else
            ChangeCityHP(roomData.BlueCity, cityHpData);
    }

    /// <summary>
    ///     主城血量更新方法
    /// </summary>
    /// <param name="city"></param>
    /// <param name="cityHpData"></param>
    public void ChangeCityHP(CityBase city, AccordCityHPData cityHpData)
    {
        city.HP += cityHpData.attackHp;
        UpdateCity();
        PELog.ColorLog(LogColor.Cyan, $"{cityHpData.campType}主城减少了{cityHpData.attackHp}血,还剩{city.HP}血量");
        var msg = new S2CAccordCityHPData
        {
            s2CMsgID = S2CMsgID.AccordCityHPData,
            accordCityHPData = new AccordCityHPData
            {
                campType = cityHpData.campType,
                HP = city.HP,
                attackHp = cityHpData.attackHp
            }
        };
        RoomSys.Instance.SendDoubleMsg(msg, roomData.Red.guid, roomData.Blue.guid, $"发送了减少{city.campType}主城血量的消息");
    }


    /// <summary>
    ///     更新主城阶段
    /// </summary>
    public void UpdateCity()
    {
        // 更新红城状态
        UpdateCityState(roomData.RedCity, roomData.cityMaxHP, roomData.ThresholdValue_One, roomData.ThresholdValue_Tow);
        // 更新蓝城状态
        UpdateCityState(roomData.BlueCity, roomData.cityMaxHP, roomData.ThresholdValue_One, roomData.ThresholdValue_Tow);
    }

    /// <summary>
    ///     提取的主城状态更新逻辑
    /// </summary>
    /// <param name="city"></param>
    /// <param name="maxHP"></param>
    /// <param name="thresholdOne"></param>
    /// <param name="thresholdTwo"></param>
    private void UpdateCityState(CityBase city, double maxHP, double thresholdOne, double thresholdTwo)
    {
        if (city.HP < maxHP * thresholdOne && city.currentState == CityState.start)
            city.HandleStateChange(city.campType, CityState.Middle, roomData.Red.guid,
                roomData.Blue.guid);

        if (city.HP < maxHP * thresholdTwo && city.currentState == CityState.Middle)
            city.HandleStateChange(city.campType, CityState.AtLast, roomData.Red.guid,
                roomData.Blue.guid);
    }


    /// <summary>
    ///     解锁武将
    /// </summary>
    public void CreateGeneral(GiftEnum giftType, int num, PlayerData items, GiftData data)
    {
        switch (giftType)
        {
            case GiftEnum.仙女棒:
                if (num == 5) GeneralFunc(items, "吕布", GeneralType.吕布, data);

                break;
            case GiftEnum.魔法镜:
                if (num == 5) GeneralFunc(items, "后羿", GeneralType.后羿, data);

                break;
            case GiftEnum.甜甜圈:
                if (num == 5) GeneralFunc(items, "关羽", GeneralType.关羽, data);

                break;
            case GiftEnum.能量电池:
                if (num == 5) GeneralFunc(items, "赵云", GeneralType.赵云, data);

                break;
            case GiftEnum.超能喷射:
                if (num == 5) GeneralFunc(items, "蒙恬", GeneralType.蒙恬, data);

                break;
        }
    }

    /// <summary>
    ///     武将方法
    /// </summary>
    /// <param name="player"></param>
    /// <param name="generalName"></param>
    /// <param name="type"></param>
    /// <param name="data"></param>
    public void GeneralFunc(PlayerData player, string generalName, GeneralType type, GiftData data)
    {
        PELog.ColorLog(LogColor.Magenta, $"{player.mySQLPlayerData.Nickname} 创建 {generalName} ");
        var general = GeneralSys.Instance.createGeneral(type, data);
        if (!player.mySQLPlayerData.generalDic.ContainsKey(general.type))
        {
            player.mySQLPlayerData.generalDic.Add(general.type, general);
            var msg = new S2CCallGeneral
            {
                s2CMsgID = S2CMsgID.CallGeneral,
                generalData = general
            };
            RoomSys.Instance.SendDoubleMsg(msg, roomData.Red.guid, roomData.Blue.guid, "解锁武将");
        }
        else
        {
            // player.mySQLPlayerData.generalDic.Add(general.type, general);
            // var msg = new S2CCallGeneral
            // {
            //     s2CMsgID = S2CMsgID.CallGeneral,
            //     generalData = general
            // };
            // RoomSys.Instance.SendDoubleMsg(msg, roomData.Red.guid, roomData.Blue.guid, "已拥有武将，基类一颗将魂，回满生命值");
            PELog.ColorLog(LogColor.Blue, $"已拥有武将，基类一颗将魂，回满生命值");
        }
    }

    /// <summary>
    ///     切换武将
    /// </summary>
    /// <param name="type"></param>
    /// <param name="uid"></param>
    public void SwitchGeneral(C2SSwitchGeneral data)
    {
        foreach (var items in roomData.PlayerList)
        {
            if (items.mySQLPlayerData.Uid == int.Parse(data.uid))
            {
                if (items.mySQLPlayerData.generalDic.TryGetValue(data.type, out var general))
                {
                    var msg = new S2CSwitchGeneral
                    {
                        s2CMsgID = S2CMsgID.SwitchGeneral,
                        data = new General
                        {
                            state = general.state,
                            reviveTime = general.reviveTime,
                            type = general.type,
                            level = general.level,
                            exp = general.exp,
                            HP = general.HP,
                            attack = general.attack,
                            skillLv = general.skillLv,
                            skill = general.skill,

                            playerIndex = int.Parse(data.uid),
                        }
                    };
                    //发送消息
                    RoomSys.Instance.SendDoubleMsg(msg, roomData.Red.guid, roomData.Blue.guid, $"成功切换武将: {general}");
                }
                else
                {
                    var s2cMsg = new S2CMsg
                    {
                        s2CMsgID = S2CMsgID.OnError,
                        str = $"没有解锁该类型的武将: {data.type}",
                    };
                    RoomSys.Instance.SendDoubleMsg(s2cMsg, roomData.Red.guid, roomData.Blue.guid, "没有解锁该类型的武将");
                }
            }
        }
    }


    /// <summary>
    ///     礼物转化为士兵
    /// </summary>
    /// <param name="giftData"></param>
    public void GiftConversionSolider(GiftData giftData)
    {
        var soldiers = ConvertSoldiers(giftData);
        if (giftData.campType == CampType.Red)
        {
            AddSoldiers(soldiers);
            AddScorePool(soldiers.scoreNum);
            UpdatePlayerData(giftData.campType, giftData.uid, giftData, soldiers.scoreNum);
            PELog.ColorLog(LogColor.Blue,
                $"{soldiers.campType}添加了{soldiers.unitType}，添加了{soldiers.soldiersCount}个，积分增加了{soldiers.scoreNum}");
        }
        else if (giftData.campType == CampType.Blue)
        {
            AddSoldiers(soldiers);
            AddScorePool(soldiers.scoreNum);
            UpdatePlayerData(giftData.campType, giftData.uid, giftData, soldiers.scoreNum);
            PELog.ColorLog(LogColor.Blue,
                $"{soldiers.campType}添加了{soldiers.unitType}，添加了{soldiers.soldiersCount}个，积分增加了{soldiers.scoreNum}");
        }

        PELog.ColorLog(LogColor.Yellow,
            $"目前红方有{roomData.RedUnitNum}个，蓝方有{roomData.BlueUnitNum}个，本局积分{roomData.ScorePool}");

        PushData(soldiers);
    }


    /// <summary>
    ///     礼物转换为兵力 积分
    /// </summary>
    /// <param name="giftData"></param>
    /// <returns></returns>
    public GiftConvertSoldiers ConvertSoldiers(GiftData giftData)
    {
        var soldiers = new GiftConvertSoldiers();
        switch (giftData.gift)
        {
            case GiftEnum.点赞:
                SetSoldierProperties(soldiers, UnitType.Unit_dzb_1, 5 * giftData.count, 10 * giftData.giftvalue,
                    giftData);
                PELog.ColorLog(LogColor.Blue, "点赞");
                break;
            case GiftEnum.仙女棒:
                SetSoldierProperties(soldiers, UnitType.Unit_zs_2, 5 * giftData.count, 10 * giftData.giftvalue,
                    giftData);
                PELog.ColorLog(LogColor.Blue, "仙女棒");
                break;
            case GiftEnum.能量药丸:
                SetSoldierProperties(soldiers, UnitType.Unit_gjs_3, 5 * giftData.count, 10 * giftData.giftvalue,
                    giftData);
                PELog.ColorLog(LogColor.Blue, "能量药丸");
                break;
            case GiftEnum.魔法镜:
                SetSoldierProperties(soldiers, UnitType.Unit_ck_4, 5 * giftData.count, 10 * giftData.giftvalue,
                    giftData);
                PELog.ColorLog(LogColor.Blue, "魔法镜");
                break;
            case GiftEnum.甜甜圈:
                SetSoldierProperties(soldiers, UnitType.Unit_qb_5, 5 * giftData.count, 10 * giftData.giftvalue,
                    giftData);
                PELog.ColorLog(LogColor.Blue, "甜甜圈");
                break;
            case GiftEnum.能量电池:
                SetSoldierProperties(soldiers, UnitType.Unit_pao_6, 5 * giftData.count, 10 * giftData.giftvalue,
                    giftData);
                PELog.ColorLog(LogColor.Blue, "能量电池");
                break;
            case GiftEnum.恶魔炸弹:
                SetSoldierProperties(soldiers, UnitType.Unit_Dun_7, 3 * giftData.count, 10 * giftData.giftvalue,
                    giftData);
                PELog.ColorLog(LogColor.Blue, "恶魔炸弹");
                break;
            case GiftEnum.神秘空投:
                SetSoldierProperties(soldiers, UnitType.Unit_xiuluo_9, 2 * giftData.count, 10 * giftData.giftvalue,
                    giftData);
                PELog.ColorLog(LogColor.Blue, "神秘空投");
                break;
            case GiftEnum.超能喷射:
                SetSoldierProperties(soldiers, UnitType.Unit_Hero, 1 * giftData.count, 10 * giftData.giftvalue,
                    giftData);
                PELog.ColorLog(LogColor.Blue, "超能喷射");
                break;
        }

        return soldiers;
    }


    /// <summary>
    ///     设置士兵的属性
    /// </summary>
    /// <param name="soldiers"></param>
    /// <param name="giftInfo"></param>
    /// <param name="giftData"></param>
    private void SetSoldierProperties(GiftConvertSoldiers soldiers, UnitType unitType, int soldiersCount,
        int scoreNum, GiftData giftData)
    {
        //任意礼物后，本局将永久获得自动点赞
        if (giftData.gift != GiftEnum.点赞)
            foreach (var item in roomData.PlayerList)
                if (item.mySQLPlayerData.Uid == int.Parse(giftData.uid))
                {
                    PELog.ColorLog(LogColor.Blue, $"{item.mySQLPlayerData.Nickname} 赠送了礼物  本局自动点赞");
                    item.IsOnLike = true;
                }

        soldiers.unitType = unitType; // 设置单位类型
        soldiers.soldiersCount = soldiersCount; // 固定士兵数量
        soldiers.scoreNum = scoreNum; // 设置积分
        soldiers.campType = giftData.campType; // 设置阵营类型
        soldiers.WhoSoldiers = giftData.nickName; // 设置玩家昵称
        soldiers.avatar = giftData.avatar; // 设置玩家头像
        soldiers.giftvalue = giftData.giftvalue; // 设置礼物值
        soldiers.giftType = giftData.gift; // 设置礼物类型
    }


    /// <summary>
    ///     推送士兵数据
    /// </summary>
    public void PushData(GiftConvertSoldiers data)
    {
        S2CMsg msg = new S2CGiftToSoldier
        {
            s2CMsgID = S2CMsgID.GiftToSoldier,
            soldierData = data
        };
        var json = JsonConvert.SerializeObject(msg);
        var bytes = Encoding.UTF8.GetBytes(json);
        ServerManager.Instance.server.singlecastText(ServerManager.Instance.server.Sessions[roomData.Red.guid], bytes,
            0, bytes.Length);
        ServerManager.Instance.server.singlecastText(ServerManager.Instance.server.Sessions[roomData.Blue.guid], bytes,
            0, bytes.Length);
        PELog.ColorLog(LogColor.Blue, "分发了礼物转换为士兵的消息");
    }


    /// <summary>
    ///     所有人获得装备     返回获得的经验或武将用于展示
    /// </summary>
    public void GetRoodomWeapon()
    {
        var weapon = WeaponSys.Instance.RandomGetWeapon();
        foreach (var item in roomData.PlayerList)
            //拥有武将才能获得装备
            if (item.mySQLPlayerData.generalDic.Count > 0)
            {
                //拥有武器  转为经验值
                if (item.mySQLPlayerData.weaponDic.ContainsKey(weapon.weaponType))
                {
                    //返回获得的经验 提升的等级与品质  ，更新武将的排名
                    WeaponSys.Instance.GetWeaponExp(item, weapon, roomData.ScorePool, roomData.PlayerList.Count);
                }
                else //没有武器  赋予武器
                {
                    item.mySQLPlayerData.weaponDic.Add(weapon.weaponType, weapon);
                }
            }
            else
            {
                //没有武将  
            }
    }

    /// <summary>
    ///     添加积分池
    /// </summary>
    /// <param name="score"></param>
    public void AddScorePool(int score)
    {
        roomData.ScorePool += score;
        PELog.ColorLog(LogColor.Yellow, $"积分池中添加了{score}分");
    }

    /// <summary>
    ///     本局积分排序
    /// </summary>
    public void RankPlayer()
    {
        roomData.PlayerList.Sort((x, y) => y.score.CompareTo(x.score));
        for (var i = 0; i < roomData.PlayerList.Count; i++) roomData.PlayerList[i].rankId = i + 1;

        roomData.PlayerRankList.Clear();
        for (var i = 0; i < roomData.PlayerList.Count; i++)
        {
            var data = new PlayerRankData
            {
                rankId = roomData.PlayerList[i].rankId,
                name = roomData.PlayerList[i].mySQLPlayerData.Nickname,
                score = roomData.PlayerList[i].score
            };
            roomData.PlayerRankList.Add(data);
        }
    }


    /// <summary>
    ///     瓜分积分池
    /// </summary>
    public void CarveUpScore()
    {
        foreach (var item in roomData.PlayerList)
            //可以参与瓜分积分
            if (item.score >= 200)
            {
                PELog.ColorLog(LogColor.Yellow, $"{item.mySQLPlayerData.Nickname} 可参与瓜分");
                //输赢
                if (item.campType == roomData.Winner)
                {
                    if (item.mySQLPlayerData.streak > 0)
                    {
                        PELog.ColorLog(LogColor.Yellow, $"{item.mySQLPlayerData.Nickname} 是赢家，瓜分了{roomData.ScorePool * 0.66 * item.mySQLPlayerData.streak}分");
                        //根据贡献 倍率为66-100    乘连胜
                        item.score += Convert.ToInt32(roomData.ScorePool * 0.66 * item.mySQLPlayerData.streak);
                    }
                }
                else
                {
                    if (item.mySQLPlayerData.streak > 0)
                    {
                        PELog.ColorLog(LogColor.Yellow, $"{item.mySQLPlayerData.Nickname} 是赢家，瓜分了{roomData.ScorePool * 0.34 * item.mySQLPlayerData.streak}分");
                        //根据贡献 倍率为0-34   乘连胜
                        item.score += Convert.ToInt32(roomData.ScorePool * 0.34 * item.mySQLPlayerData.streak);
                    }
                }
            }
    }


    /// <summary>
    ///     连胜 争夺   先判断连胜 再结算积分
    /// </summary>
    public void WinningStreak()
    {
        foreach (var item in roomData.PlayerList)
            if (item.score >= 200)
            {
                if (item.campType == roomData.Winner)
                {
                    item.mySQLPlayerData.streak += 1;
                    PELog.ColorLog(LogColor.Yellow, $"{item.mySQLPlayerData.Nickname} 是获得了连胜  ， 目前连胜场数为{item.mySQLPlayerData.streak}");
                }
                else
                {
                    var streak = Convert.ToInt32(item.mySQLPlayerData.streak * 0.3);
                    roomData.WinLimit += streak;
                    item.mySQLPlayerData.streak = item.mySQLPlayerData.streak - streak;
                    PELog.ColorLog(LogColor.Yellow, $"{item.mySQLPlayerData.Nickname} 阵营对抗失败  ，扣除连胜的30%{streak} ,目前连胜场数为{item.mySQLPlayerData.streak}");
                }
            }

        RankPlayer();
        for (var i = 0; i < 5; i++)
            switch (i)
            {
                case 0:
                    roomData.PlayerList[i].mySQLPlayerData.streak += Convert.ToInt32(roomData.WinLimit * 0.4);
                    PELog.ColorLog(LogColor.Yellow, $"第{i + 1}名瓜分了{Convert.ToInt32(roomData.WinLimit * 0.4)}连胜");
                    break;
                case 1:
                    roomData.PlayerList[i].mySQLPlayerData.streak += Convert.ToInt32(roomData.WinLimit * 0.25);
                    PELog.ColorLog(LogColor.Yellow, $"第{i + 1}名瓜分了{Convert.ToInt32(roomData.WinLimit * 0.25)}连胜");
                    break;
                case 2:
                    roomData.PlayerList[i].mySQLPlayerData.streak += Convert.ToInt32(roomData.WinLimit * 0.20);
                    PELog.ColorLog(LogColor.Yellow, $"第{i + 1}名瓜分了{Convert.ToInt32(roomData.WinLimit * 0.20)}连胜");
                    break;
                case 3:
                    roomData.PlayerList[i].mySQLPlayerData.streak += Convert.ToInt32(roomData.WinLimit * 0.1);
                    PELog.ColorLog(LogColor.Yellow, $"第{i + 1}名瓜分了{Convert.ToInt32(roomData.WinLimit * 0.1)}连胜");
                    break;
                case 4:
                    roomData.PlayerList[i].mySQLPlayerData.streak += Convert.ToInt32(roomData.WinLimit * 0.05);
                    PELog.ColorLog(LogColor.Yellow, $"第{i + 1}名瓜分了{Convert.ToInt32(roomData.WinLimit * 0.05)}连胜");
                    break;
            }
    }

    /// <summary>
    /// 房间结束
    /// </summary>
    public void GameEnd()
    {
        S2CGameEnd msg = new S2CGameEnd()
        {
            s2CMsgID = S2CMsgID.PVPRoomEnd,
            str = "对局结束了,推送一些结算数据",
        };
        RoomSys.Instance.SendDoubleMsg(msg, roomData.Red.guid, roomData.Blue.guid, "对局结束了,推送一些结算数据");
    }
}
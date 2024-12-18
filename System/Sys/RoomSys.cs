using System.Diagnostics;
using RedBlue_Server.Msg;

namespace RedBlue_Server.System;

/// <summary>
///     房间管理系统
/// </summary>
public class RoomSys : SystemRoot<RoomSys>
{
    public Dictionary<string, PVPRoom> pvpRoomDic;
    public List<PVPRoom> pvpRoomList;
    private int roomID;

    public override void Init()
    {
        base.Init();
        pvpRoomList = new List<PVPRoom>();
        pvpRoomDic = new Dictionary<string, PVPRoom>();
    }

    private bool isOn = true;

    public override void Update()
    {
        base.Update();
        for (var i = 0; i < pvpRoomList.Count; i++)
        {
            pvpRoomList[i].UpdateRoom();
            //游戏结束
            if (pvpRoomList[i].roomData.IsOver == true)
            {
                MatchSys.Instance.RecycleInvitationCode(pvpRoomList[i].roomData.RoomID);
                pvpRoomList[i].ClearRoom();
                pvpRoomList.Remove(pvpRoomList[i]);
                Console.WriteLine("游戏结束了---------------");
            }
        }

        if (DateTime.Now.Hour == 0 && isOn == true)
        {
            Console.WriteLine("第二天了,刷新登录数据");
            List<MySQLPlayerData> datas = DataSys.Instance.GetAllPlayerData(GameData.allPlayerData);
            foreach (var data in datas)
            {
                data.IsSignIn = false;
            }

            DataSys.Instance.UpdateAllPlayerData(GameData.allPlayerData, datas);
            isOn = false;
        }

        if (DateTime.Now.Hour == 2 && isOn == false)
        {
            isOn = true;
        }
    }


    /// <summary>
    ///     创建房间 ,房间开始
    /// </summary>
    /// <param name="num"></param>
    /// <param name="msg"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public void CreateRoom(C2SCreateRoom msg)
    {
        if (pvpRoomDic.ContainsKey(msg.roomId))
        {
            return;
        }

        var roomId = msg.roomId;
        var room = new PVPRoom(roomId, msg.gameModel);
        foreach (var item in MatchSys.Instance.zbDataDic[roomId])
            if (item.guid == msg.redId)
            {
                item.campType = CampType.Red;
                room.roomData.Red = item;
            }
            else if (item.guid == msg.blueId)
            {
                item.campType = CampType.Blue;
                room.roomData.Blue = item;
            }

        pvpRoomList.Add(room);
        pvpRoomDic.Add(roomId, room);

        pvpRoomDic[roomId].StartRoom();
        var s2c = new S2CRoomStart
        {
            s2CMsgID = S2CMsgID.RoomStart,
            isSuccess = true
        };
        SendDoubleMsg(s2c, pvpRoomDic[roomId].roomData.Red.guid, pvpRoomDic[roomId].roomData.Blue.guid,
            $"{roomId}房间已开始等待战斗");
    }


    #region 消息处理

    /// <summary>
    ///     找到房间添加玩家
    /// </summary>
    /// <param name="data"></param>
    public void FindRoomAddPlayer(C2SAddPlayer data)
    {
        pvpRoomDic[data.RoomId].AddPlayer(data.addPlayer);
    }

    /// <summary>
    ///     找到房间添加礼物
    /// </summary>
    /// <param name="data"></param>
    public void FindRoomAddGift(C2SGiveGifts data)
    {
        pvpRoomDic[data.RoomId].GiftConversionSolider(data.giftData);
    }

    /// <summary>
    ///     玩家切换武将
    /// </summary>
    /// <param name="data"></param>
    public void FindRoomSwitchGeneral(C2SSwitchGeneral data)
    {
        pvpRoomDic[data.RoomId].SwitchGeneral(data);
    }

    /// <summary>
    ///     同步主城减血
    /// </summary>
    /// <param name="data"></param>
    public void FindRoomCityHP(C2SAccordCityHPData data)
    {
        pvpRoomDic[data.RoomId].DecreaseCityHP(data.accordCityHPData);
    }

    /// <summary>
    ///     更新士兵数量
    /// </summary>
    /// <param name="data"></param>
    public void FindRoomUpdateSolider(C2SUpdateSoldierDeath data)
    {
        pvpRoomDic[data.RoomId].RemoveSoldiers(data.unitType, data.campType, data.count);
    }

    #endregion
}
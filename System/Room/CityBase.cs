using PEUtils;
using RedBlue_Server.Msg;
using RedBlue_Server.System;

namespace RedBlue_Server;

/// <summary>
///     主城
/// </summary>
public class CityBase
{
    public CampType campType;
    public CityState currentState;
    public float HP;
    public bool isWin;

    /// <summary>
    ///     改变主城状态
    /// </summary>
    /// <param name="newState"></param>
    public void HandleStateChange(CampType type, CityState newState, Guid red, Guid blue)
    {
        S2CCityHPStage msg;
        switch (newState)
        {
            case CityState.start:
                //开始阶段
                currentState = CityState.start;
                break;
            case CityState.Middle:
                // 中段 血量降至2/3大范围爆炸
                currentState = CityState.Middle;
                PELog.ColorLog(LogColor.Green, "进行到中段");
                msg = new S2CCityHPStage
                {
                    campType = type,
                    s2CMsgID = S2CMsgID.CityHPStage,
                    state = CityState.Middle,
                    CallBackBuffer = false
                };
                RoomSys.Instance.SendDoubleMsg(msg, red, blue, $"{type}主城进行到中段");
                break;
            case CityState.AtLast:
                // 最终段 血量降至1/3，触发反击Buff（所有单位提升50%攻击和血量，持续60秒），时间结束但双方未攻陷，则血量少的一方失败，若相同则积分少的一方失败。
                currentState = CityState.AtLast;
                PELog.ColorLog(LogColor.Green, "进行到最终段");
                msg = new S2CCityHPStage
                {
                    campType = type,
                    s2CMsgID = S2CMsgID.CityHPStage,
                    state = CityState.AtLast,
                    CallBackBuffer = true
                };
                RoomSys.Instance.SendDoubleMsg(msg, red, blue, $"{type}主城进行到最终段");
                break;
        }
    }
}

public class RedCity : CityBase
{
    public RedCity()
    {
        campType = CampType.Red;
    }
}

public class BlueCity : CityBase
{
    public BlueCity()
    {
        campType = CampType.Blue;
    }
}

public enum CityState
{
    start,
    Middle,
    AtLast
}
using PEUtils;
using RedBlue_Server.Msg;

namespace RedBlue_Server.System;

/// <summary>
///     武将系统
/// </summary>
public class GeneralSys : SystemRoot<GeneralSys>
{
    public override void Init()
    {
        base.Init();
    }

    public override void Update()
    {
        base.Update();
    }


    /// <summary>
    ///     获得武将
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public General createGeneral(GeneralType type, GiftData data)
    {
        var general = new General
        {
            type = type,
            level = GeneralLv.小兵,
            exp = 0,
            HP = 10000,
            attack = 100,
            skillLv = SkillLv.LV1,
            skill = true,

            playerIndex = int.Parse(data.uid)
        };
        //如果已拥有，就获得一颗将魂 并恢复满生命值
        return general;
    }


    /// <summary>
    ///     升级武将
    /// </summary>
    /// <param name="general"></param>
    /// <returns></returns>
    public General LevUpGeneral(General general)
    {
        switch (general.level)
        {
            case GeneralLv.小兵:
                if (general.exp == 1)
                {
                    general.level = GeneralLv.百夫长;
                    general.HP *= 1.15f;
                    general.attack *= 1.15f;
                }

                return general;
                break;
            case GeneralLv.百夫长:
                if (general.exp == 2)
                {
                    general.level = GeneralLv.校尉;
                    general.HP *= 1.25f;
                    general.attack *= 1.25f;
                }

                return general;
                break;
            case GeneralLv.校尉:
                if (general.exp == 3)
                {
                    general.level = GeneralLv.中郎将;
                    general.skillLv = SkillLv.LV2;
                    general.HP *= 1.5f;
                    general.attack *= 1.5f;
                }

                return general;
                break;
            case GeneralLv.中郎将:
                if (general.exp == 5)
                {
                    general.level = GeneralLv.骠骑将军;
                    general.skillLv = SkillLv.LV3;
                    general.HP *= 1.8f;
                    general.attack *= 1.8f;
                }

                return general;
                break;
            case GeneralLv.骠骑将军:
                if (general.exp == 10)
                {
                    general.level = GeneralLv.威武大将军;
                    general.skillLv = SkillLv.LV4;
                    general.HP *= 2.5f;
                    general.attack *= 2.5f;
                }

                return general;
                break;
            default:
                return null;
                break;
        }
    }


    public float SetGeneralTime(GeneralLv lv)
    {
        switch (lv)
        {
            case GeneralLv.小兵:
                return 60f;
                break;
            case GeneralLv.校尉:
                return 50f;
                break;
            case GeneralLv.中郎将:
                return 40f;
                break;
            case GeneralLv.骠骑将军:
                return 30f;
                break;
            case GeneralLv.威武大将军:
                return 10f;
                break;
            default:
                return 0;
                break;
        }
    }


    /// <summary>
    ///     计算武将战斗力 ,并返回一个List
    /// </summary>
    /// <param name="general"></param>
    public List<GeneralRankData> CalcuteGeneralScore(List<PlayerData> playerData)
    {
        var GeneraRankList = new List<GeneralRankData>();
        foreach (var item in playerData)
        {
            if (item.mySQLPlayerData.generalDic.Count==0)
            {
                PELog.ColorLog(LogColor.Yellow, $"抱歉{item.mySQLPlayerData.Nickname}您没有武将，不可参与排行榜");
                continue;
            }

            List<General> generals = new List<General>();
            List<WeaponBase> weaponBases = new List<WeaponBase>();
            foreach (var value in item.mySQLPlayerData.generalDic)
            {
                generals.Add(value.Value);
            }

            foreach (var value in item.mySQLPlayerData.weaponDic)
            {
                weaponBases.Add(value.Value);
            }
            

            GeneraRankList.Add(new GeneralRankData()
            {
                rankIndex = 0,
                playerName = item.mySQLPlayerData.Nickname,
                generals = generals,
                weapons = weaponBases,
                combatPower = Calcute(generals, weaponBases), //计算战斗力
            });
        }
        GeneraRankList.Sort((x, y) => y.combatPower.CompareTo(x.combatPower));
        for (var i = 0; i < GeneraRankList.Count; i++) GeneraRankList[i].rankIndex = i + 1;
        
        return GeneraRankList;
    }


    public int Calcute(List<General> generals, List<WeaponBase> weaponBases)
    {
        int num = 0;
        foreach (var general in generals)
        {
            num += (int)general.level*10 + (int)general.exp;
        }
        foreach (var weapon in weaponBases)
        {
            num += (int)weapon.level*10 + (int)weapon.exp + (int)weapon.weaponQuality*10;
        }
        return num;
    }
}

/// <summary>
///     武将类型
/// </summary>
public enum GeneralType
{
    吕布,
    后羿,
    关羽,
    赵云,
    蒙恬
}

/// <summary>
///     武将等级
/// </summary>
public enum GeneralLv
{
    小兵,
    百夫长,
    校尉,
    中郎将,
    骠骑将军,
    威武大将军
}

public enum SkillLv
{
    LV1,
    LV2,
    LV3,
    LV4
}

public enum GeneralState
{
    Death,
    Live
}

/// <summary>
///     武将类
/// </summary>
public class General
{
    public float attack;
    public bool campType;
    public int count;
    public int exp; //将魂
    public int heroId;
    public int heroLevel;
    public float HP;
    public GeneralLv level; //等级
    public float offsetZ;

    //武将基类
    public int playerIndex;
    public float reviveTime; //复活时间
    public bool skill;
    public SkillLv skillLv;
    public GeneralState state; //状态
    public GeneralType type; //类型
    public int unitIndex;
    public float unitScore;
}

/// <summary>
///     武将排行数据
/// </summary>
public class GeneralRankData
{
    public int rankIndex;
    public string playerName;
    public int combatPower;
    public List<General> generals;
    public List<WeaponBase> weapons;
}
// /// <summary>
// ///     武将排行数据
// /// </summary>
// public class GeneralRankData
// {
//     public int currentBattleNum;
//     public int GeneralLevel;
//     public string GeneralName;
//     public string playerName;
//     public int rankId;
//     public int WeaponLevel;
// }
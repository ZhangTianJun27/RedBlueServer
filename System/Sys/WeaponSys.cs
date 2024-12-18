using PEUtils;

namespace RedBlue_Server.System;

/// <summary>
///     装备系统
/// </summary>
public class WeaponSys : SystemRoot<WeaponSys>
{
    private Random random;

    public override void Init()
    {
        base.Init();
        random = new Random();
    }

    public override void Update()
    {
        base.Update();
    }


    /// <summary>
    ///     对局结束获得随机装备
    /// </summary>
    public WeaponBase RandomGetWeapon()
    {
        var randomInt = random.Next(0, 3);
        WeaponBase weapon;
        switch (randomInt)
        {
            case 0:
                weapon = CreateWeapon(WeaponType.Weapon, GainType.SkillDamage, WeaponQuality.Green);
                return weapon;
            case 1:
                weapon = CreateWeapon(WeaponType.Helmet, GainType.BoostAttack, WeaponQuality.Green);
                return weapon;
            case 2:
                weapon = CreateWeapon(WeaponType.Pauldrons, GainType.BoostHP, WeaponQuality.Green);
                return weapon;
        }

        return null;
    }

    /// <summary>
    ///     创建武器
    /// </summary>
    /// <param name="type"></param>
    /// <param name="gainType"></param>
    /// <param name="quality"></param>
    /// <returns></returns>
    public WeaponBase CreateWeapon(WeaponType type, GainType gainType, WeaponQuality quality)
    {
        //如果玩家没有就创建一个
        var weapon = new WeaponBase
        {
            weaponType = type,
            gainType = gainType,
            weaponQuality = quality,
            exp = 0,
            level = 1
        };
        return weapon;
    }


    /// <summary>
    ///     获取武器经验  并自动升级     返回经验
    /// </summary>
    /// <param name="data"></param>
    /// <param name="weaponexp"></param>
    public void GetWeaponExp(PlayerData data, WeaponBase weaponexp, int scorePool, int playerCount)
    {
        var weapon = data.mySQLPlayerData.weaponDic[weaponexp.weaponType];
        if (data.rankId == 1)
        {
            PELog.ColorLog(LogColor.Magenta, $"{data.mySQLPlayerData.Nickname}是第一名，武器经验加5倍");
            weaponUp(weapon, 5, scorePool, playerCount,data.mySQLPlayerData.Nickname);
        }
        else if (data.rankId == 2)
        {
            PELog.ColorLog(LogColor.Magenta, $"{data.mySQLPlayerData.Nickname}是第二名，武器经验加3倍");
            weaponUp(weapon, 3, scorePool, playerCount,data.mySQLPlayerData.Nickname);
        }
        else if (data.rankId == 3)
        {
            PELog.ColorLog(LogColor.Magenta, $"{data.mySQLPlayerData.Nickname}是第三名，武器经验加2倍");
            weaponUp(weapon, 2, scorePool, playerCount,data.mySQLPlayerData.Nickname);
        }
        else
        {
            PELog.ColorLog(LogColor.Magenta, $"{data.mySQLPlayerData.Nickname}没有在榜，武器经验不加倍");
            weaponUp(weapon, 1, scorePool, playerCount,data.mySQLPlayerData.Nickname);
        }
    }

    /// <summary>
    ///     升级
    /// </summary>
    /// <param name="weapon"></param>
    /// <param name="exp"></param>
    public void weaponUp(WeaponBase weapon, int exp, int scorePool, int playerCount,string name)
    {
        weapon.exp += Convert.ToInt32(scorePool / playerCount * exp);
        PELog.ColorLog(LogColor.Magenta, $"玩家--{name}--本次共获得经验--{Convert.ToInt32(scorePool / playerCount * exp)}--，目前该武器--{weapon.weaponType}--的经验为--{ weapon.exp }--");
        //升级  直到经验耗尽
        while (true)
        {
            if (weapon.exp >= 1000)
            {
                WeaponUpLevel(weapon);
                PELog.ColorLog(LogColor.Magenta, $"玩家--{name}--武器--{weapon.weaponType}--升级成功，当前等级为--{weapon.level}--还剩{weapon.exp}经验");
            }
            else
            {
                PELog.ColorLog(LogColor.Magenta, $"玩家--{name}--武器--{weapon.weaponType}--经验耗尽，当前等级为--{weapon.level}--还剩{weapon.exp}经验");
                return;
            }
        }
        
    }
    /// <summary>
    ///     武器升级
    /// </summary>
    public void WeaponUpLevel(WeaponBase weaponBase)
    {
        weaponBase.exp -= 1000;
        weaponBase.level += 1;
        if (weaponBase.level == 10)
            switch (weaponBase.weaponQuality)
            {
                case WeaponQuality.Green:
                    weaponBase.weaponQuality = WeaponQuality.Blue;
                    break;
                case WeaponQuality.Blue:
                    weaponBase.weaponQuality = WeaponQuality.Purple;
                    break;
                case WeaponQuality.Purple:
                    weaponBase.weaponQuality = WeaponQuality.Orange;
                    break;
            }
    }
    
    
    
    
}

/// <summary>
///     武器类
/// </summary>
public class WeaponBase
{
    public int exp;
    public GainType gainType;
    public int level;
    public WeaponQuality weaponQuality;
    public WeaponType weaponType;
}

/// <summary>
///     武器类型
/// </summary>
public enum WeaponType
{
    Weapon, //武器
    Helmet, //头盔
    Pauldrons //肩甲
}

/// <summary>
///     武器增益
/// </summary>
public enum GainType
{
    SkillDamage, //技能增伤
    BoostAttack, //增加攻击力
    BoostHP //增加血量
}

/// <summary>
///     武器品质 十级提升一次品质
/// </summary>
public enum WeaponQuality
{
    Green,
    Blue,
    Purple,
    Orange
}
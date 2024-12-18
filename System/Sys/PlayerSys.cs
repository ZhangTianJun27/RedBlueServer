using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PEUtils;
using RedBlue_Server.Msg;

namespace RedBlue_Server.System;

public class PlayerSys : SystemRoot<PlayerSys>
{
    public Dictionary<string, PlayerData> playerDic = new();

    /// <summary>
    ///     所有玩家列表
    /// </summary>
    public List<PlayerData> playerList;


    public string tableName = "AllPlayerTable";

    public override void Init()
    {
        base.Init();
    }

    public override void Update()
    {
        base.Update();
    }

    // /// <summary>
    // ///     创建表
    // /// </summary>
    // /// <param name="tableName"></param>
    // public void CreatePlayerDataTable(string tableName)
    // {
    //     try
    //     {
    //         using (var connection = new MySqlConnection(GameData.linkMySQLPath))
    //         {
    //             connection.Open();
    //
    //             var createTableQuery = $@"CREATE TABLE IF NOT EXISTS {tableName} (
    //                 AvatarUrl VARCHAR(255),                
    //                 Nickname VARCHAR(100) NOT NULL,       
    //                 Uid VARCHAR(100) NOT NULL PRIMARY KEY, 
    //                 weekRank INT,                          
    //                 weekScore INT,                         
    //                 streak INT,                          
    //                 giftDic JSON,                          
    //                 WeaponDic JSON,                        
    //                 generalDic JSON                         
    //             );";
    //
    //             using (var command = new MySqlCommand(createTableQuery, connection))
    //             {
    //                 command.ExecuteNonQuery(); // 执行创建表的 SQL 语句
    //                 PELog.ColorLog(LogColor.Green, $"表 {tableName} 创建成功或已存在。"); // 记录表创建状态
    //             }
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         PELog.ColorLog(LogColor.Red, $"创建玩家数据表失败: {ex.Message}"); // 记录具体错误信息
    //     }
    // }


    /// <summary>
    ///     在所有玩家数据中查询单个玩家
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="tableName"></param>
    public MySQLPlayerData QueryPlayer(string tableName, string uid)
    {
        try
        {
            using (var connection = new MySqlConnection(GameData.linkMySQLPath))
            {
                connection.Open();

                var selectSQL = $"SELECT * FROM {tableName} WHERE Uid = @Uid";
                using (var command = new MySqlCommand(selectSQL, connection))
                {
                    command.Parameters.AddWithValue("@Uid", uid); // 添加参数化查询以防止SQL注入
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // 创建玩家数据对象并填充数据
                            var mySqlPlayerData = new MySQLPlayerData
                            {
                                Uid = Convert.ToInt32(reader["Uid"]),
                                Nickname = reader["Nickname"].ToString(),
                                AvatarUrl = reader["AvatarUrl"].ToString(),
                                monthRank = Convert.ToInt32(reader["monthRank"]),
                                weekRank = Convert.ToInt32(reader["weekRank"]),
                                IsSignIn = Convert.ToInt32(reader["IsSignIn"]) != 0,
                                SignInDays = Convert.ToInt32(reader["SignInDays"]),
                                streak = Convert.ToInt32(reader["streak"]),
                                giftDic = JsonConvert.DeserializeObject<Dictionary<GiftEnum, int>>(reader["giftDic"].ToString()),
                                weaponDic = JsonConvert.DeserializeObject<Dictionary<WeaponType, WeaponBase>>(reader["weaponDic"].ToString()),
                                generalDic = JsonConvert.DeserializeObject<Dictionary<GeneralType, General>>(reader["generalDic"].ToString())
                            };
                            PELog.ColorLog(LogColor.Green, $"成功查询玩家信息: {mySqlPlayerData.Nickname}");
                            return mySqlPlayerData;
                        }
                    }
                }
            }
        }
        catch
        {
            PELog.ColorLog(LogColor.Red, $"查询玩家{uid}失败");
        }

        return new MySQLPlayerData();
    }


    /// <summary>
    ///     写入单个数据
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="data"></param>
    private void EveryInningRank(string tableName, MySQLPlayerData data)
    {
        try
        {
            // 使用 MySqlConnection 创建数据库连接
            using (var connection = new MySqlConnection(GameData.linkMySQLPath))
            {
                connection.Open(); // 打开数据库连接

                // 开始数据库事务
                using (var transaction = connection.BeginTransaction())
                {
                    var insertQuery =
                        $"INSERT INTO {tableName} (AvatarUrl, Nickname, Uid,weekRank,weekScore,streak,giftDic,WeaponDic,generalDic) VALUES (@AvatarUrl, @Nickname, @Uid,@weekRank,@weekScore,@streak,@giftDic,@WeaponDic,@generalDic)";
                    using (var command = new MySqlCommand(insertQuery, connection, transaction)) // 在事务中执行插入
                    {
                        command.Parameters.Clear(); // 清除之前的参数

                        command.Parameters.AddWithValue("@AvatarUrl", data.AvatarUrl);
                        command.Parameters.AddWithValue("@Nickname", data.Nickname);
                        command.Parameters.AddWithValue("@Uid", data.Uid);
                        command.Parameters.AddWithValue("@weekRank", data.monthRank);
                        command.Parameters.AddWithValue("@weekScore", data.weekRank);
                        command.Parameters.AddWithValue("@IsSignIn", data.IsSignIn);
                        command.Parameters.AddWithValue("@SignInDays", data.SignInDays);
                        command.Parameters.AddWithValue("@streak", data.streak);
                        command.Parameters.AddWithValue("@giftDic",
                            JsonConvert.SerializeObject(data.giftDic));
                        command.Parameters.AddWithValue("@generalDic",
                            JsonConvert.SerializeObject(data.generalDic));
                        command.Parameters.AddWithValue("@WeaponDic",
                            JsonConvert.SerializeObject(data.weaponDic));


                        command.ExecuteNonQuery(); // 执行插入操作
                        Console.WriteLine($"表 {tableName} 数据写入完成");
                    }

                    transaction.Commit(); // 提交事务
                }
            }

            GetEveryInningRank(tableName);
        }
        catch (Exception ex) // 捕获异常
        {
            // 打印异常信息，方便调试
            PELog.ColorLog(LogColor.Red, $"连接数据库失败: {ex.Message}");
        }
    }

    /// <summary>
    ///     查询每局排行
    /// </summary>
    private void GetEveryInningRank(string tableName)
    {
        using (var connection = new MySqlConnection(GameData.linkMySQLPath))
        {
            connection.Open();

            var selectQuery = $"SELECT * FROM {tableName} ORDER BY Score DESC";
            var rankings = new List<MySQLPlayerData>();
            using (var command = new MySqlCommand(selectQuery, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    rankings.Add(new MySQLPlayerData
                    {
                        //待补充
                        AvatarUrl = reader.GetString("AvatarUrl"),
                        Nickname = reader.GetString("Nickname"),
                        Uid = reader.GetInt32("Uid"),
                        monthRank = reader.GetInt32("monthRank"),
                        weekRank = reader.GetInt32("weekRank"),
                        IsSignIn = Convert.ToInt32(reader["IsSignIn"]) != 0,
                        SignInDays = reader.GetInt32("SignInDays"),
                        streak = reader.GetInt32("Streak"),
                        giftDic = JsonConvert.DeserializeObject<Dictionary<GiftEnum, int>>(reader.GetString("GiftDic")),
                        weaponDic = JsonConvert.DeserializeObject<Dictionary<WeaponType, WeaponBase>>(reader.GetString("WeaponDic")),
                        generalDic = JsonConvert.DeserializeObject<Dictionary<GeneralType, General>>(reader.GetString("GeneralDic")),
                    });
            }

            foreach (var ranking in rankings)
                Console.WriteLine($"Uid: {ranking.Uid}, 玩家名字: {ranking.Nickname}");
        }
    }


    /// <summary>
    /// 每日登录
    /// </summary>
    public void EveryDayLogin(PlayerData data)
    {
        data.mySQLPlayerData.IsSignIn = true;
        if (data.mySQLPlayerData.SignInDays == 7)
        {
            data.mySQLPlayerData.SignInDays = 0;
        }
        data.mySQLPlayerData.SignInDays += 1;
        WeaponBase weaponBase = GiveLowWeapon(data.mySQLPlayerData.SignInDays);
        if (!data.mySQLPlayerData.weaponDic.ContainsKey(weaponBase.weaponType))
        {
            //装备
            data.mySQLPlayerData.weaponDic.Add(weaponBase.weaponType, weaponBase);
        }
        else
        {
            //经验
        }

        EveryInningRank(GameData.allPlayerData, data.mySQLPlayerData);
    }

    /// <summary>
    /// 根据连签获得随机品质的装备
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public WeaponBase GiveLowWeapon(int num)
    {
        Random random = new Random();
        int index = random.Next(0, 3);
        WeaponBase weaponBase = new WeaponBase()
        {
            exp = 0,
            gainType = (GainType)random.Next(0, 3),
            level = 1,
            weaponQuality = RandomQuality(index),
            weaponType = (WeaponType)random.Next(0, 3),
        };
        return weaponBase;
    }

    public WeaponQuality RandomQuality(int num)
    {
        Random random = new Random();
        int index = random.Next(0, num);
        if (index >= 6)
        {
            return WeaponQuality.Blue;
        }
        else
        {
            return WeaponQuality.Green;
        }
    }
}

public class PlayerData
{
    /// <summary>
    ///     玩家阵营
    /// </summary>
    public CampType campType;

    //玩家本局是否自动点赞
    public bool IsOnLike;

    /// <summary>
    ///     自动点赞间隔
    /// </summary>
    /// <returns></returns>
    public float LikeInterval;

    /// <summary>
    ///     玩家长期数据
    /// </summary>
    public MySQLPlayerData mySQLPlayerData;

    /// <summary>
    ///     本局排行
    /// </summary>
    public int rankId;

    /// <summary>
    ///     本局分数
    /// </summary>
    public int score;
}

/// <summary>
///     玩家数据
/// </summary>
public class MySQLPlayerData
{
    /// <summary>
    ///     玩家头像路径
    /// </summary>
    public string AvatarUrl;


    /// <summary>
    ///     玩家已拥有武将
    /// </summary>
    public Dictionary<GeneralType, General> generalDic = new();

    /// <summary>
    ///     玩家已赠送礼物
    /// </summary>
    public Dictionary<GiftEnum, int> giftDic = new()
    {
        { GiftEnum.点赞, 0 },
        { GiftEnum.评论666, 0 },
        { GiftEnum.仙女棒, 0 },
        { GiftEnum.恶魔炸弹, 0 },
        { GiftEnum.甜甜圈, 0 },
        { GiftEnum.神秘空投, 0 },
        { GiftEnum.能量电池, 0 },
        { GiftEnum.能量药丸, 0 },
        { GiftEnum.魔法镜, 0 },
        { GiftEnum.超能喷射, 0 }
    };

    //每日签到
    public bool IsSignIn;

    //连续签到天数    如果刷新时  签到为false  连签归零
    public int SignInDays;

    /// <summary>
    ///     月排名
    /// </summary>
    public int monthRank;

    /// <summary>
    ///     玩家名字
    /// </summary>
    public string Nickname;

    /// <summary>
    ///     连胜
    /// </summary>
    public int streak;

    /// <summary>
    ///     玩家ID
    /// </summary>
    public int Uid;


    /// <summary>
    ///     玩家已拥有武器
    /// </summary>
    public Dictionary<WeaponType, WeaponBase> weaponDic = new();

    /// <summary>
    ///     周排名
    /// </summary>
    public int weekRank;
}
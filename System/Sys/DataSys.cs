using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PEUtils;
using RedBlue_Server.Msg;

namespace RedBlue_Server.System;

/// <summary>
///     数据系统
/// </summary>
public class DataSys : SystemRoot<DataSys>
{
    private string ConnectPath;

    public override void Init()
    {
        base.Init();
        ConnectPath = GameData.linkMySQLPath;
    }

    public override void Update()
    {
        base.Update();
    }


    /// <summary>
    ///     武将月排行
    /// </summary>
    public void GeneralRank()
    {
    }

    /// <summary>
    ///     每局积分排行
    /// </summary>
    public void ScoreEveryDayRank(List<PlayerRankData> data)
    {
        EveryInningRank("每局排行", data);
    }

    /// <summary>
    ///     积分周排行
    /// </summary>
    public void ScoreWeekRank(List<PlayerRankData> data)
    {
        WeekRank("本周排行", data);
    }

    /// <summary>
    ///     积分月排行
    /// </summary>
    public void ScoreMonthRank(List<PlayerRankData> data)
    {
        MonthRank("本月排行", data);
    }


    /// <summary>
    ///     更新每局排行
    /// </summary>
    private void EveryInningRank(string tableName, List<PlayerRankData> data)
    {
        try
        {
            // 使用 MySqlConnection 创建数据库连接
            using (var connection = new MySqlConnection(ConnectPath))
            {
                connection.Open(); // 打开数据库连接

                // 创建表的 SQL 语句
                var createTableQuery =
                    $@"CREATE TABLE IF NOT EXISTS {tableName}(RankId INT AUTO_INCREMENT PRIMARY KEY,PlayerName VARCHAR(100) NOT NULL,Score INT NOT NULL);";
                using (var command = new MySqlCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery(); // 执行创建表的 SQL 语句
                    Console.WriteLine($"表 {tableName} 创建成功或已存在。");
                }

                // 清空表的 SQL 语句
                var deleteQuery = $"DELETE FROM {tableName}"; // 构造清空表的 SQL 语句
                using (var command = new MySqlCommand(deleteQuery, connection))
                {
                    command.ExecuteNonQuery(); // 执行清空表的 SQL 语句
                    Console.WriteLine($"表 {tableName} 已清空");
                }

                // 开始数据库事务
                using (var transaction = connection.BeginTransaction())
                {
                    var insertQuery =
                        $"INSERT INTO {tableName} (RankId, PlayerName, Score) VALUES (@RankId, @PlayerName, @Score)"; // 构造插入 SQL 语句
                    using (var command = new MySqlCommand(insertQuery, connection, transaction)) // 在事务中执行插入
                    {
                        foreach (var player in data)
                        {
                            command.Parameters.Clear(); // 清除之前的参数
                            command.Parameters.AddWithValue("@RankId", player.rankId); // 添加玩家排名 ID
                            command.Parameters.AddWithValue("@PlayerName", player.name); // 添加玩家名称
                            command.Parameters.AddWithValue("@Score", player.score); // 添加玩家得分

                            command.ExecuteNonQuery(); // 执行插入操作
                        }

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
    private List<PlayerRankData> GetEveryInningRank(string tableName)
    {
        using (var connection = new MySqlConnection(ConnectPath))
        {
            connection.Open();
            // 查询排名的 SQL 语句
            var selectQuery = $"SELECT * FROM {tableName} ORDER BY Score DESC"; // 构造查询 SQL 语句
            var rankings = new List<PlayerRankData>(); // 玩家排名列表
            using (var command = new MySqlCommand(selectQuery, connection)) // 创建查询命令
            using (var reader = command.ExecuteReader()) // 执行查询并获取结果
            {
                while (reader.Read()) // 逐行读取结果
                    rankings.Add(new PlayerRankData
                    {
                        rankId = reader.GetInt32(0), // 获取排名 ID
                        name = reader.GetString(1), // 获取玩家名称
                        score = reader.GetInt32(2) // 获取玩家得分
                    });
            }

            foreach (var ranking in rankings) // 输出排名结果
                Console.WriteLine($"本局排行名次: {ranking.rankId}, 玩家名字: {ranking.name}, 分数: {ranking.score}");
            return rankings;
        }
    }


    /// <summary>
    ///     写入周排行
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="data"></param>
    private void WeekRank(string tableName, List<PlayerRankData> data)
    {
        try
        {
            using (var connection = new MySqlConnection(ConnectPath))
            {
                connection.Open();
                var createSQL =
                    $@"CREATE TABLE IF NOT EXISTS {tableName} (RankId INT AUTO_INCREMENT PRIMARY KEY, PlayerName VARCHAR(100) NOT NULL,Score INT NOT NULL ,lastRankId INT NOT NULL );";
                using (var command = new MySqlCommand(createSQL, connection))
                {
                    command.ExecuteNonQuery();
                    PELog.ColorLog(LogColor.Green, $"{tableName}这个表已经存在或已经创建成功");
                }

                var delSQL = $"DELETE FROM {tableName}";
                using (var command = new MySqlCommand(delSQL, connection))
                {
                    command.ExecuteNonQuery();
                    PELog.ColorLog(LogColor.Green, $"{tableName}这个表已经清空");
                }


                // 开始数据库事务
                using (var transaction = connection.BeginTransaction())
                {
                    var insertQuery =
                        $@"INSERT INTO {tableName} (RankId, PlayerName, Score,LastRankId) VALUES (@RankId, @PlayerName, @Score,@LastRankId)"; // 构造插入 SQL 语句
                    using (var command = new MySqlCommand(insertQuery, connection, transaction)) // 在事务中执行插入
                    {
                        foreach (var player in data)
                        {
                            command.Parameters.Clear(); // 清除之前的参数
                            command.Parameters.AddWithValue("@RankId", player.rankId); // 添加玩家排名 ID
                            command.Parameters.AddWithValue("@PlayerName", player.name); // 添加玩家名称
                            command.Parameters.AddWithValue("@Score", player.score); // 添加玩家得分
                            command.Parameters.AddWithValue("@LastRankId", player.rankId); // 添加上周排行
                            command.ExecuteNonQuery(); // 执行插入操作
                        }

                        Console.WriteLine($"表 {tableName} 数据写入完成");
                    }

                    transaction.Commit(); // 提交事务
                }
            }

            GetWeekRankData(tableName);
        }
        catch
        {
            PELog.ColorLog(LogColor.Red, "更新周排行失败");
        }
    }

    /// <summary>
    ///     查询每周排行数据
    /// </summary>
    /// <param name="tableName"></param>
    private List<PlayerRankData> GetWeekRankData(string tableName)
    {
        try
        {
            using (var connection = new MySqlConnection(ConnectPath))
            {
                connection.Open();
                // 查询排名的 SQL 语句
                var selectQuery = $"SELECT * FROM {tableName} ORDER BY Score DESC"; // 构造查询 SQL 语句
                var rankings = new List<PlayerRankData>(); // 玩家排名列表
                using (var command = new MySqlCommand(selectQuery, connection)) // 创建查询命令
                using (var reader = command.ExecuteReader()) // 执行查询并获取结果
                {
                    while (reader.Read()) // 逐行读取结果
                        rankings.Add(new PlayerRankData
                        {
                            rankId = reader.GetInt32(0), // 获取排名 ID
                            name = reader.GetString(1), // 获取玩家名称
                            score = reader.GetInt32(2), // 获取玩家得分
                            lastRankId = reader.GetInt32(3) // 获取上周排名
                        });
                }

                foreach (var ranking in rankings) // 输出排名结果
                    Console.WriteLine(
                        $"本周排行名次: {ranking.rankId}, 玩家名字: {ranking.name}, 分数: {ranking.score},上周排名为{ranking.lastRankId}");
                return rankings;
            }
        }
        catch
        {
            PELog.ColorLog(LogColor.Red, "查询周排行失败");
        }

        return null;
    }


    /// <summary>
    ///     写入月排行
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="data"></param>
    private void MonthRank(string tableName, List<PlayerRankData> data)
    {
        try
        {
            using (var connection = new MySqlConnection(ConnectPath))
            {
                connection.Open();
                var createSQL =
                    $@"CREATE TABLE IF NOT EXISTS {tableName} (RankId INT AUTO_INCREMENT PRIMARY KEY, PlayerName VARCHAR(100) NOT NULL,Score INT NOT NULL ,lastRankId INT NOT NULL );";
                using (var command = new MySqlCommand(createSQL, connection))
                {
                    command.ExecuteNonQuery();
                    PELog.ColorLog(LogColor.Green, $"{tableName}这个表已经存在或已经创建成功");
                }

                var delSQL = $"DELETE FROM {tableName}";
                using (var command = new MySqlCommand(delSQL, connection))
                {
                    command.ExecuteNonQuery();
                    PELog.ColorLog(LogColor.Green, $"{tableName}这个表已经清空");
                }


                // 开始数据库事务
                using (var transaction = connection.BeginTransaction())
                {
                    var insertQuery =
                        $@"INSERT INTO {tableName} (RankId, PlayerName, Score,LastRankId) VALUES (@RankId, @PlayerName, @Score,@LastRankId)"; // 构造插入 SQL 语句
                    using (var command = new MySqlCommand(insertQuery, connection, transaction)) // 在事务中执行插入
                    {
                        foreach (var player in data)
                        {
                            command.Parameters.Clear(); // 清除之前的参数
                            command.Parameters.AddWithValue("@RankId", player.rankId); // 添加玩家排名 ID
                            command.Parameters.AddWithValue("@PlayerName", player.name); // 添加玩家名称
                            command.Parameters.AddWithValue("@Score", player.score); // 添加玩家得分
                            command.Parameters.AddWithValue("@LastRankId", player.rankId); // 添加上周排行
                            command.ExecuteNonQuery(); // 执行插入操作
                        }

                        Console.WriteLine($"表 {tableName} 数据写入完成");
                    }

                    transaction.Commit(); // 提交事务
                }
            }

            GetMonthRankData(tableName);
        }
        catch
        {
            PELog.ColorLog(LogColor.Red, "更新月排行失败");
        }
    }

    /// <summary>
    ///     查询每月排行数据
    /// </summary>
    /// <param name="tableName"></param>
    private List<PlayerRankData> GetMonthRankData(string tableName)
    {
        try
        {
            using (var connection = new MySqlConnection(ConnectPath))
            {
                connection.Open();
                // 查询排名的 SQL 语句
                var selectQuery = $"SELECT * FROM {tableName} ORDER BY Score DESC"; // 构造查询 SQL 语句
                var rankings = new List<PlayerRankData>(); // 玩家排名列表
                using (var command = new MySqlCommand(selectQuery, connection)) // 创建查询命令
                using (var reader = command.ExecuteReader()) // 执行查询并获取结果
                {
                    while (reader.Read()) // 逐行读取结果
                        rankings.Add(new PlayerRankData
                        {
                            rankId = reader.GetInt32(0), // 获取排名 ID
                            name = reader.GetString(1), // 获取玩家名称1
                            score = reader.GetInt32(2), // 获取玩家得分
                            lastRankId = reader.GetInt32(3) // 获取上周排名
                        });
                }

                foreach (var ranking in rankings) // 输出排名结果
                    Console.WriteLine(
                        $"本月排行名次: {ranking.rankId}, 玩家名字: {ranking.name}, 分数: {ranking.score},上周排名为{ranking.lastRankId}");
                return rankings;
            }
        }
        catch
        {
            PELog.ColorLog(LogColor.Red, "查询月排行失败");
        }

        return null;
    }


    /// <summary>
    ///     更新本周排行
    /// </summary>
    /// <param name="playerRankDataList"></param>
    public void UpdateWeekRank(List<PlayerRankData> playerRankDataList)
    {
        var WeekRank = GetWeekRankData("本周排行");
        if (WeekRank == null)
        {
            ScoreWeekRank(playerRankDataList);
            return;
        }

        var dic = new Dictionary<string, PlayerRankData>();
        foreach (var item in WeekRank)
            if (!dic.ContainsKey(item.name))
                dic.Add(item.name, item);
            else
                PELog.ColorLog(LogColor.Green, $"有一个重复的人{item.name}");

        foreach (var item in playerRankDataList)
            if (dic.ContainsKey(item.name))
            {
                dic[item.name].score += item.score;
                PELog.ColorLog(LogColor.Green, $"{item.name}在周排行中添加了 {item.score},现在还有 {dic[item.name].score}");
            }
            else
            {
                PELog.ColorLog(LogColor.Green, $"周排行添加了玩家{item.name}");
                WeekRank.Add(item);
            }

        WeekRank.Sort((x, y) => y.score.CompareTo(x.score));
        for (var i = 0; i < WeekRank.Count; i++) WeekRank[i].rankId = i + 1;

        this.WeekRank("本周排行", WeekRank);
    }

    /// <summary>
    ///     更新本月排行
    /// </summary>
    /// <param name="playerRankDataList"></param>
    public void UpdateMonthRank(List<PlayerRankData> playerRankDataList)
    {
        var WeekRank = GetMonthRankData("本月排行");
        if (WeekRank == null)
        {
            ScoreMonthRank(playerRankDataList);
            return;
        }

        var dic = new Dictionary<string, PlayerRankData>();

        foreach (var item in WeekRank)
            if (!dic.ContainsKey(item.name))
                dic.Add(item.name, item);
            else
                PELog.ColorLog(LogColor.Green, $"有一个重复的人{item.name}");

        foreach (var item in playerRankDataList)
            if (dic.ContainsKey(item.name))
            {
                dic[item.name].score += item.score;
                PELog.ColorLog(LogColor.Green, $"{item.name}在月排行中添加了 {item.score},现在还有 {dic[item.name].score}");
            }
            else
            {
                PELog.ColorLog(LogColor.Green, $"月排行添加了玩家{item.name}");
                WeekRank.Add(item);
            }

        WeekRank.Sort((x, y) => y.score.CompareTo(x.score));
        for (var i = 0; i < WeekRank.Count; i++) WeekRank[i].rankId = i + 1;

        MonthRank("本月排行", WeekRank);
    }


    /// <summary>
    ///     创建所有玩家数据
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="SQL"></param>
    public bool CreateMySQLTable(string tableName)
    {
        try
        {
            var SQL =
                @$"CREATE TABLE IF NOT EXISTS {tableName} 
            (
            Uid INT NOT NULL PRIMARY KEY,
            AvatarUrl TEXT NOT NULL,
            Nickname TEXT NOT NULL,
            monthRank INT NOT NULL,
            weekRank INT NOT NULL,
            IsSignIn INT NOT NULL,
            SignInDays INT NOT NULL,
            streak INT NOT NULL,
            giftDic TEXT NOT NULL,
            weaponDic TEXT NOT NULL,
            generalDic TEXT NOT NULL
            )"; // 定义列的数据类型
            using (var connect = new MySqlConnection(ConnectPath))
            {
                connect.Open();

                using (var command = new MySqlCommand(SQL, connect))
                {
                    command.ExecuteNonQuery();
                    PELog.ColorLog(LogColor.Green, $"--{tableName}--这个表已经存在或已经创建成功");
                }
            }

            return true;
        }
        catch
        {
            PELog.ColorLog(LogColor.Red, $"创建表:--{tableName}--失败");
            return false;
        }
    }


    /// <summary>
    ///     更新所有玩家的数据
    /// </summary>
    public void UpdateAllPlayerData(string tableName, List<MySQLPlayerData> playerRankDataList)
    {
        if (!CreateMySQLTable(tableName)) return;

        var playerData = GetAllPlayerData(tableName);
        if (playerData.Count == 0)
        {
            WritePlayerData(tableName, playerRankDataList);
            PELog.ColorLog(LogColor.Green, $"查询不到表--{tableName}--直接写入");
            return;
        }

        var dic = new Dictionary<int, MySQLPlayerData>();

        //将排行数据添加到字典中
        foreach (var item in playerData)
            if (!dic.TryAdd(item.Uid, item))
                PELog.ColorLog(LogColor.Green, $"字典中添加时有一个重复的人--{item.Nickname}--");

        //更新
        foreach (var item in playerRankDataList)
            if (dic.ContainsKey(item.Uid))
            {
                dic[item.Uid] = item;
                var existingRank = playerData.FirstOrDefault(x => x.Uid == item.Uid);
                if (existingRank != null) existingRank = dic[item.Uid]; // 更新 WeekRank 中的得分

                playerData.Add(existingRank);
                PELog.ColorLog(LogColor.Green, $"--{tableName}--表中--{item.Nickname}--玩家数据更新");
            }
            else
            {
                PELog.ColorLog(LogColor.Green, $"--{tableName}--没有该玩家，添加了玩家--{item.Nickname}--");
                playerData.Add(item);
            }

        //将数据更新到数据库中
        WritePlayerData(tableName, playerData);
    }

    /// <summary>
    ///     写入所有玩家数据
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="playerData"></param>
    public void WritePlayerData(string tableName, List<MySQLPlayerData> playerData)
    {
        using (var connection = new MySqlConnection(ConnectPath))
        {
            connection.Open(); // 打开数据库连接

            var delSQL = $"DELETE FROM {tableName}";
            using (var command = new MySqlCommand(delSQL, connection))
            {
                command.ExecuteNonQuery();
                PELog.ColorLog(LogColor.Green, $"{tableName}这个表已经清空");
            }

            foreach (var player in playerData) // 遍历玩家列表
            {
                var updateSQL =
                    $"INSERT INTO {tableName} (AvatarUrl, Nickname, Uid, monthRank, weekRank,IsSignIn,SignInDays, streak, giftDic, weaponDic, generalDic) " +
                    $"VALUES (@AvatarUrl, @Nickname, @Uid, @monthRank, @weekRank,@IsSignIn,@SignInDays, @streak, @giftDic, @weaponDic, @generalDic) " +
                    $"ON DUPLICATE KEY UPDATE Uid=@Uid,AvatarUrl=@AvatarUrl, monthRank=@monthRank, weekRank=@weekRank, IsSignIn=@IsSignIn,SignInDays=@SignInDays,streak=@streak, giftDic=@giftDic, weaponDic=@weaponDic, generalDic=@generalDic;";

                using (var command = new MySqlCommand(updateSQL, connection)) // 创建 SQL 命令
                {
                    command.Parameters.AddWithValue("@Uid", player.Uid);
                    command.Parameters.AddWithValue("@AvatarUrl", player.AvatarUrl); // 添加参数
                    command.Parameters.AddWithValue("@Nickname", player.Nickname);
                    command.Parameters.AddWithValue("@monthRank", player.monthRank);
                    command.Parameters.AddWithValue("@IsSignIn", (bool)player.IsSignIn);
                    command.Parameters.AddWithValue("@SignInDays", player.SignInDays);
                    command.Parameters.AddWithValue("@weekRank", player.weekRank);
                    command.Parameters.AddWithValue("@streak", player.streak);
                    command.Parameters.AddWithValue("@giftDic", JsonConvert.SerializeObject(player.giftDic));
                    command.Parameters.AddWithValue("@weaponDic", JsonConvert.SerializeObject(player.weaponDic));
                    command.Parameters.AddWithValue("@generalDic", JsonConvert.SerializeObject(player.generalDic));

                    command.ExecuteNonQuery(); // 执行更新
                }
            }
        }
    }


    /// <summary>
    ///     查询所有玩家数据
    /// </summary>
    /// <param name="tableName"></param>
    public List<MySQLPlayerData> GetAllPlayerData(string tableName)
    {
        var rankings = new List<MySQLPlayerData>();
        try
        {
            using (var connection = new MySqlConnection(ConnectPath))
            {
                connection.Open();
                // 查询排名的 SQL 语句
                var selectQuery = $"SELECT * FROM {tableName}"; // 构造查询 SQL 语句
                using (var command = new MySqlCommand(selectQuery, connection)) // 创建查询命令
                using (var reader = command.ExecuteReader()) // 执行查询并获取结果
                {
                    while (reader.Read()) // 逐行读取结果
                        rankings.Add(new MySQLPlayerData
                        {
                            Uid = reader.GetInt32("Uid"),
                            AvatarUrl = reader.GetString("AvatarUrl"),
                            Nickname = reader.GetString("Nickname"),
                            monthRank = reader.GetInt32("monthRank"),
                            weekRank = reader.GetInt32("weekRank"),
                            IsSignIn =  reader.GetInt32("IsSignIn") != 0,
                            SignInDays = reader.GetInt32("SignInDays"),
                            streak = reader.GetInt32("streak"),
                            giftDic = JsonConvert.DeserializeObject<Dictionary<GiftEnum, int>>(reader.GetString("giftDic")),
                            weaponDic = JsonConvert.DeserializeObject<Dictionary<WeaponType, WeaponBase>>(reader.GetString("weaponDic")),
                            generalDic = JsonConvert.DeserializeObject<Dictionary<GeneralType, General>>(reader.GetString("generalDic"))
                        });
                }

                return rankings;
            }
        }
        catch
        {
            PELog.ColorLog(LogColor.Red, $"查询--{tableName}--失败");
            return rankings;
        }
    }


    /// <summary>
    ///     更新武将月排行
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="playerData"></param>
    public void UpdateGeneralRank(string tableName, List<GeneralRankData> generalData)
    {
        var generalList = QueryGeneralRank(GameData.GeneralRank);
        if (generalList.Count == 0)
        {
            WriteGeneralRank(tableName, generalData);
            return;
        }

        var dic = new Dictionary<string, GeneralRankData>();

        foreach (var item in generalList)
            if (!dic.ContainsKey(item.playerName))
                dic.Add(item.playerName, item);
            else
                PELog.ColorLog(LogColor.Green, $"检测到一个重复的数据--{item.playerName}--");


        foreach (var item in generalData)
            if (dic.ContainsKey(item.playerName))
            {
                dic[item.playerName].combatPower = item.combatPower;
                PELog.ColorLog(LogColor.Green, $"--{item.playerName}--的武将战力更新了，现在还有--{item.combatPower}--");
            }
            else
            {
                PELog.ColorLog(LogColor.Green, $"月排行添加了玩家--{item.playerName}--,武将战力为--{item.combatPower}--");
                generalList.Add(item);
            }

        generalList.Sort((x, y) => y.combatPower.CompareTo(x.combatPower));
        // //从序列的开头返回指定数目的连续元素
        // //最多一百名
        // var topPlayers = generalList.Take(100);
        for (var i = 0; i < generalList.Count; i++) generalList[i].rankIndex = i + 1;

        WriteGeneralRank(GameData.GeneralRank, generalList);
    }


    /// <summary>
    ///     查询武将月排行
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public List<GeneralRankData> QueryGeneralRank(string tableName)
    {
        var ranking = new List<GeneralRankData>();
        try
        {
            using (var connection = new MySqlConnection(ConnectPath))
            {
                connection.Open();
                var SQL = $"SELECT * FROM {tableName}";
                using (var command = new MySqlCommand(SQL, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            ranking.Add(new GeneralRankData
                            {
                                rankIndex = reader.GetInt32("rankIndex"),
                                playerName = reader.GetString("playerName"),
                                combatPower = reader.GetInt32("combatPower"),
                                generals = JsonConvert.DeserializeObject<List<General>>(reader.GetString("generals")),
                                weapons = JsonConvert.DeserializeObject<List<WeaponBase>>(reader.GetString("weapons")),
                            });
                    }
                }
            }

            return ranking;
        }
        catch
        {
            PELog.ColorLog(LogColor.Red, $"查询--{tableName}--失败");
            return ranking;
        }
    }


    /// <summary>
    /// 写入武将排行数据
    /// </summary>
    /// <param name="tableName"></param>
    public void WriteGeneralRank(string tableName, List<GeneralRankData> generalRankDatas)
    {
        using (var connection = new MySqlConnection(ConnectPath))
        {
            connection.Open(); // 打开数据库连接

            var SQL = $@"CREATE TABLE IF NOT EXISTS {tableName} (
    rankIndex INT NOT NULL PRIMARY KEY,
    playerName TEXT NOT NULL,
    combatPower INT NOT NULL,
    generals TEXT NOT NULL,
    weapons TEXT NOT NULL
)";
            using (var command = new MySqlCommand(SQL, connection))
            {
                command.ExecuteNonQuery();
                PELog.ColorLog(LogColor.Green, $"创建MySQL表--{tableName}--成功");
            }


            var delSQL = $"DELETE FROM {tableName}";
            using (var command = new MySqlCommand(delSQL, connection))
            {
                command.ExecuteNonQuery();
                PELog.ColorLog(LogColor.Green, $"{tableName}这个表已经清空");
            }

            foreach (var general in generalRankDatas) // 遍历玩家列表
            {
                var updateSQL = $"INSERT INTO {tableName}(rankIndex, playerName, combatPower,generals,weapons) VALUES (@rankIndex, @playerName, @combatPower,@generals,@weapons)";
                using (var command = new MySqlCommand(updateSQL, connection)) // 创建 SQL 命令
                {
                    command.Parameters.Clear(); // 清除之前的参数

                    command.Parameters.AddWithValue("@rankIndex", general.rankIndex);
                    command.Parameters.AddWithValue("@playerName", general.playerName); // 添加参数
                    command.Parameters.AddWithValue("@combatPower", general.combatPower);
                    command.Parameters.AddWithValue("@generals", JsonConvert.SerializeObject(general.generals));
                    command.Parameters.AddWithValue("@weapons", JsonConvert.SerializeObject(general.weapons));

                    command.ExecuteNonQuery(); // 执行更新
                }
            }
        }
    }
}
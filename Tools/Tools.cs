using PETimer;
using PEUtils;

public class Tools : SingletonBase<Tools>
{
    public override void Init()
    {
        base.Init();
        PELog.InitSettings();
    }

    public override void Update()
    {
        base.Update();
    }


    /// <summary>
    ///     异步定时器   循环次数不可为0，无限循环为-1，次数循环>0
    /// </summary>
    /// <param name="intervel"></param>
    /// <param name="func"></param>
    /// <param name="count"></param>
    /// <param name="sum"></param>
    public void TimerExample(uint intervel, Action<int> func, int count, int sum)
    {
        var tickTimer = new TickTimer(10)
        {
            LogFunc = PELog.Log,
            WarnFunc = PELog.Warn,
            ErrorFunc = PELog.Error
        };
        var taskID = 0;
        Task.Run(async () =>
        {
            await Task.Delay(2000);
            var historyTime = DateTime.UtcNow;
            taskID = tickTimer.AddTask(
                intervel,
                func,
                tid => { PELog.ColorLog(LogColor.Blue, $"tid：{tid} cancel"); },
                count);
            PELog.ColorLog(LogColor.Yellow, $"心跳计时器的ID为{taskID}");
        });
        //独立的线程驱动
        Task.Run(async () =>
        {
            while (true)
            {
                
                tickTimer.HandleTask(); //外部线程回调
                await Task.Delay(2);
            }
        });
    }
}
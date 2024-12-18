/// <summary>
///     事件类
/// </summary>
public class CEvent
{
    /// <summary>
    ///     事件ID
    /// </summary>
    private readonly EGameEvent eventId;

    /// <summary>
    ///     存储事件的参数
    /// </summary>
    private readonly Dictionary<string, object> paramList;

    public CEvent()
    {
        paramList = new Dictionary<string, object>();
    }

    public CEvent(EGameEvent id)
    {
        eventId = id;
        paramList = new Dictionary<string, object>();
    }

    /// <summary>
    ///     用于获取事件的ID
    /// </summary>
    /// <returns></returns>
    public EGameEvent GetEventId()
    {
        return eventId;
    }

    /// <summary>
    ///     添加事件参数
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void AddParam(string name, object value)
    {
        paramList[name] = value;
    }

    /// <summary>
    ///     获取指定名称的参数的值
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public object GetParam(string name)
    {
        if (paramList.ContainsKey(name)) return paramList[name];

        return null;
    }

    /// <summary>
    ///     是否存在指定名称的参数
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool HasParam(string name)
    {
        if (paramList.ContainsKey(name)) return true;

        return false;
    }

    /// <summary>
    ///     获取参数的数量
    /// </summary>
    /// <returns></returns>
    public int GetParamCount()
    {
        return paramList.Count;
    }

    /// <summary>
    ///     获取事件信息
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, object> GetParamList()
    {
        return paramList;
    }
}

/// <summary>
///     事件类型
/// </summary>
public enum EGameEvent
{
    eGameEvent_ErrorStr = 101,
    eGameEvent_ConnectServerFail = 102,
    eGameEvent_ConnectServerSuccess = 103
}
/*
 * Advanced C# messenger by Ilya Suzdalnitski. V1.0
 *
 * Based on Rod Hyde's "CSharpMessenger" and Magnus Wolffelt's "CSharpMessenger Extended".
 *
 * Features:
    * Prevents a MissingReferenceException because of a reference to a destroyed message handler.
    * Option to log all messages
    * Extensive error detection, preventing silent bugs
 *
 * Usage examples:
    1. Messenger.AddListener<GameObject>("prop collected", PropCollected);
       Messenger.Broadcast<GameObject>("prop collected", prop);
    2. Messenger.AddListener<float>("speed changed", SpeedChanged);
       Messenger.Broadcast<float>("speed changed", 0.5f);
 *
 * Messenger cleans up its evenTable automatically upon loading of a new level.
 *
 * Don't forget that the messages that should survive the cleanup, should be marked with Messenger.MarkAsPermanent(string)
 *
 */

//#define LOG_ALL_MESSAGES
//#define LOG_ADD_LISTENER
//#define LOG_BROADCAST_MESSAGE

#define REQUIRE_LISTENER

internal static class EventCenter
{
    public delegate void Callback();

    public delegate void Callback<T>(T arg1);

    public delegate void Callback<T, U>(T arg1, U arg2);

    public delegate void Callback<T, U, V>(T arg1, U arg2, V arg3);

    public delegate void Callback<T, U, V, X>(T arg1, U arg2, V arg3, X arg4);

    public static Dictionary<EGameEvent, Delegate> mEventTable = new();

    //Message handlers that should never be removed, regardless of calling Cleanup
    public static List<EGameEvent> mPermanentMessages = new();


    //Marks a certain message as permanent.
    public static void MarkAsPermanent(EGameEvent eventType)
    {
#if LOG_ALL_MESSAGES
		Debug.Log("Messenger MarkAsPermanent \t\"" + eventType + "\"");
#endif

        mPermanentMessages.Add(eventType);
    }


    public static void Cleanup()
    {
#if LOG_ALL_MESSAGES
		Debug.Log("MESSENGER Cleanup. Make sure that none of necessary listeners are removed.");
#endif

        var messagesToRemove = new List<EGameEvent>();

        foreach (var pair in mEventTable)
        {
            var wasFound = false;

            foreach (var message in mPermanentMessages)
                if (pair.Key == message)
                {
                    wasFound = true;
                    break;
                }

            if (!wasFound)
                messagesToRemove.Add(pair.Key);
        }

        foreach (var message in messagesToRemove) mEventTable.Remove(message);
    }

    public static void PrEGameEventEventTable()
    {
        Console.WriteLine("\t\t\t=== MESSENGER PrEGameEventEventTable ===");

        foreach (var pair in mEventTable) Console.WriteLine("\t\t\t" + pair.Key + "\t\t" + pair.Value);

        Console.WriteLine("\n");
    }

    public static void OnListenerAdding(EGameEvent eventType, Delegate listenerBeingAdded)
    {
#if LOG_ALL_MESSAGES || LOG_ADD_LISTENER
		Debug.Log("MESSENGER OnListenerAdding \t\"" + eventType + "\"\t{" + listenerBeingAdded.Target + " -> " + listenerBeingAdded.Method + "}");
#endif

        if (!mEventTable.ContainsKey(eventType)) mEventTable.Add(eventType, null);

        var d = mEventTable[eventType];
        if (d != null && d.GetType() != listenerBeingAdded.GetType())
            throw new ListenerException(string.Format(
                "Attempting to add listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being added has type {2}",
                eventType, d.GetType().Name, listenerBeingAdded.GetType().Name));
    }

    public static void OnListenerRemoving(EGameEvent eventType, Delegate listenerBeingRemoved)
    {
#if LOG_ALL_MESSAGES
		Debug.Log("MESSENGER OnListenerRemoving \t\"" + eventType + "\"\t{" + listenerBeingRemoved.Target + " -> " + listenerBeingRemoved.Method + "}");
#endif

        if (mEventTable.ContainsKey(eventType))
        {
            var d = mEventTable[eventType];

            if (d == null)
                throw new ListenerException(string.Format(
                    "Attempting to remove listener with for event type \"{0}\" but current listener is null.",
                    eventType));
            if (d.GetType() != listenerBeingRemoved.GetType())
                throw new ListenerException(string.Format(
                    "Attempting to remove listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being removed has type {2}",
                    eventType, d.GetType().Name, listenerBeingRemoved.GetType().Name));
        }
        else
        {
            throw new ListenerException(string.Format(
                "Attempting to remove listener for type \"{0}\" but Messenger doesn't know about this event type.",
                eventType));
        }
    }

    public static void OnListenerRemoved(EGameEvent eventType)
    {
        if (mEventTable[eventType] == null) mEventTable.Remove(eventType);
    }

    public static void OnBroadcasting(EGameEvent eventType)
    {
#if REQUIRE_LISTENER
        if (!mEventTable.ContainsKey(eventType))
        {
        }
#endif
    }

    public static BroadcastException CreateBroadcastSignatureException(EGameEvent eventType)
    {
        return new BroadcastException(string.Format(
            "Broadcasting message \"{0}\" but listeners have a different signature than the broadcaster.", eventType));
    }

    public static void SendEvent(CEvent evt)
    {
        Broadcast(evt.GetEventId(), evt);
    }

    public class BroadcastException : Exception
    {
        public BroadcastException(string msg)
            : base(msg)
        {
        }
    }

    public class ListenerException : Exception
    {
        public ListenerException(string msg)
            : base(msg)
        {
        }
    }


    //Disable the unused variable warning
#pragma warning disable 0414
    //Ensures that the MessengerHelper will be created automatically upon start of the game.
//	static private MessengerHelper mMessengerHelper = ( new GameObject("MessengerHelper") ).AddComponent< MessengerHelper >();
#pragma warning restore 0414


    #region 添加监听事件

    //No parameters
    public static void AddListener(EGameEvent eventType, Callback handler)
    {
        OnListenerAdding(eventType, handler);
        mEventTable[eventType] = (Callback)mEventTable[eventType] + handler;
    }

    //Single parameter
    public static void AddListener<T>(EGameEvent eventType, Callback<T> handler)
    {
        OnListenerAdding(eventType, handler);
        mEventTable[eventType] = (Callback<T>)mEventTable[eventType] + handler;
    }

    //Two parameters
    public static void AddListener<T, U>(EGameEvent eventType, Callback<T, U> handler)
    {
        OnListenerAdding(eventType, handler);
        mEventTable[eventType] = (Callback<T, U>)mEventTable[eventType] + handler;
    }

    //Three parameters
    public static void AddListener<T, U, V>(EGameEvent eventType, Callback<T, U, V> handler)
    {
        OnListenerAdding(eventType, handler);
        mEventTable[eventType] = (Callback<T, U, V>)mEventTable[eventType] + handler;
    }

    //Four parameters
    public static void AddListener<T, U, V, X>(EGameEvent eventType, Callback<T, U, V, X> handler)
    {
        OnListenerAdding(eventType, handler);
        mEventTable[eventType] = (Callback<T, U, V, X>)mEventTable[eventType] + handler;
    }

    #endregion


    #region 移除监听事件

    //No parameters
    public static void RemoveListener(EGameEvent eventType, Callback handler)
    {
        OnListenerRemoving(eventType, handler);
        mEventTable[eventType] = (Callback)mEventTable[eventType] - handler;
        OnListenerRemoved(eventType);
    }

    //Single parameter
    public static void RemoveListener<T>(EGameEvent eventType, Callback<T> handler)
    {
        OnListenerRemoving(eventType, handler);
        mEventTable[eventType] = (Callback<T>)mEventTable[eventType] - handler;
        OnListenerRemoved(eventType);
    }

    //Two parameters
    public static void RemoveListener<T, U>(EGameEvent eventType, Callback<T, U> handler)
    {
        OnListenerRemoving(eventType, handler);
        mEventTable[eventType] = (Callback<T, U>)mEventTable[eventType] - handler;
        OnListenerRemoved(eventType);
    }

    //Three parameters
    public static void RemoveListener<T, U, V>(EGameEvent eventType, Callback<T, U, V> handler)
    {
        OnListenerRemoving(eventType, handler);
        mEventTable[eventType] = (Callback<T, U, V>)mEventTable[eventType] - handler;
        OnListenerRemoved(eventType);
    }

    //Four parameters
    public static void RemoveListener<T, U, V, X>(EGameEvent eventType, Callback<T, U, V, X> handler)
    {
        OnListenerRemoving(eventType, handler);
        mEventTable[eventType] = (Callback<T, U, V, X>)mEventTable[eventType] - handler;
        OnListenerRemoved(eventType);
    }

    #endregion


    #region 广播

    //No parameters
    public static void Broadcast(EGameEvent eventType)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
		Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (mEventTable.TryGetValue(eventType, out d))
        {
            var callback = d as Callback;

            if (callback != null)
                callback();
            else
                throw CreateBroadcastSignatureException(eventType);
        }
    }

    //Single parameter
    public static void Broadcast<T>(EGameEvent eventType, T arg1)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
		Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (mEventTable.TryGetValue(eventType, out d))
        {
            var callback = d as Callback<T>;

            if (callback != null)
                callback(arg1);
            else
                throw CreateBroadcastSignatureException(eventType);
        }
    }

    //Two parameters
    public static void Broadcast<T, U>(EGameEvent eventType, T arg1, U arg2)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
		Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (mEventTable.TryGetValue(eventType, out d))
        {
            var callback = d as Callback<T, U>;

            if (callback != null)
                callback(arg1, arg2);
            else
                throw CreateBroadcastSignatureException(eventType);
        }
    }

    //Three parameters
    public static void Broadcast<T, U, V>(EGameEvent eventType, T arg1, U arg2, V arg3)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
		Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (mEventTable.TryGetValue(eventType, out d))
        {
            var callback = d as Callback<T, U, V>;

            if (callback != null)
                callback(arg1, arg2, arg3);
            else
                throw CreateBroadcastSignatureException(eventType);
        }
    }

    //Four parameters
    public static void Broadcast<T, U, V, X>(EGameEvent eventType, T arg1, U arg2, V arg3, X arg4)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
		Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (mEventTable.TryGetValue(eventType, out d))
        {
            var callback = d as Callback<T, U, V, X>;

            if (callback != null)
                callback(arg1, arg2, arg3, arg4);
            else
                throw CreateBroadcastSignatureException(eventType);
        }
    }

    #endregion
}
using System;
using System.Collections.Generic;
using us.frostraptor.modUtils.logging;

namespace IRBTModUtils.Logging
{

    //public static class LogExtensions
    //{
    //    public delegate void LogDelegate(string message);

    //    static Dictionary<SimpleModLogger, LogDelegate> DelegateCache = new Dictionary<SimpleModLogger, LogDelegate>();

    //    public static LogDelegate Debug(this SimpleModLogger logger)
    //    {
    //        if (!logger.IsDebug) return null;

    //        bool hasDelegate = DelegateCache.TryGetValue(logger, out LogDelegate logDel);
    //        if (!hasDelegate)
    //        {
    //            logDel = (LogDelegate)Delegate.CreateDelegate(typeof(LogDelegate), logger, "log");
    //            DelegateCache.Add(logger, logDel);
    //        }
    //        return logDel;

    //    }

    //    public static LogDelegate Trace(this SimpleModLogger logger)
    //    {
    //        if (!logger.IsDebug) return null;

    //        LogDelegate logDel;
    //        if (DelegateCache.ContainsKey(logger))
    //        {
    //            logDel = DelegateCache[logger];
    //        }
    //        else
    //        {
    //            logDel = (LogDelegate)Delegate.CreateDelegate(typeof(LogDelegate), logger, "log");
    //            DelegateCache.Add(logger, logDel);
    //        }
    //        return logDel;

    //    }
    //}
}

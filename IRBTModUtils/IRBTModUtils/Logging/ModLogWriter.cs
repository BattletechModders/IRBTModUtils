﻿using HBS.Logging;
using IRBTModUtils.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace IRBTModUtils.Logging
{

    public struct ModLogWriter
    {
        // Lazy instantiated asynchronous worker, will toggle when first accessed. Async as in thread, not as in coroutine.
        AsyncLogWorker Async = AsyncLogWorker._instance;
        private readonly StreamWriter LogStream;
        public readonly ILog HBSLogger;
        public readonly string Label;

        // Formatted date time placeholder
        string now;
        bool isSync = false;
        DateTime dateTime;
        LogLevel Level;

        public ModLogWriter(StreamWriter sw, string label)
        {
            // Disable Async log path if user specifies. Otherwise lazy instantiate
            LogStream = sw;
            if (label != null)
            {
                HBSLogger = Logger.GetLogger(label);
                Label = label;
            }
            else
            {
                HBSLogger = null;
                Label = null;
            }
            Level = LogLevel.Warning;
        }


        /// <summary>
        /// ModLogWriter write function without exception handling and toggleable synchronous/multithreaded async write
        /// </summary>        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string message)
        {
            dateTime = DateTime.UtcNow;
            if (Async._bRunning == false)
            {
                isSync = true;
                now = FastFormatDate.ToHHmmssfff_(ref dateTime);
                SendMessageSync(now, message, LogStream);
            }
            else
            {
                Async.SendMessageDate(message, dateTime.Ticks, LogStream);
            }
            if (HBSLogger != null)
            {
                // Reformat the HBS Log Time, in async mode so time will not be set prior
                if (!isSync)
                {
                    now = FastFormatDate.ToHHmmssfff_(ref dateTime);
                }
                HBSLogger.LogAtLevel(Level, now + " " + Label + " " + message);
                isSync = false;
            }
        }


        /// <summary>
        /// ModLogWriter write function with exception handling and toggleable synchronous/multithreaded async write
        /// </summary>        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Exception e, string message = null)
        {

            dateTime = DateTime.UtcNow;


            // Async Fallback implementation
            if (Async._bRunning == false)
            {
                isSync = true;
                now = FastFormatDate.ToHHmmssfff_(ref dateTime);
                SendMessageExceptSync(now, e, message, LogStream);
            }
            else
            {
                Async.SendMessageDateExcept(message, e, dateTime.Ticks, LogStream);
            }
            if (HBSLogger != null)
            {
                // Reformat the HBS Log Time, in async mode so time will not be set prior
                if (isSync)
                {
                    now = FastFormatDate.ToHHmmssfff_(ref dateTime);
                }
                if (message != null) LogStream.WriteLine(now + message);
                HBSLogger.LogAtLevel(Level, now + " " + Label + " " + e?.Message);
                HBSLogger.LogAtLevel(Level, now + " " + Label + "Stacktrace available in mod logs");
                isSync = false;
            }
        }
        
        
        /// <summary>
        /// Existing synchronous implementation of ModLogWriter without exception handling or date formatting
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void SendMessageSync(string now, string message, StreamWriter LogStream)
        {
            Async.StartSyncStat();
            LogStream.WriteLine(now + " " + message);
            LogStream.Flush();
            Async.StopSyncStat();
        }


        /// <summary>
        /// Existing synchronous implementation of ModLogWriter with exception handling and without date formatting
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void SendMessageExceptSync(string now, Exception e, string message, StreamWriter LogStream)
        {
            if (message != null) LogStream.WriteLine(now + " " + message);
            LogStream.WriteLine(now + " " + e?.Message);
            LogStream.WriteLine(now + " " + e?.StackTrace);

            if (e?.InnerException != null)
            {
                LogStream.WriteLine(now + " " + e?.InnerException.Message);
                LogStream.WriteLine(now + " " + e?.InnerException.StackTrace);

                if (e.InnerException?.InnerException != null)
                {
                    LogStream.WriteLine(now + " " + e?.InnerException.InnerException.Message);
                    LogStream.WriteLine(now + " " + e?.InnerException.InnerException.StackTrace);
                }
            }

            LogStream.Flush();
        }
    }
}

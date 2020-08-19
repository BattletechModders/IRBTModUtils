using HBS.Logging;
using System;
using System.IO;

namespace us.frostraptor.modUtils.logging
{

    // Logs to a static file inside the mod class 
    public class IntraModLogger
    {

        private StreamWriter LogStream;
        private readonly string LogFile;
        private readonly string LogLabel;
        private readonly ILog HBSLogger;

        public readonly bool IsDebug;
        public readonly bool IsTrace;

        public IntraModLogger(string modDir, string logFilename = "mod", bool isDebug = false, bool isTrace = false) : this (modDir, logFilename, "mod", isDebug, isTrace)
        {
        }

        public IntraModLogger(string modDir, string logFilename = "mod", string logLabel = "mod", bool isDebug = false, bool isTrace = false)
        {
            if (LogFile == null)
            {
                LogFile = Path.Combine(modDir, $"{logFilename}.log");
            }

            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }

            LogStream = File.AppendText(LogFile);

            LogLabel = "<" + logLabel + ">";
            IsDebug = isDebug;
            IsTrace = isTrace;

            HBSLogger = HBS.Logging.Logger.GetLogger(logLabel);
        }

        public void Trace(string message)
        {
            if (IsTrace)
            {
                Log(message);
            }
        }

        public void Debug(string message)
        {
            if (IsDebug)
            {
                Log(message);
            }
        }

        public void Info(string message)
        {
            Log(message);
        }

        public void Warn(string message)
        {
            Log("WARN " + message);
            HBSLogger.LogAtLevel(LogLevel.Warning, this.LogLabel + message);
        }

        public void Error(string message)
        {
            Log("ERR " + message);
            HBSLogger.LogAtLevel(LogLevel.Error, this.LogLabel + message);
        }

        public void Error(Exception e)
        {
            Log("ERR " + e.Message);
            HBSLogger.LogAtLevel(LogLevel.Error, this.LogLabel + e.Message);
        }

        public void Error(string message, Exception e)
        {
            Log("ERR " + message);
            Log("ERR " + e.Message);
            Log("ERR " + e.StackTrace);

            HBSLogger.LogAtLevel(LogLevel.Error, this.LogLabel + message, e);
        }

        public void Log(string message)
        {
            string now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
            LogStream.WriteLine($"{now} - {message}");
            LogStream.Flush();
        }

    }
}


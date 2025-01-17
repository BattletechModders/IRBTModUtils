using HBS.Logging;
using System;
using System.IO;

namespace IRBTModUtils.Logging
{
    // Basic logger that does not evaluate log messages until the specified level is enabled.
    //  More performant that other loggers due to the short-circuited evaluation, which prevents
    //  string creation (and garbage). 
    public class DeferringLogger
    {
        private readonly string LogFile;

        private readonly StreamWriter LogStream;
        //private readonly FileStream fileStream;
        public readonly string LogLabel;

        public readonly bool IsDebug;
        public readonly bool IsTrace;

        readonly ModLogWriter ModOnlyWriter;
        readonly ModLogWriter CombinedWriter;

        public DeferringLogger(string modDir, string logFilename = "mod", bool isDebug = false, bool isTrace = false) : this(modDir, logFilename, "mod", isDebug, isTrace)
        {
        }

        public DeferringLogger(string modDir, string logFilename = "mod", string logLabel = "mod", bool isDebug = false, bool isTrace = false)
        {
            if (LogFile == null)
            {
                LogFile = Path.Combine(modDir, $"{logFilename}.log");
            }

            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }

            LogStream = new StreamWriter(LogFile,true,new System.Text.UTF8Encoding(false), 65535);

            LogLabel = "<" + logLabel + ">";

            IsDebug = isDebug;
            IsTrace = isTrace;

            ModOnlyWriter = new ModLogWriter(LogStream, null);
            CombinedWriter = new ModLogWriter(LogStream, LogLabel);
        }

        public Nullable<ModLogWriter> Trace
        {
            get { return IsTrace ? (Nullable<ModLogWriter>)ModOnlyWriter : null; }
            private set { }
        }

        public Nullable<ModLogWriter> Debug
        {
            get { return IsDebug ? (Nullable<ModLogWriter>)ModOnlyWriter : null; }
            private set { }
        }

        public Nullable<ModLogWriter> Info
        {
            get { return (Nullable<ModLogWriter>)ModOnlyWriter; }
            private set { }
        }

        public Nullable<ModLogWriter> Warn
        {
            get 
            {
                if (CombinedWriter.HBSLogger.IsWarningEnabled) return CombinedWriter;
                else return (Nullable<ModLogWriter>)ModOnlyWriter;
            }
            private set { }
        }

        public Nullable<ModLogWriter> Error
        {
            get
            {
                if (CombinedWriter.HBSLogger.IsErrorEnabled) return CombinedWriter;
                else return (Nullable<ModLogWriter>)ModOnlyWriter;
            }
            private set { }
        }
    }
    
}


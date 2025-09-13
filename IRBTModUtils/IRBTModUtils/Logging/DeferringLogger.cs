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
        public readonly string LogLabel;

        public readonly bool IsDebug;
        public readonly bool IsTrace;
        public readonly bool IsNoOp;

        readonly ModLogWriter ModOnlyWriter;
        readonly ModLogWriter CombinedWriter;

        public DeferringLogger(string modDir, string logFilename = "mod", bool isDebug = false, bool isTrace = false) : this(modDir, logFilename, "mod", isDebug, isTrace)
        {
        }

        public DeferringLogger(string modDir, string logFilename = "mod", string logLabel = "mod", bool isDebug = false, bool isTrace = false)
        {
            IsDebug = isDebug;
            IsTrace = isTrace;

            if (modDir != null && modDir.Length > 0)
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

                ModOnlyWriter = new ModLogWriter(LogStream, null);
                CombinedWriter = new ModLogWriter(LogStream, LogLabel);

                IsNoOp = false;
            }
            else
            {
                Console.WriteLine("Unable to configure logger for writing, all messages will be dropped.");
                ModOnlyWriter = new ModLogWriter(null, null);
                CombinedWriter = new ModLogWriter(null, null);

                IsNoOp = true;
            }


        }

        public Nullable<ModLogWriter> Trace
        {
            get 
            {
                if (IsNoOp) return null;
                return IsTrace ? (Nullable<ModLogWriter>)ModOnlyWriter : null; 
            }
            private set { }
        }

        public Nullable<ModLogWriter> Debug
        {
            get 
            {
                if (IsNoOp) return null;
                return IsDebug ? (Nullable<ModLogWriter>)ModOnlyWriter : null; 
            }
            private set { }
        }

        public Nullable<ModLogWriter> Info
        {
            get 
            {
                return IsNoOp ? null : (Nullable<ModLogWriter>)ModOnlyWriter;
            }
            private set { }
        }

        public Nullable<ModLogWriter> Warn
        {
            get 
            {
                if (IsNoOp) return null;
                else
                {
                    if (CombinedWriter.HBSLogger.IsWarningEnabled) return CombinedWriter;
                    else return (Nullable<ModLogWriter>)ModOnlyWriter;
                }
            }
            private set { }
        }

        public Nullable<ModLogWriter> Error
        {
            get
            {
                if (IsNoOp) return null;
                else
                {
                    if (CombinedWriter.HBSLogger.IsWarningEnabled) return CombinedWriter;
                    else return (Nullable<ModLogWriter>)ModOnlyWriter;
                }
            }
            private set { }
        }
    }
    
}


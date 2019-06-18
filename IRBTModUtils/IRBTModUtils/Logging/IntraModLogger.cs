using System;
using System.IO;

namespace us.frostraptor.modUtils.logging {

    // Logs to a static file inside the mod class 
    public class IntraModLogger {

        private StreamWriter LogStream;
        private readonly string LogFile;

        private readonly bool IsDebug;
        private readonly bool IsTrace;

        public IntraModLogger(string modDir, string logName="mod", bool isDebug=false, bool isTrace=false) {
            if (LogFile == null) {
                LogFile = Path.Combine(modDir, $"{logName}.log");
            }
            if (File.Exists(LogFile)) {
                File.Delete(LogFile);
            }

            LogStream = File.AppendText(LogFile);
            IsDebug = isDebug;
            IsTrace = isTrace;

        }

        public void Trace(string message) { if (IsTrace) { Log(message); } }

        public void Debug(string message) { if (IsDebug) { Log(message); } }

        public void Info(string message) { Log(message); }

        public void Warn(string message) { Log("[WARNING]" + message); }

        public void Error(string message) { Log("[ERROR]" + message); }
        public void Error(Exception e) { Log("[ERROR]" + e.Message); }

        private void Log(string message) {
            string now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
            LogStream.WriteLine($"{now} - {message}");
            LogStream.Flush();
        }

    }
}

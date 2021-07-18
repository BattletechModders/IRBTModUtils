using HBS.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRBTModUtils.Logging
{

    public struct ModLogWriter
    {
        readonly StreamWriter LogStream;
        public readonly ILog HBSLogger;
        public readonly string Label;
        LogLevel Level;

        public ModLogWriter(StreamWriter sw, string label)
        {
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

        public void Write(string message)
        {
            // Write our internal log
            string now = DateTime.UtcNow.ToString("HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
            LogStream.WriteLine(now + " " + message);
            LogStream.Flush();

            if (HBSLogger != null)
            {
                // Write the HBS logging
                HBSLogger.LogAtLevel(Level, now + " " + Label + " " + message);
            }
        }

        public void Write(Exception e, string message = null)
        {
            // Write our internal log
            string now = DateTime.UtcNow.ToString("HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
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

            if (HBSLogger != null)
            {
                // Write the HBS logging
                if (message != null) LogStream.WriteLine(now + message);
                HBSLogger.LogAtLevel(Level, now + " " + Label + " " + e?.Message);
                HBSLogger.LogAtLevel(Level, now + " " + Label + "Stacktrace available in mod logs");
            }
        }
    }
}

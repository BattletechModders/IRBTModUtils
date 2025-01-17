using System.Diagnostics;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using IRBTModUtils.Math;
using System.Text;
using System.Runtime.CompilerServices;
using System.IO;
namespace IRBTModUtils.Logging
{
    /// <summary>
    /// Statistics class meant for use with AsyncLogging and a shared stopwatch
    /// </summary>
    [ComVisible(false)]
    [HostProtection(Synchronization = true, ExternalThreading = true)]
    public class AsyncLogStat
    {
        private int _iterations = 0;
        private double _msElapsed = 0;
        private Stopwatch _stopwatch = new Stopwatch();
        private string _name = "";

        private double _meanMs = 0;
        private double _lastMeanMs = 0;
        private double _varianceMs = 0;

        public AsyncLogStat(ref Stopwatch watch, string Name)
        {
            _name = Name;
            Reset();
        }

        public void Reset()
        {
            _iterations = 0;
            _msElapsed = 0;
            _meanMs = 0;
            _lastMeanMs = 0;
            _varianceMs = 0;

        }

        public void Start()
        {
            _stopwatch.Restart();
        }

        public void Stop()
        {
            _stopwatch.Stop();
            _iterations += 1;
            double sampleTimeMs  = _stopwatch.Elapsed.TotalMilliseconds;
            _msElapsed += sampleTimeMs;

            // Generate sample mean and variance
            StatUtils.WelfordAlgorithm(
                sampleTimeMs, 
                ref _iterations, ref _meanMs, ref _lastMeanMs, ref _varianceMs);

        }

        public string PrintInstance()
        {
            if (_name == null)      { return $"STAT: [ERROR] No Statistic Name Set"; }
            if (_stopwatch == null) { return $"STAT: [ERROR] {_name} Stopwatch is null"; }


            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("|{0,15} |{1,15} |{2,20} |{3,20} |", _name, _iterations, $"{System.Math.Round(_msElapsed, 3)}" + " ms", $"{System.Math.Round(_meanMs * 1E3, 3)}" + " us");

            // Convert milliseconds to microseconds (1E3)
            return sb.ToString();
        }

    }

    /// <summary>
    /// Statistics collection to aggregate performance numbers for asynchronous log dispatch
    /// </summary>
    //[ComVisible(false)]
    //[HostProtection(Synchronization = true, ExternalThreading = true)]
    public class AsyncLogStatCollection
    {
        Stopwatch _dispatchWatch = new Stopwatch();
        Stopwatch _asyncWatch = new Stopwatch();

        AsyncLogStat _dispatchTime;
        AsyncLogStat _writeTime;
        AsyncLogStat _encodeTime;
        AsyncLogStat _flushTime;
        AsyncLogStat _syncTime;

        private long _burstCount = 0;
        private long _bytesWritten  = 0;
        private long _maxMessageBytes = 0;
        private long _msgCount = 0;
        private long _windowCount = 0;

        private long _sampleWindow = 1000;
        private long _prewarmWindow = 1000;
        private bool _clearedPrewarm = false;

        public AsyncLogStatCollection()
        {
            _writeTime = new AsyncLogStat(ref _asyncWatch, "Write");
            _encodeTime = new AsyncLogStat(ref _asyncWatch, "Encode");
            _dispatchTime = new AsyncLogStat(ref _dispatchWatch, "Dispatch");
            _flushTime = new AsyncLogStat(ref _asyncWatch, "Flush");
            _syncTime = new AsyncLogStat(ref _dispatchWatch, "Sync");
            _burstCount = 0;
            _msgCount = 0;
            _bytesWritten = 0;
            _maxMessageBytes = 0;
        }


        public bool Sample(long processCount, long maxMessageBytes, long bytesWritten, string prefix, StreamWriter sw)
        {
            _windowCount += processCount;
            
            if (processCount > _burstCount)
            {
               _burstCount = processCount;
            }
            
            if (maxMessageBytes > _maxMessageBytes)
            {
               _maxMessageBytes = maxMessageBytes;
            }

            _bytesWritten += bytesWritten;
            _msgCount += processCount;


            if (!_clearedPrewarm)
            {
                if (_windowCount >= _prewarmWindow)
                {
                    sw.WriteLine(ToString(prefix));
                    sw.Flush();
                    ResetAll();
                    _windowCount = _windowCount - _sampleWindow;
                    _clearedPrewarm = true;
                    return true;
                }
            }
            if (_windowCount >= _sampleWindow)
            {
                    
                sw.WriteLine(ToString(prefix));
                sw.Flush();
                _windowCount = _windowCount - _sampleWindow;
                return true;
            }
            return false;
        }

        public void ResetAll()
        {
            _writeTime.Reset();
            _encodeTime.Reset();
            _dispatchTime.Reset();
            _flushTime.Reset();
            _syncTime.Reset();
            _burstCount = 0;
            _bytesWritten = 0;
            _msgCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CalculateBurst(long burstCount)
        {
            if (burstCount > _burstCount)
            {
               _burstCount = burstCount;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TallyBytesWritten(long bytesWritten)
        {
            _bytesWritten += bytesWritten;
        }

        void AppendHeader(StringBuilder sb, string prefix = "")
        {
            if (prefix != "") { sb.Append(prefix); }
            sb.AppendFormat("|{0,15} |{1,15} |{2,20} |{3,20} |\n", "Stat", "Count", "Total", "Mean");
            if (prefix != "") { sb.Append(prefix); }
            sb.Append("|----------------|----------------|---------------------|---------------------|\n");
        }

        void AppendOverallStats(StringBuilder sb, string prefix = "")
        {
            if (prefix != "") { sb.Append(prefix); }
            sb.AppendLine($"Message Count: {_msgCount} Burst Count: {_burstCount} Burst Bytes {_maxMessageBytes} Bytes Written: {_bytesWritten}\n");
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            AppendHeader(sb);
            AppendOverallStats(sb);
            sb.AppendLine(_writeTime.PrintInstance());
            sb.AppendLine(_encodeTime.PrintInstance());
            sb.AppendLine(_dispatchTime.PrintInstance());
            sb.AppendLine(_flushTime.PrintInstance());
            sb.AppendLine(_syncTime.PrintInstance());
            return sb.ToString();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string prefix)
        {
            StringBuilder sb = new StringBuilder();
            if (!_clearedPrewarm)
            {
                AppendOverallStats(sb, prefix + "(Prewarm) ");
            }
            else
            {                
                AppendOverallStats(sb, prefix);
            }
            char[] tab = new char[prefix.Length];
            for(int i = 0; i < tab.Length; i++)
            {
                tab[i] = ' ';
            }
            string tabStr = new string(tab);
            AppendHeader(sb, tabStr);
            sb.AppendLine(tabStr+ _writeTime.PrintInstance());
            sb.AppendLine(tabStr + _encodeTime.PrintInstance());
            sb.AppendLine(tabStr + _dispatchTime.PrintInstance());
            sb.AppendLine(tabStr + _flushTime.PrintInstance());
            sb.AppendLine(tabStr + _syncTime.PrintInstance());
            return sb.ToString();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartDispatch() { _dispatchTime.Start(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopDispatch() { _dispatchTime.Stop(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartWrite() { _writeTime.Start(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopWrite() { _writeTime.Stop(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartEncode() { _encodeTime.Start(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopEncode() { _encodeTime.Stop(); }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartFlush() { _flushTime.Start(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopFlush() { _flushTime.Stop(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartSync() { _syncTime.Start(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopSync() { _syncTime.Stop(); }

    }
}

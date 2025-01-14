using System.Diagnostics;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using IRBTModUtils.Math;
using System.Text;
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

        public AsyncLogStatCollection()
        {
            _writeTime = new AsyncLogStat(ref _asyncWatch, "Write");
            _encodeTime = new AsyncLogStat(ref _asyncWatch, "Encode");
            _dispatchTime = new AsyncLogStat(ref _dispatchWatch, "Dispatch");
            _flushTime = new AsyncLogStat(ref _asyncWatch, "Flush");
            _syncTime = new AsyncLogStat(ref _dispatchWatch, "Sync");
        }

        public void ResetAll()
        {
            _writeTime.Reset();
            _encodeTime.Reset();
            _dispatchTime.Reset();
            _flushTime.Reset();
            _syncTime.Reset();
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("|{0,15} |{1,15} |{2,20} |{3,20} |\n", "Stat", "Count", "Total", "Mean");
            sb.Append("|----------------|----------------|---------------------|---------------------|\n");
            sb.AppendLine(_writeTime.PrintInstance());
            sb.AppendLine(_encodeTime.PrintInstance());
            sb.AppendLine(_dispatchTime.PrintInstance());
            sb.AppendLine(_flushTime.PrintInstance());
            sb.AppendLine(_syncTime.PrintInstance());
            return sb.ToString();
        }
        
        public string ToString(string Prefix)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Prefix);
            sb.AppendFormat("|{0,15} |{1,15} |{2,20} |{3,20} |\n", "Stat", "Count", "Total", "Mean");
            sb.Append(Prefix);
            sb.Append("|----------------|----------------|---------------------|---------------------|\n");
            sb.AppendLine(Prefix + _writeTime.PrintInstance());
            sb.AppendLine(Prefix + _encodeTime.PrintInstance());
            sb.AppendLine(Prefix + _dispatchTime.PrintInstance());
            sb.AppendLine(Prefix + _flushTime.PrintInstance());
            sb.AppendLine(Prefix + _syncTime.PrintInstance());
            return sb.ToString();
        }


        public void StartDispatch() { _dispatchTime.Start(); }
        public void StopDispatch() { _dispatchTime.Stop(); }

        public void StartWrite() { _writeTime.Start(); }
        public void StopWrite() { _writeTime.Stop(); }

        public void StartEncode() { _encodeTime.Start(); }
        public void StopEncode() { _encodeTime.Stop(); }


        public void StartFlush() { _flushTime.Start(); }
        public void StopFlush() { _flushTime.Stop(); 
        
        }
        public void StartSync() { _syncTime.Start(); }
        public void StopSync() { _syncTime.Stop(); }

    }
}

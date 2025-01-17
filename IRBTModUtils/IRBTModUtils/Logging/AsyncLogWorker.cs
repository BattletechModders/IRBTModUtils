using IRBTModUtils.Helper;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Threading;

namespace IRBTModUtils.Logging
{


    /// <summary>
    /// Asynchronous logger that is lazy instantiated and processes messages through an MPMC queue. Offloads log processing from main thread.
    /// </summary>
    [ComVisible(false)]
    [HostProtection(Synchronization = true, ExternalThreading = true)]
    public class AsyncLogWorker
    {
        private static readonly Lazy<AsyncLogWorker> lazy =
            new Lazy<AsyncLogWorker>(() => new AsyncLogWorker());

        public static AsyncLogWorker _instance
        {
            get
            {
                return lazy.Value;
            }
        }

        AsyncLogStatCollection _asyncStats = new AsyncLogStatCollection();

        // Buffer memory. Note, relatively small since pointers not data
        public MPMCQueue queue = new MPMCQueue(1024 * 1024);

        // Local queue for freeing up concurrent queue during bursting
        AsyncLogMessage[] messages = new AsyncLogMessage[1024 * 1024];

        // Constants
        private const int MEMORY_SIZE = 4 * 1024; // 4 KiB
        private const int MEMORY_PAGES = 512;        // UTF16 -> UTF8 Expansion size
        private const int MEMORY_TOTAL = MEMORY_SIZE * MEMORY_PAGES;
        private const int HHMMSSFFF_DATE_LEN = 13;
        private const string STATUS_LOG_NAME = "irbt_async.log";
        private const string BIST_LOG_NAME = "irbt_async_bist.log";
        private int NEW_LINE_SIZE = Environment.NewLine.Length;
        private string NEW_LINE = Environment.NewLine;


        // Allocate 512 4KiB memory pages, for a total of 2,097.152 KB
        // Worst case experienced so far was 200 KB.
        public char[] buffer = new char[MEMORY_TOTAL];

        // Writer
        public StreamWriter _statusLog = null;

        public bool _bRunning = false;

        string _statusLogPath = "";
        string _bistLogPath = "";


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SendStatusMessage(string message)
        {
            var utc = DateTime.UtcNow;
            string now = FastFormatDate.ToHHmmssfff_(ref utc);
            _statusLog.WriteLine(now + message);
            _statusLog.Flush();
        }

        private AsyncLogWorker()
        {
            if (Directory.Exists(Mod.ModDir))
            {
                _statusLogPath = Path.Combine(Mod.ModDir + "/" + STATUS_LOG_NAME);
            }
            else
            {
                _statusLogPath = Path.Combine(STATUS_LOG_NAME);
            }

            _statusLog = new StreamWriter(_statusLogPath);

            if (!Mod.Config.AsyncLogging)
            {
                SendStatusMessage("AsyncLog [INIT] Synchronous Logging Enabled - Shutting Down");
                return;
            }

            SendStatusMessage("AsyncLog [INIT] Asynchronous Logging Enabled - Starting Thread");
            StartThread();
            StartBIST();
            SendStatusMessage("AsyncLog [INIT] Initialization Complete");
        }




        /// <summary>
        /// Quickly dequeue references to thread local memory to free up buffer, avoid flush delay, and reduce contention
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int DoubleBuffer()
        {
            int processCount = 0;
            // Double buffer references to managed memory quickly, to free up queue space
            while (queue.TryDequeue(out object item))
            {
                if (item != null)
                {
                    // If the writer is null, a dispatch may have a closed/bad stream
                    if (((AsyncLogMessage)item)._writer == null) { continue; }

                    // Empty messages are handled later, add to buffer and continue;
                    messages[processCount] = (AsyncLogMessage)item;
                    processCount++;
                }
            }
            return processCount;
        }

        /// <summary>
        /// Dequeues messages into a thread local buffer and flushes contiguasly based on writer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        long ProcessMessages()
        {
            // Note, observation of stats can throw off performance due to overhead/readout. Dispatch times worsened due to attempts to be nicer to the CPU
            // Original flush implementation preserved to slow producer down for HDD users. Pending Pinvoke based optimization for encoding.

            DateTime curUtc;
            StreamWriter curWriter = null;
            int flushCount = 0;
            long ticks = 0;
            long bytesWritten = 0;
            long maxMessageBytes = 0;
            int truncatedSize = 0;
            string nowStr = "";

            int processCount = DoubleBuffer();

            // Begin processing messages
            for (int i = 0; i < processCount; i++)
            {
                // On init, make sure to assign writer. Should be null checked prior to buffering
                if (i == 0) { curWriter = messages[i]._writer; }

                AsyncLogMessage item = messages[i];

                // Cut the tape, and write existing contiguous data
                if (item._writer != curWriter)
                {
                    _asyncStats.StartFlush();
                    curWriter.Flush();
                    _asyncStats.StopFlush();
                    flushCount++;
                    curWriter = item._writer;
                }

                _asyncStats.StartWrite();

                // Cache date during message bursts
                if (ticks != item._ticks)
                {
                    curUtc = new DateTime(item._ticks);
                    ticks = item._ticks;
                    FastFormatDate.ToHHmmssfff(ref curUtc, ref buffer);
                }

                // Truncation Handling
                truncatedSize = item._message.Length;
                if (item._message.Length > MEMORY_TOTAL)
                {
                    // Truncate and leave space for newline and date str
                    truncatedSize = MEMORY_TOTAL - NEW_LINE_SIZE - HHMMSSFFF_DATE_LEN;
                }
                for (int j = 0; j < truncatedSize; j++)
                {
                    buffer[j + HHMMSSFFF_DATE_LEN] = item._message[j];
                }

                // Concat line ending, branching hopefully optimized by JIT. Windows \r\n, Unix: \n
                // See: https://learn.microsoft.com/en-us/dotnet/api/system.environment.newline?view=netframework-4.7.2
                buffer[HHMMSSFFF_DATE_LEN + item._message.Length] = NEW_LINE[0];
                if (NEW_LINE_SIZE == 2)
                {
                    buffer[HHMMSSFFF_DATE_LEN + item._message.Length + 1] = NEW_LINE[1];
                }
                int outputLength = HHMMSSFFF_DATE_LEN + truncatedSize + NEW_LINE_SIZE;


                // Output
                curWriter.Write(buffer, 0, outputLength);

                bytesWritten += outputLength;
                if (maxMessageBytes < outputLength)
                {
                    maxMessageBytes = outputLength;
                }

                // Exception handling not optimized, dynamically allocate for now and avoid truncation
                if (item._e != null)
                {
                    item._writer.WriteLine(nowStr + item._e?.StackTrace);

                    if (item._e?.InnerException != null)
                    {
                        item._writer.WriteLine(nowStr + item._e?.InnerException.Message);
                        item._writer.WriteLine(nowStr + item._e?.InnerException.StackTrace);

                        if (item._e.InnerException?.InnerException != null)
                        {
                            item._writer.WriteLine(nowStr + item._e?.InnerException?.InnerException.Message);
                            item._writer.WriteLine(nowStr + item._e?.InnerException?.InnerException.StackTrace);
                        }
                    }
                }
                _asyncStats.StopWrite();

                // Last item, cut the current tape and flush. Decrement count for indexing
                if (i == processCount - 1)
                {

                    _asyncStats.StartFlush();
                    curWriter.Flush();
                    flushCount++;
                    _asyncStats.StopFlush();
                }
            }

            if (processCount > 0)
            {
                var printUtc = DateTime.UtcNow;
                nowStr = FastFormatDate.ToHHmmssfff_(ref printUtc);
                _asyncStats.Sample(processCount, maxMessageBytes, bytesWritten, nowStr, _statusLog);
            }

            return processCount;
        }


        // Helper functions for main thread to log their synchronous times. Benchmark non-exception path as most oft used.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartSyncStat() {
            _asyncStats.StartSync();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopSyncStat()
        {
            _asyncStats.StopSync();
        }


        // Refactor these functions, naming needs work
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendMessageDate(string message, long dateTimeTicks, StreamWriter writer)
        {
            _asyncStats.StartDispatch();
            var Message = new AsyncLogMessage(message, null, dateTimeTicks, writer);
            queue.TryEnqueue(Message);
            _asyncStats.StopDispatch();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendMessageDateExcept(string message, Exception e, long dateTimeTicks, StreamWriter writer)
        {
            _asyncStats.StartDispatch();
            var Message = new AsyncLogMessage(message, e, dateTimeTicks, writer);
            queue.TryEnqueue(Message);
            _asyncStats.StopDispatch();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendMessage(string message, StreamWriter writer)
        {
            _asyncStats.StartDispatch();
            var Message = new AsyncLogMessage(message, null, DateTime.UtcNow.Ticks, writer);
            queue.TryEnqueue(Message);
            _asyncStats.StopDispatch();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendMessageException(string message, Exception e, StreamWriter writer)
        {
            _asyncStats.StartDispatch();
            var Message = new AsyncLogMessage(message, e, DateTime.UtcNow.Ticks, writer);
            queue.TryEnqueue(Message);
            _asyncStats.StopDispatch();
        }

        void StartBIST()
        {
            var thread = new Thread(BIST);
            thread.Priority = ThreadPriority.AboveNormal;
            thread.Start();
            thread.Join();
        }

        void StartThread()
        {
            var thread = new Thread(Loop);
            thread.Priority = ThreadPriority.Normal;
            thread.Start();
            _bRunning = true;
        }

        void BIST()
        {
            byte[] randBytes = new byte[4096];

            _bistLogPath = Path.Combine(Mod.ModDir + "/" + BIST_LOG_NAME);
            if (File.Exists(_bistLogPath))
            {
                File.Delete(_bistLogPath);
            }
            var bistLogWriter = new StreamWriter(_bistLogPath, true, new System.Text.UTF8Encoding(true), 65535);

            SendStatusMessage("AsyncLog [BIST] Starting");
            SendStatusMessage("AsyncLog [BIST] Generating Random String");
            RandomNumberGenerator.Create().GetBytes(randBytes);
            var testString = Convert.ToBase64String(randBytes);

            SendStatusMessage("AsyncLog [BIST] Dispatching Message");
            long testDate = DateTime.UtcNow.Ticks;
            SendMessageDate(testString, testDate, bistLogWriter);


            SendStatusMessage("AsyncLog [BIST] Checking File Creation");

            int i = 0;
            while (!File.Exists(_bistLogPath))
            {
                Thread.Sleep(100);
                if (i == 100)
                {
                    SendStatusMessage("AsyncLog [BIST] FAIL CREATE - TIMEOUT");
                }
                i++;
            }

            i = 0;
            SendStatusMessage("AsyncLog [BIST] File Created");
            while (new FileInfo(_bistLogPath).Length == 0)
            {
                Thread.Sleep(100);
                if (i == 100)
                {
                    SendStatusMessage("AsyncLog [BIST] FAIL WRITE - TIMEOUT");
                }
                i++;
            }
            SendStatusMessage("AsyncLog [BIST] File Modified");
            bistLogWriter.Close();
            var reader = new StreamReader(_bistLogPath);
            string readBack = reader.ReadToEnd().Remove(0, 14);

            if (readBack.Length < 4096) { _statusLog.Write("AsyncLog [BIST] FAIL - READBACK LENGTH"); return; }

            var refTime = new DateTime(testDate);

            // Readback will fail if the endline is incorrect. This is intentional to check other platforms
            string checkString = FastFormatDate.ToHHmmssfff_(ref refTime) + testString + Environment.NewLine;
            checkString = checkString.Remove(0, 14);

            if (checkString.Length < 4096) { _statusLog.Write("AsyncLog [BIST] FAIL - CHECKSTR LENGTH"); return; }

            SendStatusMessage("AsyncLog [BIST] Read Back: " + readBack);
            SendStatusMessage("AsyncLog [BIST] Check Str: " + checkString);

            if (Mod.Config.SimulateAsyncBISTFail)
            {
                SendStatusMessage("AsyncLog [BIST] SIMULATED BIST FAILURE");
                SendStatusMessage("AsyncLog [BIST] Reverting to Synchronous Mode");
                Mod.Config.AsyncLogging = false;
                this._bRunning = false;
                return;
            }

            if (readBack == checkString)
            {
                SendStatusMessage("AsyncLog [BIST] SUCCESS");
                reader.Close();
                return;
            }
            SendStatusMessage("AsyncLog [BIST] FAIL - BAD READBACK");
            reader.Close();
            Mod.Config.AsyncLogging = false;
            this._bRunning = false;
            return;
        }

        void Loop()
        {

            var dt = DateTime.UtcNow;
            string now = FastFormatDate.ToHHmmssfff_(ref dt);
            _statusLog.WriteLine($"{now}AsyncLog [RUN] Thread Started");
            _statusLog.WriteLine($"{now}AsyncLog [RUN] Logger Status File: {_statusLogPath}");
            _statusLog.Flush();

            var spinner = new SpinWait();

            _statusLog.WriteLine($"{now}AsyncLog [RUN] Running Worker");
            while (_bRunning)
            {
                if (ProcessMessages() > 0)
                {
                    spinner.Reset();
                }
                spinner.SpinOnce();
            }
            _statusLog.WriteLine($"{now}AsyncLog [RUN] Stopping Worker");
        }
    }
}

using IRBTModUtils.Helper;
using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Security.Cryptography;
using Org.BouncyCastle.Utilities;

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

        AsyncLogStatCollection _statCollection = new AsyncLogStatCollection();

        public MPMCQueue queue = new MPMCQueue(1024*1024);

        AsyncLogMessage[] messages = new AsyncLogMessage[1024*1024];

        // Local Memory
        private const int MEMORY_SIZE = 4 * 1024; // 4 KiB
        private const int MEMORY_PAGES = 256;        // UTF16 -> UTF8 Expansion size
        private const int STAT_WINDOW_COUNT = 1000; // Outputs stats every number of messages

        // Allocate 256 4KiB memory pages, for a total of 1.048576 MB
        public char[] buffer = new char[MEMORY_PAGES * MEMORY_SIZE];

        // Writer
        public StreamWriter writer = null;


        public double processTime = 0;
        public double dispatchTime = 0;
        public double saved = 0;
        public int msgCounter = 0;
        public long msgReal = 0;
        public long burstCount = 0;
        public StreamWriter statusLogWriter = null;
        private DateTime _curDateTime;
        private long _curTicks;
        readonly private int DateLength = 13;

        long bytesWritten = 0;

        public bool _bRunning = false;

        bool clearPrewarm = true;

        readonly string asyncLogName = "irbt_async.log";
        readonly string bistLogName = "irbt_async_bist.log";
        string statusLogPath = "irbt_async.log";
        string bistLogPath = "irbt_async_bist.log";


        void SendStatusMessage(string message)
        {
            var utc = DateTime.UtcNow;
            string now = FastFormatDate.ToHHmmssfff(ref utc);
            statusLogWriter.WriteLine(now + message);
            statusLogWriter.Flush();
        }

        private AsyncLogWorker()
        {
            if (Directory.Exists(Mod.ModDir))
            {
                statusLogPath = Path.Combine(Mod.ModDir + "/" + asyncLogName);
            }

            statusLogWriter = new StreamWriter(statusLogPath);

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

        // Will crash thread if segfault. No exception handling for speed.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ProcessItem(AsyncLogMessage item)
        {
            msgCounter += 1;
            msgReal += 1;

            _statCollection.StartWrite();

            if (writer == null)
            {
                writer = item._writer;
            }


            // Exception log, rarer so concat for now
            if (item._e != null)
            {
                _curDateTime = new DateTime(item._ticks);
                string now = FastFormatDate.ToHHmmssfff(ref _curDateTime);

                if (item._message != null)
                {
                    item._writer.WriteLine(now + item._message);
                
                    bytesWritten += now.Length + item._message.Length;
                }
                item._writer.WriteLine(now + item._e?.Message);
                item._writer.WriteLine(now + item._e?.StackTrace);
                    
                    
                if (item._e?.InnerException != null)
                {
                    item._writer.WriteLine(now + item._e?.InnerException.Message);
                    item._writer.WriteLine(now + item._e?.InnerException.StackTrace);

                    if (item._e.InnerException?.InnerException != null)
                    {
                        item._writer.WriteLine(now + item._e?.InnerException?.InnerException.Message);
                        item._writer.WriteLine(now + item._e?.InnerException?.InnerException.StackTrace);
                    }
                }
            }
            else
            {
                //buffer = System.Buffers.ArrayPool<char>.Shared.Rent(DateLength + item.message.Length + 1);
                if (_curTicks != item._ticks)
                {
                    _curTicks = item._ticks;
                    _curDateTime = new DateTime(_curTicks);
                    FastFormatDate.ToHHmmssfff(ref _curDateTime, ref buffer);
                }

                for (int i = 0; i < item._message.Length; i++)
                {
                    buffer[i + DateLength] = item._message[i];
                }
                buffer[DateLength + item._message.Length] = '\n';
                int outputLength = DateLength + item._message.Length + 1;

                item._writer.Write(buffer, 0, outputLength);
                bytesWritten += outputLength;
            }

            _statCollection.StopWrite();
            // Upon initialization, local writer is null, ensure immediate flush occurs to be responsive                        
            if (writer == null)
            {
                _statCollection.StartFlush();
                writer = item._writer;
                writer.Flush();
                _statCollection.StopFlush();
            }
            // Flush stored writer if new messages come in
            else if (writer != item._writer)
            {
                _statCollection.StartFlush();
                writer.Flush();
                writer = item._writer;
                item._writer.Flush();
                _statCollection.StopFlush();
            }

            if (msgCounter > STAT_WINDOW_COUNT)
            {
                msgCounter = 0;

                string now = FastFormatDate.ToHHmmssfff(ref _curDateTime);
                SendStatusMessage($"AsyncLog [STAT] Thread ID: {System.Environment.CurrentManagedThreadId} Messages: {msgReal} Bytes: {bytesWritten}");
                statusLogWriter.WriteLine(_statCollection.ToString("             "));
                statusLogWriter.Flush();


                if (clearPrewarm)
                {
                    clearPrewarm = false;
                    msgReal = 0;

                    _statCollection.ResetAll();
                    SendStatusMessage("AsyncLog [RUN] Clearing Prewarm");
                    statusLogWriter.Flush();
                }

            }

        }


        
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        long ProcessMessages()
        {

            StreamWriter curWriter = null;

            int tapeCount = 0;
            int flushCount = 0;

            // Quickly dequeue references to thread local memory to free up buffer to reduce contention
            int processCount = DoubleBuffer();

            // Imagine the queue of messages as tapes of writes, as mods often log in blocks
            // We cut the tape for each streamwriter, block encode, and send that to flush
            // 
            // This helps the OS process contiguous writes where possible, reduces cache misses,
            // and reduces HDD write head seek times to a track (in not-fragmented file case)

            long ticks = 0;
            string now = "";
            DateTime curUtc;

            for (int i = 0; i < processCount; i++)
            {
                // On init, make sure to assign
                if (i == 0) { curWriter = messages[i]._writer; }

                if (messages[i]._e == null)
                {
                    // Cut the tape, and write existing contiguous
                    if (messages[i]._writer != curWriter)
                    {
                        curWriter.Flush();
                        flushCount++;
                        curWriter = messages[i]._writer;
                    }
                    long len = messages[i]._message.Length + 14;

                    if (ticks != messages[i]._ticks)
                    {  
                        curUtc = new DateTime(messages[i]._ticks);
                        now = FastFormatDate.ToHHmmssfff(ref curUtc);
                    }
                    curWriter.WriteLine(now + messages[i]._message);
                }

                // Last item, cut the current tape and flush. Decrement count for indexing
                if (i == processCount - 1 ) { curWriter.Flush(); flushCount++; }

            }
            
            
            if (processCount > 0)
            {
                SendStatusMessage($"Processing - Messages: {processCount}, Flushes: {flushCount}");
            }

            return processCount;
        }


        public void StartSyncStat()
        {
            _statCollection.StartSync();

        }

        public void StopSyncStat()
        {
            _statCollection.StopSync();
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendMessageDate(string message, long dateTimeTicks, StreamWriter writer)
        {
            _statCollection.StartDispatch();
            var Message = new AsyncLogMessage(message, null, dateTimeTicks, writer);
            queue.TryEnqueue(Message);
            _statCollection.StopDispatch();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendMessageDateExcept(string message, Exception e, long dateTimeTicks, StreamWriter writer)
        {
            _statCollection.StartDispatch();
            var Message = new AsyncLogMessage(message, e, dateTimeTicks, writer);
            queue.TryEnqueue(Message);
            _statCollection.StopDispatch();
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendMessage(string message, StreamWriter writer)
        {
            _statCollection.StartDispatch();
            var Message = new AsyncLogMessage(message, null, DateTime.UtcNow.Ticks, writer);
            queue.TryEnqueue(Message);
            _statCollection.StopDispatch();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendMessageException(string message, Exception e, StreamWriter writer)
        {
            _statCollection.StartDispatch();
            var Message = new AsyncLogMessage(message, e, DateTime.UtcNow.Ticks, writer);
            queue.TryEnqueue(Message);
            _statCollection.StopDispatch();
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

            bistLogPath = Path.Combine(Mod.ModDir + "/" + bistLogName);
            if (File.Exists(bistLogPath))
            {
                File.Delete(bistLogPath);
            }
            var bistLogWriter = new StreamWriter(bistLogPath, true, new System.Text.UTF8Encoding(false), 65535);
            //var bistLogWriter = File.AppendText(bistLogPath);

            SendStatusMessage("AsyncLog [BIST] Starting");
            SendStatusMessage("AsyncLog [BIST] Generating Random String");
            RandomNumberGenerator.Create().GetBytes(randBytes);
            var testString = Convert.ToBase64String(randBytes);


            SendStatusMessage("AsyncLog [BIST] Dispatching Message");
            long testDate = DateTime.UtcNow.Ticks;
            SendMessageDate(testString, testDate, bistLogWriter);


            SendStatusMessage("AsyncLog [BIST] Checking File Creation");
            while (!File.Exists(bistLogPath))
            {
                Thread.Sleep(1000);
                SendStatusMessage("AsyncLog [BIST] FAIL CREATE - TIMEOUT");
                return;
            }

            SendStatusMessage("AsyncLog [BIST] File Created");
            while (new FileInfo(bistLogPath).Length == 0)
            {
                Thread.Sleep(1000);
                SendStatusMessage("AsyncLog [BIST] FAIL WRITE - TIMEOUT");
                return;
            }
            SendStatusMessage("AsyncLog [BIST] File Modified");
            bistLogWriter.Close();
            var reader = new StreamReader(bistLogPath);
            string readBack = reader.ReadToEnd().Remove(0, 13);

            if (readBack.Length < 4096) { statusLogWriter.Write("AsyncLog [BIST] FAIL - READBACK LENGTH"); return; }

            var refTime = new DateTime(testDate);
            string checkString = FastFormatDate.ToHHmmssfff(ref refTime) + testString + "\n";
            checkString = checkString.Remove(0, 13);
            if (checkString.Length < 4096) { statusLogWriter.Write("AsyncLog [BIST] FAIL - CHECKSTR LENGTH"); return; }

            SendStatusMessage("AsyncLog [BIST] Read Back: " + readBack);
            SendStatusMessage("AsyncLog [BIST] Check Str: " + checkString);

            reader.Close();

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
                return;
            }
            SendStatusMessage("AsyncLog [BIST] FAIL - BAD READBACK");
            Mod.Config.AsyncLogging = false;
            this._bRunning = false;
            return;
        }



        void Loop()
        {

            var dt = DateTime.UtcNow;
            string now = FastFormatDate.ToHHmmssfff(ref dt);
            statusLogWriter.WriteLine($"{now}AsyncLog [RUN] Thread Started");
            statusLogWriter.WriteLine($"{now}AsyncLog [RUN] Logger Status File: {statusLogPath}");
            statusLogWriter.Flush();

            var spinner = new SpinWait();

            statusLogWriter.WriteLine($"{now}AsyncLog [RUN] Running Worker");
            while (_bRunning)
            {
                if (ProcessMessages() > 0)
                {
                    spinner.Reset();
                }
                spinner.SpinOnce();
            }
            statusLogWriter.WriteLine($"{now}AsyncLog [RUN] Stopping Worker");
        }


    }
}

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IRBTModUtils.Logging
{
    /// <summary>
    /// Async Message data to be dispatched. Attempts to reduce queue allocations and marshals managed memory instead.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public class AsyncLogMessage
    {
        // Explicit layout for cache line optimization on x86-64
        [FieldOffset(0)]
        public long _ticks;
        [FieldOffset(8)]
        public StreamWriter _writer;
        [FieldOffset(16)]
        public Exception _e;
        [FieldOffset(24)]
        public AsyncMessageData _message;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncLogMessage(
            string message, 
            Exception e, 
            long ticks, 
            StreamWriter writer)
        {
            _e = e;
            _writer = writer;
            _ticks = ticks;
            _message = new AsyncStandardMessage(message);
            ((AsyncStandardMessage)_message)._message = message;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncLogMessage(
            object message, 
            Exception e, 
            long ticks, 
            StreamWriter writer)
        {
            _e = e;
            _writer = writer;
            _ticks = ticks;
            _message = (AsyncMessageData)message;
        }

    };
    
    /// <summary>
    /// See AsyncStandardMessage() for example, inherit to use. 
    /// Enables asynchronous formatting to offload main, ensure members are not changed between dispatch and asynchronous processing.
    /// </summary>
    abstract public class AsyncMessageData()
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        abstract public string Format();
    }


    /// <summary>
    /// Default message handler used by Async Log. No formatting performed during asynchronous write.
    /// </summary>
    public class AsyncStandardMessage() : AsyncMessageData
    {
        public string _message = "";
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncStandardMessage(string message) : this()
        {
            this._message = message;
        }   

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        override public string Format()
        {
            return _message;
        }
    }

 
    /// <summary>
    /// Similar to AsyncLogMessage, but pass in derived class through AsyncMessageData instead.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public class AsyncStructuredMessage
    {
        [FieldOffset(0)]
        public long _ticks;
        [FieldOffset(8)]
        public StreamWriter _writer;
        [FieldOffset(16)]
        public Exception _e;
        [FieldOffset(24)]
        public AsyncMessageData _message;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncStructuredMessage(
            AsyncMessageData message, 
            Exception e, 
            long ticks, 
            StreamWriter writer)
        {
            _e = e;
            _writer = writer;
            _ticks = ticks;
            _message = message;
        }
    };

}

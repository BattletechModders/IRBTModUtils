using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IRBTModUtils.Logging
{
    /// <summary>
    /// Async Message data to be dispatched. Attempts to reduce queue allocations and marshals managed memory instead
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public class AsyncLogMessage
    {
        [FieldOffset(0)]
        public long _ticks;
        [FieldOffset(8)]
        public StreamWriter _writer;
        [FieldOffset(16)]
        public Exception _e;
        [FieldOffset(24)]
        public string _message;

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
            _message = message;
        }
    };
}

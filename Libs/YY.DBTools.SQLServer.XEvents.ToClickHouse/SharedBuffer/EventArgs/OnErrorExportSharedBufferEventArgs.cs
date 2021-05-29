using System;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer.EventArgs
{
    public sealed class OnErrorExportSharedBufferEventArgs : System.EventArgs
    {
        public OnErrorExportSharedBufferEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }    
}

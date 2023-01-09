using System.Collections.Generic;

namespace YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer.EventArgs
{
    public sealed class OnSendLogFromSharedBufferEventArgs : System.EventArgs
    {
        public IReadOnlyDictionary<LogBufferItemKey, LogBufferItem> DataFromBuffer { get; }

        public OnSendLogFromSharedBufferEventArgs(
            IReadOnlyDictionary<LogBufferItemKey, LogBufferItem> dataFromBuffer)
        {
            DataFromBuffer = dataFromBuffer;
        }
    }    
}

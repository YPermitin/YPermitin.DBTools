using YY.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer.Exceptions;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer.EventArgs
{
    public sealed class OnErrorExportSharedBufferEventArgs : System.EventArgs
    {
        public OnErrorExportSharedBufferEventArgs(ExportSharedBufferException exception)
        {
            Exception = exception;
        }

        public ExportSharedBufferException Exception { get; }
    }    
}

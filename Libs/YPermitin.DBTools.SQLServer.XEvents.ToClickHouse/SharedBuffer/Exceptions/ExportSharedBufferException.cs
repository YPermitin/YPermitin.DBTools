using System;

namespace YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer.Exceptions
{
    public class ExportSharedBufferException : Exception
    {
        public XEventsExportSettings.LogSourceSettings Settings { get; }

        public ExportSharedBufferException(string message, Exception innerException, XEventsExportSettings.LogSourceSettings settings)
            : base(message, innerException)
        {
            Settings = settings;
        }
    }
}

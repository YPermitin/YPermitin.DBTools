using System;

namespace YPermitin.DBTools.SQLServer.XEvents.ToClickHouse
{
    public class OnErrorExportDataEventArgs
    {
        public Exception Exception { get; set; }
    }
}

using System;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse
{
    public class OnErrorExportDataEventArgs
    {
        public Exception Exception { get; set; }
    }
}

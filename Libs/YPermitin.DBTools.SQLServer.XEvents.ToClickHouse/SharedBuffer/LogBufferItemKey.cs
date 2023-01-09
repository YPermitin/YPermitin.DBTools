using System;

namespace YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer
{
    public class LogBufferItemKey
    {
        public XEventsExportSettings.LogSourceSettings Settings { get; }
        public DateTime Period { get; }
        public string LogFile { get; }

        public LogBufferItemKey(
            XEventsExportSettings.LogSourceSettings setting,
            DateTime period,
            string logFile)
        {
            Settings = setting;
            Period = period;
            LogFile = logFile;
        }
    }
}

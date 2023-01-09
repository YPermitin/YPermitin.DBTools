using Microsoft.Extensions.Configuration;
using YPermitin.DBTools.SQLServer.XEvents.ToClickHouse;

namespace YPermitin.DBTools.SQLServer.ExtendedEventsToClickHouse
{
    public class XEventsExportApplicationSettings : XEventsExportSettings
    {
        public static XEventsExportApplicationSettings CreateSettings(IConfiguration configuration, bool AllowInteractiveActions, string LogDirectoryPath)
        {
            return new XEventsExportApplicationSettings(configuration, AllowInteractiveActions, LogDirectoryPath);
        }

        public bool AllowInteractiveActions { get; }

        public string LogDirectoryPath { get; }

        public XEventsExportApplicationSettings(IConfiguration configuration, bool AllowInteractiveActions, string LogDirectoryPath) : base(configuration)
        {
            this.AllowInteractiveActions = AllowInteractiveActions;
            this.LogDirectoryPath = LogDirectoryPath;
        }
    }
}
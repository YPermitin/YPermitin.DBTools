using System.IO;
using YY.DBTools.Core;

namespace YY.DBTools.SQLServer.ExtendedEventsToClickHouse
{
    public class XEventsExportApplicationSettings : XEventsExportSettings
    {
        public static XEventsExportApplicationSettings CreateSettings(string configFile, bool AllowInteractiveActions, string LogDirectoryPath)
        {
            configFile ??= "appsettings.json";

            FileInfo configFileInfo = new FileInfo(configFile);
            return configFileInfo.Exists ? new XEventsExportApplicationSettings(configFile, AllowInteractiveActions, LogDirectoryPath) : null;
        }

        private readonly bool _allowInteractiveActions;
        private readonly string _logDirectoryPath;

        public bool AllowInteractiveActions => _allowInteractiveActions;
        public string LogDirectoryPath => _logDirectoryPath;

        public XEventsExportApplicationSettings(string configFile, bool AllowInteractiveActions, string LogDirectoryPath) :base(configFile)
        {
            _allowInteractiveActions = AllowInteractiveActions;
            _logDirectoryPath = LogDirectoryPath;
        }
    }
}

using Microsoft.Extensions.Configuration;
using System.IO;
using YY.DBTools.Core.Helpers;

namespace YY.DBTools.Core
{
    public class XEventsExportSettings
    {
        public static XEventsExportSettings CreateSettings(string configFile)
        {
            configFile ??= "appsettings.json";

            FileInfo configFileInfo = new FileInfo(configFile);
            return configFileInfo.Exists ? new XEventsExportSettings(configFile) : null;
        }

        public XEventsExportSettings(string configFile)
        {
            FileInfo configFileInfo = new FileInfo(configFile);
            if (configFileInfo.Exists)
            {
                IConfiguration configuration = new ConfigurationBuilder()
                    .AddJsonFile(configFile, optional: true, reloadOnChange: true)
                    .Build();

                string StorageTypeValue = configuration.GetValue("StorageType", string.Empty);
                StorageType = EnumHelper.GetEnumValueByName<StorageType>(StorageTypeValue);

                IConfigurationSection XEventLogSection = configuration.GetSection("XEvents");
                XEventsPath = XEventLogSection.GetValue("SourcePath", string.Empty);
                DelayMs = XEventLogSection.GetValue("DelayMs", 60000);
                UseWatchMode = XEventLogSection.GetValue("UseWatchMode", false);
                Portion = XEventLogSection.GetValue("Portion", 10000);

                ConnectionString = configuration.GetConnectionString("XEventsDatabase");
            }
            else
                throw new FileNotFoundException("Config file not found.", configFile);
        }

        public readonly string XEventsPath;
        public readonly int DelayMs;
        public readonly bool UseWatchMode;
        public readonly int Portion;
        public readonly string ConnectionString;
        public readonly StorageType StorageType;
    }
}

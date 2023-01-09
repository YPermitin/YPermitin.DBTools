using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using YPermitin.DBTools.Core;

namespace YPermitin.DBTools.SQLServer.XEvents.ToClickHouse
{
    public class XEventsExportSettings
    {
        public static XEventsExportSettings Create(string configFile)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile(configFile, optional: true, reloadOnChange: true)
                .Build();

            return Create(Configuration);
        }
        public static XEventsExportSettings Create(IConfiguration configuration)
        {
            return new XEventsExportSettings(configuration);
        }

        public StorageType StorageType { get; }
        public string ConnectionString { get; }
        public WatchModeSettings WatchMode { get; }
        public ExportSettings Export { get; }
        public IReadOnlyList<LogSourceSettings> LogSources { get; }

        public XEventsExportSettings(IConfiguration configuration)
        {
            string storageTypeAsString = configuration.GetValue("StorageType", string.Empty);
            StorageType = Enum.Parse<StorageType>(storageTypeAsString);

            ConnectionString = configuration.GetConnectionString("XEventsDatabase");

            var exportParametersSection = configuration.GetSection("Export");
            var bufferSection = exportParametersSection.GetSection("Buffer");
            var maxItemCountSize = bufferSection.GetValue("MaxItemCountSize", 10000);
            var maxSaveDurationMs = bufferSection.GetValue("MaxSaveDurationMs", 60000);
            var maxBufferSizeItemsCount = bufferSection.GetValue("MaxBufferSizeItemsCount", 100000);
            Export = new ExportSettings(
                new ExportSettings.BufferSettings(maxItemCountSize, maxSaveDurationMs, maxBufferSizeItemsCount));

            var watchModeSection = configuration.GetSection("WatchMode");
            var useWatchMode = watchModeSection.GetValue("Use", false);
            var periodicityWatchMode = watchModeSection.GetValue("Periodicity", 60000);
            WatchMode = new WatchModeSettings(useWatchMode, periodicityWatchMode);

            var logSourcesSection = configuration.GetSection("LogSources");
            var logSources = logSourcesSection.GetChildren();
            List<LogSourceSettings> logSourceSettings = new List<LogSourceSettings>();
            foreach (var logSource in logSources)
            {
                var logSourceName = logSource.GetValue("Name", string.Empty);
                var logSourceDescription = logSource.GetValue("Description", string.Empty);
                string sourcePath = logSource.GetValue("SourcePath", string.Empty);
                int portion = logSource.GetValue("Portion", 10000);
                string timeZoneName = logSource.GetValue("TimeZone", string.Empty);
                TimeZoneInfo timeZone;
                if (string.IsNullOrEmpty(timeZoneName))
                    timeZone = TimeZoneInfo.Local;
                else
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);

                logSourceSettings.Add(new LogSourceSettings(
                    logSourceName,
                    logSourceDescription,
                    sourcePath,
                    portion,
                    timeZone));
            }
            LogSources = logSourceSettings;
        }

        public class WatchModeSettings
        {
            public bool Use { get; }
            public int Periodicity { get; }

            public WatchModeSettings(bool use, int periodicity)
            {
                Use = use;
                Periodicity = periodicity;
            }
        }

        public class LogSourceSettings
        {
            private ExtendedEventsLogBase _xEventsLog;

            public string Name { get; }
            public string Description { get; }
            public ExtendedEventsLogBase XEventsLog
            {
                get
                {
                    if (_xEventsLog == null)
                    {
                        _xEventsLog = new ExtendedEventsLogBase()
                        {
                            Name = Name,
                            Description = Description
                        };
                    }

                    return _xEventsLog;
                }
            }
            public string SourcePath { get; }
            public int Portion { get; }
            public TimeZoneInfo TimeZone { get; }
            public object LockObject { get; }

            public LogSourceSettings(string name, string description, string sourcePath, int portion, TimeZoneInfo timeZone)
            {
                Name = name;
                Description = description;
                Portion = portion;
                SourcePath = sourcePath;
                TimeZone = timeZone;
                LockObject = new object();
            }
        }

        public class ExportSettings
        {
            public BufferSettings Buffer { get; }

            public ExportSettings(BufferSettings bufferSettings)
            {
                Buffer = bufferSettings;
            }

            public class BufferSettings
            {
                public long MaxItemCountSize { get; }
                public long MaxSaveDurationMs { get; }
                public long MaxBufferSizeItemsCount { get; }

                public BufferSettings(long maxItemCountSize, long maxSaveDurationMs, long maxBufferSizeItemsCount)
                {
                    MaxItemCountSize = maxItemCountSize;
                    MaxSaveDurationMs = maxSaveDurationMs;
                    MaxBufferSizeItemsCount = maxBufferSizeItemsCount;
                }
            }
        }
    }
}

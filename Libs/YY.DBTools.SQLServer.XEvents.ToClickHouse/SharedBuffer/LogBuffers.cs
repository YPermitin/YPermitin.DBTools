using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YY.DBTools.SQLServer.XEvents.ToClickHouse.Models;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer
{
    public class LogBuffers
    {
        public readonly ConcurrentDictionary<LogBufferItemKey, LogBufferItem> LogBuffer;
        
        public ConcurrentDictionary<XEventsExportSettings.LogSourceSettings, ConcurrentDictionary<string, ExtendedEventsPosition>> LogPositions { get; }
        
        public LogBuffers()
        {
            LogBuffer = new ConcurrentDictionary<LogBufferItemKey, LogBufferItem>();
            LogPositions = new ConcurrentDictionary<XEventsExportSettings.LogSourceSettings, ConcurrentDictionary<string, ExtendedEventsPosition>>();
        }

        /// <summary>
        /// Общее количество записей логов в буфере
        /// </summary>
        public long TotalItemsCount
        {
            get
            {
                return LogBuffer
                    .Select(e => e.Value.ItemsCount)
                    .Sum();
            }
        }
        
        public async Task SaveLogsAndPosition(
            XEventsExportSettings.LogSourceSettings logSettings,
            ExtendedEventsPosition position,
            IList<XEventData> rowsData
            )
        {
            if(position == null)
                return;

            lock (logSettings.LockObject)
            {
                var logFileInfo = new FileInfo(position.CurrentFileData);
                SaveLogs(logSettings, position, rowsData, logFileInfo);
            }
        }
        
        private void SaveLogs(
            XEventsExportSettings.LogSourceSettings logSettings,
            ExtendedEventsPosition position,
            IList<XEventData> rowsData,
            FileInfo logFileInfo)
        {
            var newBufferItem = new LogBufferItem();
            newBufferItem.LogPosition = position;
            foreach (var rowData in rowsData)
            {
                newBufferItem.LogRows.TryAdd(new EventKey()
                {
                    Id = Guid.NewGuid(),
                    File = logFileInfo
                }, rowData);
            }

            LogBuffer.TryAdd(new LogBufferItemKey(logSettings, DateTime.Now, logFileInfo.FullName), 
                newBufferItem);

            LogPositions.AddOrUpdate(logSettings,
                (settings) =>
                {
                    var newPositions = new ConcurrentDictionary<string, ExtendedEventsPosition>();
                    newPositions.AddOrUpdate(logFileInfo.Name,
                            (fileName) => position, 
                            (fileName, oldPosition) => position);
                    return newPositions;
                },
                (settings, logBufferItem) =>
                {
                    logBufferItem.AddOrUpdate(logFileInfo.Name,
                            (fileName) => position,
                            (fileName, oldPosition) => position);
                    return logBufferItem;
                });
        }
        
        public async Task<ExtendedEventsPosition> GetLastPosition(
            XEventsExportSettings.LogSourceSettings logSettings,
            string directoryName)
        {
            ExtendedEventsPosition position = null;
            if (LogPositions.TryGetValue(logSettings, out ConcurrentDictionary<string, ExtendedEventsPosition> settingPositions))
            {
                settingPositions.TryGetValue(directoryName, out position);
            }

            return position;
        }
    }
}

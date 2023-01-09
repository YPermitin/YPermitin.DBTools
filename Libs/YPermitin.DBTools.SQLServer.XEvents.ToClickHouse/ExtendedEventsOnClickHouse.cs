using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using YPermitin.DBTools.Core.Helpers;
using YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.Models;

namespace YPermitin.DBTools.SQLServer.XEvents.ToClickHouse
{
    public class ExtendedEventsOnClickHouse : IXEventsOnTarget
    {
        private const int _defaultPortion = 1000;
        private readonly int _portion;
        private readonly string _connectionString;
        private ExtendedEventsLogBase _xEventsLog;

        public ExtendedEventsOnClickHouse() : this(null, _defaultPortion)
        {

        }
        public ExtendedEventsOnClickHouse(int portion) : this(null, portion)
        {
            _portion = portion;
        }
        public ExtendedEventsOnClickHouse(string connectionString, int portion)
        {
            _portion = portion;

            if (connectionString == null)
            {
                IConfiguration Configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();
                _connectionString = Configuration.GetConnectionString("XEventsDatabase");
            }

            _connectionString = connectionString;
        }

        public void SetLogInformation(ExtendedEventsLogBase xEventsLog)
        {
            _xEventsLog = xEventsLog;
        }

        public async Task<bool> LogFileChanged(FileInfo logFileInfo)
        {
            ExtendedEventsPosition position = await GetLastPosition(logFileInfo.Name);

            if (position == null)
                return true;

            if (position.FinishReadFile)
            {
                if (position.LogFileCreateDate.Truncate(TimeSpan.FromSeconds(1)) != logFileInfo.CreationTimeUtc.Truncate(TimeSpan.FromSeconds(1))
                    || position.LogFileModificationDate.Truncate(TimeSpan.FromSeconds(1)) != logFileInfo.LastWriteTimeUtc.Truncate(TimeSpan.FromSeconds(1)))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<ExtendedEventsPosition> GetLastPosition(string fileName)
        {
            using (var context = new ClickHouseContext(_connectionString))
            {
                return await context.GetLogFilePosition(_xEventsLog, fileName);
            }
        }

        public int GetPortionSize()
        {
            return _portion;
        }

        public async Task Save(XEventData eventData)
        {
            List<XEventData> rowsData = new List<XEventData>
            {
                eventData
            };
            await Save(rowsData);
        }

        public async Task Save(List<XEventData> eventsData)
        {
            using (var context = new ClickHouseContext(_connectionString))
            {
                await context.SaveRowsData(_xEventsLog, eventsData);
            }
        }
        
        public async Task SaveLogPosition(ExtendedEventsPosition position)
        {
            using (var context = new ClickHouseContext(_connectionString))
            {
                await context.SaveLogPosition(_xEventsLog, position);
            }
        }
    }
}

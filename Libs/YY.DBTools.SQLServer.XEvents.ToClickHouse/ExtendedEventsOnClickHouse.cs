using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using YY.DBTools.SQLServer.XEvents.ToClickHouse.Models;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse
{
    public class ExtendedEventsOnClickHouse : IXEventsOnTarget
    {
        private const int _defaultPortion = 1000;
        private readonly int _portion;
        private readonly Dictionary<string, DateTime> _maxPeriodsByFiles = new Dictionary<string, DateTime>();
        private readonly string _connectionString;
        private int _stepsToClearLogFiles = 1000;
        private int _currentStepToClearLogFiles;

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

        public ExtendedEventsPosition GetLastPosition()
        {
            throw new System.NotImplementedException();
        }

        public int GetPortionSize()
        {
            return _portion;
        }

        public async Task Save(XEventData eventData)
        {
            IList<XEventData> rowsData = new List<XEventData>
            {
                eventData
            };
            await Save(rowsData);
        }

        public async Task Save(IList<XEventData> eventsData)
        {
            using (var context = new ClickHouseContext(_connectionString))
            {
                List<XEventData> newEntities = new List<XEventData>();
                foreach (var itemRow in eventsData)
                {
                    if (itemRow == null)
                        continue;

                    if (!_maxPeriodsByFiles.TryGetValue(itemRow.FileName, out var maxPeriod))
                    {
                        maxPeriod = await context.GetRowsDataMaxPeriod(itemRow.FileName);
                        _maxPeriodsByFiles.Add(itemRow.FileName, maxPeriod);
                    }

                    if (maxPeriod != DateTime.MinValue)
                    {
                        if (itemRow.Timestamp.DateTime < maxPeriod)
                            continue;

                        if (itemRow.Timestamp.DateTime == maxPeriod)
                            if (await context.RowDataExistOnDatabase(itemRow.FileName, itemRow))
                                continue;
                    }

                    newEntities.Add(itemRow);
                }
                await context.SaveRowsData(newEntities);
            }
        }

        public async Task<bool> LogFileLoaded(string fileName)
        {
            using (var context = new ClickHouseContext(_connectionString))
            {
                return await context.LogFileLoaded(fileName);
            }
        }

        public async Task SaveLogPosition(FileInfo logFileInfo, ExtendedEventsPosition position, bool finishReadFile)
        {
            using (var context = new ClickHouseContext(_connectionString))
            {
                if(await LogFileLoaded(logFileInfo.FullName))
                    return;;

                await context.SaveLogPosition(logFileInfo, position, finishReadFile);
                if (_currentStepToClearLogFiles == 0 || _currentStepToClearLogFiles >= _stepsToClearLogFiles)
                {
                    await context.RemoveArchiveLogFileRecords(logFileInfo.Name);
                    _currentStepToClearLogFiles = 0;
                }
                _currentStepToClearLogFiles += 1;
            }
        }
    }
}

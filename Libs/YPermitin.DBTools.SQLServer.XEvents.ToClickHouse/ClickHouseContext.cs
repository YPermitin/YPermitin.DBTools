using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using ClickHouse.Client.Copy;
using YPermitin.DBTools.Core.Helpers;
using YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.Helpers;
using YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.Models;
using YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer;

namespace YPermitin.DBTools.SQLServer.XEvents.ToClickHouse
{
    public class ClickHouseContext : IDisposable
    {
        #region Private Members

        private ClickHouseConnection _connection;

        #endregion

        #region Constructors

        public ClickHouseContext(string connectionSettings)
        {
            ClickHouseHelpers.CreateDatabaseIfNotExist(connectionSettings);

            _connection = new ClickHouseConnection(connectionSettings);
            _connection.Open();

            var cmdDDL = _connection.CreateCommand();

            cmdDDL.CommandText = Resources.Query_CreateTable_XEventData;
            cmdDDL.ExecuteNonQuery();

            cmdDDL.CommandText = Resources.Query_CreateTable_LogFiles;
            cmdDDL.ExecuteNonQuery();
        }

        #endregion

        #region Public Methods

        #region RowsData

        public async Task SaveRowsData(ExtendedEventsLogBase xEventsLog, List<XEventData> xEventsData)
        {
            IDictionary<string, List<XEventData>> xEventsDataToInsert = xEventsData
                .GroupBy(g => g.FileName)
                .ToDictionary(k => k.Key, v => v.ToList());

            await SaveRowsData(xEventsLog, xEventsDataToInsert);
        }

        public async Task SaveRowsData(ExtendedEventsLogBase xEventsLog,
            IDictionary<string, List<XEventData>> xEventsData,
            Dictionary<string, LastRowsInfoByLogFile> maxPeriodByFiles = null)
        {
            if (maxPeriodByFiles == null) maxPeriodByFiles = new Dictionary<string, LastRowsInfoByLogFile>();

            List<object[]> rowsForInsert = new List<object[]>();
            foreach (var eventInfo in xEventsData)
            {
                FileInfo logFileInfo = new FileInfo(eventInfo.Key);
                foreach (var eventItem in eventInfo.Value)
                {
                    DateTime periodServer = eventItem.Timestamp.LocalDateTime;
                    DateTime periodUtc = TimeZoneInfo.ConvertTimeToUtc(periodServer, TimeZoneInfo.Local);
                    DateTime periodLocal = periodServer;

                    if (!maxPeriodByFiles.TryGetValue(logFileInfo.Name, out LastRowsInfoByLogFile lastInfo))
                    {
                        if (logFileInfo.Directory != null)
                        {
                            GetRowsDataMaxPeriodAndId(
                                xEventsLog,
                                logFileInfo.Name,
                                periodUtc,
                                out var maxPeriod,
                                out var maxId
                            );
                            lastInfo = new LastRowsInfoByLogFile(maxPeriod, maxId);
                            maxPeriodByFiles.Add(logFileInfo.Name, lastInfo);
                        }
                    }

                    bool existByPeriod = lastInfo.MaxPeriod > ClickHouseHelpers.MinDateTimeValue &&
                                         periodUtc.Truncate(TimeSpan.FromSeconds(1)) <= lastInfo.MaxPeriod;
                    bool existById = lastInfo.MaxId > 0 &&
                                     eventItem.Id <= lastInfo.MaxId;
                    if (existByPeriod && existById)
                        continue;

                    if (logFileInfo.Directory != null)
                        rowsForInsert.Add(new object[]
                        {
                            xEventsLog.Name,
                            logFileInfo.Name,
                            eventItem.EventNumber,
                            periodUtc,
                            periodLocal,
                            eventItem.EventName,
                            eventItem.UUID.ToString(),
                            eventItem.Username ?? string.Empty,
                            eventItem.UsernameNT ?? string.Empty,
                            eventItem.UsernameSessionNT ?? string.Empty,
                            eventItem.SessionId ?? 0,
                            eventItem.PlanHandle ?? string.Empty,
                            eventItem.IsSystem == null ? 0 : ((bool)eventItem.IsSystem ? 1 : 0),
                            eventItem.ExecutionPlanGuid?.ToString() ?? string.Empty,
                            eventItem.DatabaseName ?? string.Empty,
                            eventItem.DatabaseId ?? 0,
                            eventItem.NumaNodeId ?? 0,
                            eventItem.CpuId ?? 0,
                            eventItem.ProcessId ?? 0,
                            eventItem.SQLText ?? string.Empty,
                            eventItem.SQLTextHash ?? string.Empty,
                            eventItem.ClientAppName ?? string.Empty,
                            eventItem.ClientHostname ?? string.Empty,
                            eventItem.ClientId ?? 0,
                            eventItem.QueryHash ?? string.Empty,
                            eventItem.ServerInstanceName ?? string.Empty,
                            eventItem.ServerPrincipalName ?? string.Empty,
                            eventItem.ServerPrincipalId ?? 0,
                            eventItem.CpuTime ?? 0,
                            eventItem.Duration ?? 0,
                            eventItem.PhysicalReads ?? 0,
                            eventItem.LogicalReads ?? 0,
                            eventItem.Writes ?? 0,
                            eventItem.RowCount ?? 0,
                            eventItem.GetActionsAsJSON(),
                            eventItem.GetFieldsAsJSON()
                        });
                }
            }

            if (rowsForInsert.Count == 0)
                return;

            using (ClickHouseBulkCopy bulkCopyInterface = new ClickHouseBulkCopy(_connection)
            {
                DestinationTableName = "XEventData",
                BatchSize = 100000,
                MaxDegreeOfParallelism = 4
            })
            {
                await bulkCopyInterface.WriteToServerAsync(rowsForInsert);
                rowsForInsert.Clear();
            }
        }

        public async Task SaveRowsData(Dictionary<LogBufferItemKey, LogBufferItem> sourceDataFromBuffer)
        {
            List<object[]> rowsForInsert = new List<object[]>();
            List<object[]> positionsForInsert = new List<object[]>();
            Dictionary<string, LastRowsInfoByLogFile> maxPeriodByDirectories = new Dictionary<string, LastRowsInfoByLogFile>();

            var dataFromBuffer = sourceDataFromBuffer
                .OrderBy(i => i.Key.Period)
                .ThenBy(i => i.Value.LogPosition.EventNumber)
                .ToList();

            long itemNumber = 0;
            foreach (var dataItem in dataFromBuffer)
            {
                itemNumber++;
                FileInfo logFileInfo = new FileInfo(dataItem.Key.LogFile);

                DateTime eventPeriodUtc;
                if (dataItem.Value.LogPosition.EventPeriod != null)
                {
                    DateTime periodServer = dataItem.Value.LogPosition.EventPeriod.Value.LocalDateTime;
                    DateTime periodLocal = TimeZoneInfo.ConvertTime(periodServer, TimeZoneInfo.Local, dataItem.Key.Settings.TimeZone);
                    eventPeriodUtc = TimeZoneInfo.ConvertTimeToUtc(periodLocal, dataItem.Key.Settings.TimeZone);
                }
                else
                    eventPeriodUtc = DateTime.MinValue;

                positionsForInsert.Add(new object[]
                {
                    dataItem.Key.Settings.XEventsLog.Name,
                    DateTime.UtcNow.Ticks + itemNumber,
                    logFileInfo.Name,
                    DateTime.UtcNow,
                    logFileInfo.CreationTimeUtc,
                    logFileInfo.LastWriteTimeUtc,
                    dataItem.Value.LogPosition.EventNumber,
                    dataItem.Value.LogPosition.EventUUID,
                    eventPeriodUtc,
                    dataItem.Value.LogPosition.FinishReadFile
                });

                foreach (var rowData in dataItem.Value.LogRows)
                {
                    DateTime periodServer = rowData.Value.Timestamp.LocalDateTime;
                    DateTime periodLocal = TimeZoneInfo.ConvertTime(periodServer, TimeZoneInfo.Local, dataItem.Key.Settings.TimeZone);
                    DateTime periodUtc = TimeZoneInfo.ConvertTimeToUtc(periodLocal, dataItem.Key.Settings.TimeZone);

                    if (!maxPeriodByDirectories.TryGetValue(logFileInfo.FullName, out LastRowsInfoByLogFile lastInfo))
                    {
                        if (logFileInfo.Directory != null)
                        {
                            GetRowsDataMaxPeriodAndId(
                                dataItem.Key.Settings.XEventsLog,
                                logFileInfo.Name,
                                periodUtc,
                                out var maxPeriod,
                                out var maxId
                            );
                            lastInfo = new LastRowsInfoByLogFile(maxPeriod, maxId);
                            maxPeriodByDirectories.Add(logFileInfo.FullName, lastInfo);
                        }
                    }

                    bool existByPeriod = lastInfo.MaxPeriod > ClickHouseHelpers.MinDateTimeValue &&
                                         periodUtc.Truncate(TimeSpan.FromSeconds(1)) <= lastInfo.MaxPeriod;
                    bool existById = lastInfo.MaxId > 0 &&
                                     rowData.Value.Id <= lastInfo.MaxId;
                    if (existByPeriod && existById)
                        continue;

                    var eventItem = rowData.Value;
                    rowsForInsert.Add(new object[]
                        {
                            dataItem.Key.Settings.XEventsLog.Name,
                            logFileInfo.Name,
                            eventItem.EventNumber,
                            periodUtc,
                            periodLocal,
                            eventItem.EventName,
                            eventItem.UUID.ToString(),
                            eventItem.Username ?? string.Empty,
                            eventItem.UsernameNT ?? string.Empty,
                            eventItem.UsernameSessionNT ?? string.Empty,
                            eventItem.SessionId ?? 0,
                            eventItem.PlanHandle ?? string.Empty,
                            eventItem.IsSystem == null ? 0 : ((bool)eventItem.IsSystem ? 1 : 0),
                            eventItem.ExecutionPlanGuid?.ToString() ?? string.Empty,
                            eventItem.DatabaseName ?? string.Empty,
                            eventItem.DatabaseId ?? 0,
                            eventItem.NumaNodeId ?? 0,
                            eventItem.CpuId ?? 0,
                            eventItem.ProcessId ?? 0,
                            eventItem.SQLText ?? string.Empty,
                            eventItem.SQLTextHash ?? string.Empty,
                            eventItem.ClientAppName ?? string.Empty,
                            eventItem.ClientHostname ?? string.Empty,
                            eventItem.ClientId ?? 0,
                            eventItem.QueryHash ?? string.Empty,
                            eventItem.ServerInstanceName ?? string.Empty,
                            eventItem.ServerPrincipalName ?? string.Empty,
                            eventItem.ServerPrincipalId ?? 0,
                            eventItem.CpuTime ?? 0,
                            eventItem.Duration ?? 0,
                            eventItem.PhysicalReads ?? 0,
                            eventItem.LogicalReads ?? 0,
                            eventItem.Writes ?? 0,
                            eventItem.RowCount ?? 0,
                            eventItem.GetActionsAsJSON(),
                            eventItem.GetFieldsAsJSON()
                        });
                }
            }

            if (rowsForInsert.Count > 0)
            {
                using (ClickHouseBulkCopy bulkCopyInterface = new ClickHouseBulkCopy(_connection)
                {
                    DestinationTableName = "XEventData",
                    BatchSize = 100000,
                    MaxDegreeOfParallelism = 4
                })
                {
                    await bulkCopyInterface.WriteToServerAsync(rowsForInsert);
                    rowsForInsert.Clear();
                }
            }

            if (positionsForInsert.Count > 0)
            {
                using (ClickHouseBulkCopy bulkCopyInterface = new ClickHouseBulkCopy(_connection)
                {
                    DestinationTableName = "LogFiles",
                    BatchSize = 100000
                })
                {
                    await bulkCopyInterface.WriteToServerAsync(positionsForInsert);
                }
            }
        }
  
        public async Task<DateTime> GetRowsDataMaxPeriod(ExtendedEventsLogBase xEventsLog, string FileName)
        {
            DateTime output = DateTime.MinValue;

            using (var command = _connection.CreateCommand())
            {
                command.CommandText =
                    @"SELECT
                        MAX(Period) AS MaxPeriod
                    FROM XEventData AS RD
                    WHERE ExtendedEventsLog = {ExtendedEventsLog:String}
                        AND FileName = {FileName:String} ";
                command.Parameters.Add(new ClickHouseDbParameter
                {
                    ParameterName = "ExtendedEventsLog",
                    Value = xEventsLog.Name
                });
                command.Parameters.Add(new ClickHouseDbParameter
                {
                    ParameterName = "FileName",
                    Value = FileName
                });
                using (var cmdReader = await command.ExecuteReaderAsync())
                {
                    if (await cmdReader.ReadAsync())
                        output = cmdReader.GetDateTime(0);
                }
            }

            return output;
        }

        #endregion

        #region LogFiles

        public async Task<ExtendedEventsPosition> GetLogFilePosition(ExtendedEventsLogBase xEventsLog, string fileName)
        {
            var cmdGetLastLogFileInfo = _connection.CreateCommand();
            cmdGetLastLogFileInfo.CommandText =
                @"SELECT	                
                    FileName,
	                LastEventNumber,
	                LastEventUUID,
	                LastEventPeriod,
                    FinishReadFile,
                    FileCreateDate,
                    FileModificationDate
                FROM LogFiles AS LF
                WHERE ExtendedEventsLog = {ExtendedEventsLog:String}
                    AND FileName = {FileName:String}
                    AND Id IN (
                        SELECT
                            MAX(Id) LastId
                        FROM LogFiles AS LF_LAST
                        WHERE LF_LAST.ExtendedEventsLog = {ExtendedEventsLog:String}
                            AND LF_LAST.FileName = {FileName:String}
                    )";
            cmdGetLastLogFileInfo.AddParameterToCommand("ExtendedEventsLog", DbType.AnsiString, xEventsLog.Name);
            cmdGetLastLogFileInfo.AddParameterToCommand("FileName", DbType.AnsiString, fileName);

            ExtendedEventsPosition output = null;
            await using (var cmdReader = await cmdGetLastLogFileInfo.ExecuteReaderAsync())
            {
                if (await cmdReader.ReadAsync())
                {
                    bool finishReadFile = cmdReader.GetBoolean(4);
                    output = new ExtendedEventsPosition(
                        cmdReader.GetInt64(1),
                        cmdReader.GetString(0),
                        cmdReader.GetString(2),
                        cmdReader.GetDateTime(3),
                        finishReadFile,
                        cmdReader.GetDateTime(5),
                        cmdReader.GetDateTime(6));
                }
            }

            return output;
        }

        public async Task SaveLogPosition(ExtendedEventsLogBase xEventsLog, ExtendedEventsPosition position)
        {
            await SaveLogPositions(xEventsLog, new List<ExtendedEventsPosition>()
            {
                position
            });
        }

        public async Task SaveLogPositions(ExtendedEventsLogBase xEventsLog, List<ExtendedEventsPosition> positions)
        {
            List<object[]> positionsForInsert = new List<object[]>();

            foreach (var positionItem in positions)
            {
                FileInfo logFileInfo = new FileInfo(positionItem.CurrentFileData);
                long itemNumber = positions.IndexOf(positionItem) + 1;

                positionsForInsert.Add(new object[]
                {
                    xEventsLog.Name,
                    DateTime.UtcNow.Ticks + itemNumber,
                    logFileInfo.Name,
                    DateTime.UtcNow,
                    logFileInfo.CreationTimeUtc,
                    logFileInfo.LastWriteTimeUtc,
                    positionItem.EventNumber,
                    positionItem.EventUUID,
                    positionItem.EventPeriod,
                    positionItem.CurrentFileData.Replace("\\", "\\\\"),
                });
            }

            if (positionsForInsert.Count > 0)
            {
                using (ClickHouseBulkCopy bulkCopyInterface = new ClickHouseBulkCopy(_connection)
                {
                    DestinationTableName = "LogFiles",
                    BatchSize = 100000
                })
                {
                    await bulkCopyInterface.WriteToServerAsync(positionsForInsert);
                }
            }
        }

        public IDictionary<string, ExtendedEventsPosition> GetCurrentLogPositions(
            ExtendedEventsLogBase xEventsLog)
        {
            var cmdGetLastLogFileInfo = _connection.CreateCommand();
            cmdGetLastLogFileInfo.CommandText = Resources.Query_GetActualPositions;
            cmdGetLastLogFileInfo.AddParameterToCommand("ExtendedEventsLog", DbType.AnsiString, xEventsLog.Name);

            IDictionary<string, ExtendedEventsPosition> output = new Dictionary<string, ExtendedEventsPosition>();
            using (var cmdReader = cmdGetLastLogFileInfo.ExecuteReader())
            {
                while (cmdReader.Read())
                {
                    string fileName = cmdReader.GetString(2).Replace("\\\\", "\\");
                    bool finishReadFile = cmdReader.GetBoolean(9);
                    output.Add(fileName, new ExtendedEventsPosition(
                        cmdReader.GetInt64(6),
                        fileName,
                        cmdReader.GetString(7),
                        cmdReader.GetDateTime(8),
                        finishReadFile,
                        cmdReader.GetDateTime(10),
                        cmdReader.GetDateTime(11)
                    ));
                }
            }

            return output;
        }

        #endregion

        public void Dispose()
        {
            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();

                _connection.Dispose();
                _connection = null;
            }
        }

        #endregion

        #region Service

        private void GetRowsDataMaxPeriodAndId(
            ExtendedEventsLogBase xEventsLog,
            string fileName, DateTime fromPeriod,
            out DateTime maxPeriod, out long maxId)
        {
            DateTime outputMaxPeriod = DateTime.MinValue;
            long outputMaxId = 0;

            using (var command = _connection.CreateCommand())
            {
                command.CommandText =
                    @"SELECT
                        MAX(Period) AS MaxPeriod,
                        MAX(EventNumber) AS MaxId
                    FROM XEventData AS RD
                    WHERE ExtendedEventsLog = {xEventLog:String}
                        AND FileName = {fileName:String}
                        AND Period >= {fromPeriod:DateTime}";
                command.AddParameterToCommand("xEventLog", xEventsLog.Name);
                command.AddParameterToCommand("fileName", fileName);
                command.AddParameterToCommand("fromPeriod", fromPeriod);
                using (var cmdReader = command.ExecuteReader())
                {
                    if (cmdReader.Read())
                    {
                        outputMaxPeriod = cmdReader.GetDateTime(0);
                        outputMaxId = cmdReader.GetInt64(1);
                    }
                }
            }

            maxPeriod = outputMaxPeriod;
            maxId = outputMaxId;
        }

        public readonly struct LastRowsInfoByLogFile
        {
            public LastRowsInfoByLogFile(DateTime maxPeriod, long maxId)
            {
                MaxPeriod = maxPeriod;
                MaxId = maxId;
            }

            public DateTime MaxPeriod { get; }
            public long MaxId { get; }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using ClickHouse.Client.Copy;
using YY.DBTools.SQLServer.XEvents.ToClickHouse.Helpers;
using YY.DBTools.SQLServer.XEvents.ToClickHouse.Models;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse
{
    public class ClickHouseContext : IDisposable
    {
        #region Private Members

        private ClickHouseConnection _connection;

        #endregion

        #region Constructors

        public ClickHouseContext(string connectionSettings)
        {
            CheckDatabaseSettings(connectionSettings);

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

        public async Task SaveRowsData(List<XEventData> eventData)
        {
            using (ClickHouseBulkCopy bulkCopyInterface = new ClickHouseBulkCopy(_connection)
            {
                DestinationTableName = "XEventData",
                BatchSize = 100000
            })
            {
                var values = eventData.Select(i => new object[]
                {
                    i.FileName,
                    i.EventNumber,
                    i.Timestamp.DateTime,
                    i.EventName,
                    i.UUID.ToString(),
                    i.Username ?? string.Empty,
                    i.UsernameNT ?? string.Empty,
                    i.UsernameSessionNT ?? string.Empty,
                    i.SessionId ?? 0,
                    i.PlanHandle ?? string.Empty,
                    i.IsSystem == null ? 0 : ((bool)i.IsSystem ? 1 : 0),
                    i.ExecutionPlanGuid?.ToString() ?? string.Empty,
                    i.DatabaseName ?? string.Empty,
                    i.DatabaseId ?? 0,
                    i.NumaNodeId ?? 0,
                    i.CpuId ?? 0,
                    i.ProcessId ?? 0,
                    i.SQLText ?? string.Empty,
                    i.SQLTextHash ?? string.Empty,
                    i.ClientAppName ?? string.Empty,
                    i.ClientHostname ?? string.Empty,
                    i.ClientId ?? 0,
                    i.QueryHash ?? string.Empty,
                    i.ServerInstanceName ?? string.Empty,
                    i.ServerPrincipalName ?? string.Empty,
                    i.ServerPrincipalId ?? 0,
                    i.CpuTime ?? 0,
                    i.Duration ?? 0,
                    i.PhysicalReads ?? 0,
                    i.LogicalReads ?? 0,
                    i.Writes ?? 0,
                    i.RowCount ?? 0,
                    i.GetActionsAsJSON(),
                    i.GetFieldsAsJSON()
                }).AsEnumerable();

                await bulkCopyInterface.WriteToServerAsync(values);
            }
        }
        public async Task SaveLogPosition(FileInfo logFileInfo, ExtendedEventsPosition position, bool finishReadFile = false)
        {
            var commandAddLogInfo = _connection.CreateCommand();
            commandAddLogInfo.CommandText =
                @"INSERT INTO LogFiles (
                    CreateDate,
                    FileName,
                    FileCreateDate,
                    FileModificationDate,
                    LastEventNumber,
                    LastEventUUID,
                    LastEventPeriod,
                    FinishReadFile
                ) VALUES (
                    {CreateDate:DateTime},
                    {FileName:String},
                    {FileCreateDate:DateTime},
                    {FileModificationDate:DateTime},
                    {LastEventNumber:Int64},
                    {LastEventUUID:String},
                    {LastEventPeriod:DateTime},
                    {FinishReadFile:Int64}
                )";

            commandAddLogInfo.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "CreateDate",
                DbType = DbType.DateTime,
                Value = DateTime.Now
            });
            commandAddLogInfo.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "FileName",
                DbType = DbType.AnsiString,
                Value = logFileInfo.Name
            });
            commandAddLogInfo.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "FileCreateDate",
                DbType = DbType.DateTime,
                Value = logFileInfo.CreationTime.GetAllowDateTime()
            });
            commandAddLogInfo.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "FileModificationDate",
                DbType = DbType.DateTime,
                Value = logFileInfo.LastWriteTime.GetAllowDateTime()
            });
            commandAddLogInfo.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "LastEventNumber",
                DbType = DbType.Int64,
                Value = position.EventNumber
            });
            commandAddLogInfo.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "LastEventUUID",
                DbType = DbType.AnsiString,
                Value = position.EventUUID
            });
            commandAddLogInfo.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "LastEventPeriod",
                DbType = DbType.DateTime,
                Value = position.EventPeriod?.DateTime.GetAllowDateTime() ?? DateTime.MinValue.GetAllowDateTime()
            });
            commandAddLogInfo.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "FinishReadFile",
                DbType = DbType.Int64,
                Value = finishReadFile ? 1 : 0
            });

            await commandAddLogInfo.ExecuteNonQueryAsync();
        }
        public async Task RemoveArchiveLogFileRecords(string FileName)
        {
            var commandRemoveArchiveLogInfo = _connection.CreateCommand();
            commandRemoveArchiveLogInfo.CommandText =
                @"ALTER TABLE LogFiles DELETE
                WHERE FileName = {FileName:String}
                    AND CreateDate < (
                    SELECT MAX(CreateDate) AS LastCreateDate
                    FROM LogFiles lf
                    WHERE FileName = {FileName:String}
                )";
            commandRemoveArchiveLogInfo.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = "FileName",
                DbType = DbType.AnsiString,
                Value = FileName
            });
            await commandRemoveArchiveLogInfo.ExecuteNonQueryAsync();
        }
        public async Task<DateTime> GetRowsDataMaxPeriod(string FileName)
        {
            DateTime output = DateTime.MinValue;

            using (var command = _connection.CreateCommand())
            {
                command.CommandText =
                    @"SELECT
                        MAX(Period) AS MaxPeriod
                    FROM XEventData AS RD
                    WHERE FileName = {FileName:String} ";
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
        public async Task<bool> RowDataExistOnDatabase(string FileName, XEventData eventData)
        {
            bool output = false;

            using (var command = _connection.CreateCommand())
            {
                command.CommandText =
                    @"SELECT
                        FileName
                    FROM XEventData AS XD
                    WHERE FileName = {FileName:String}
                        AND Period = {Period:DateTime}
                        AND EventNumber = {EventNumber:Int64}
                        AND EventName = {EventName:String}
                        AND UUID = {UUID:String}";
                command.Parameters.Add(new ClickHouseDbParameter
                {
                    ParameterName = "FileName",
                    DbType = DbType.AnsiString,
                    Value = FileName
                });
                command.Parameters.Add(new ClickHouseDbParameter
                {
                    ParameterName = "Period",
                    DbType = DbType.DateTime,
                    Value = eventData.Timestamp.DateTime
                });
                command.Parameters.Add(new ClickHouseDbParameter
                {
                    ParameterName = "EventName",
                    DbType = DbType.AnsiString,
                    Value = eventData.EventName
                });
                command.Parameters.Add(new ClickHouseDbParameter
                {
                    ParameterName = "UUID",
                    DbType = DbType.AnsiString,
                    Value = eventData.UUID.ToString()
                });
                command.Parameters.Add(new ClickHouseDbParameter
                {
                    ParameterName = "EventNumber",
                    DbType = DbType.Int64,
                    Value = eventData.EventNumber
                });

                using (var cmdReader = await command.ExecuteReaderAsync())
                {
                    if (await cmdReader.ReadAsync())
                        output = true;
                }
            }

            return output;
        }
        public async Task<bool> LogFileLoaded(string FileName)
        {
            bool output = false;
            FileInfo logFileInfo = new FileInfo(FileName);

            using (var command = _connection.CreateCommand())
            {
                command.CommandText =
                    @"SELECT
                        FileName
                    FROM LogFiles AS LF
                    WHERE FileName = {FileName:String}
                        AND FinishReadFile = 1
                        AND FileModificationDate = {FileModificationDate:DateTime}";
                command.Parameters.Add(new ClickHouseDbParameter
                {
                    ParameterName = "FileName",
                    DbType = DbType.AnsiString,
                    Value = logFileInfo.Name
                });
                command.Parameters.Add(new ClickHouseDbParameter
                {
                    ParameterName = "FileModificationDate",
                    DbType = DbType.DateTime,
                    Value = logFileInfo.LastWriteTime
                });

                using (var cmdReader = await command.ExecuteReaderAsync())
                {
                    if (await cmdReader.ReadAsync())
                        output = true;
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

        #region Private Methods

        private void CheckDatabaseSettings(string connectionSettings)
        {
            ClickHouseHelpers.CreateDatabaseIfNotExist(connectionSettings);
        }

        #endregion
    }
}

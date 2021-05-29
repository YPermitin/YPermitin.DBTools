using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse.Helpers
{
    public static class ClickHouseHelpers
    {
        private static readonly DateTime _minDateTime = new DateTime(1970, 1, 1);

        public static readonly DateTime MinDateTimeValue = new DateTime(1970, 1, 1);

        public static Dictionary<string, string> GetConnectionParams(string connectionString)
        {
            var connectionParams = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split('=', StringSplitOptions.RemoveEmptyEntries))
                .Select(i => new { Name = i[0], Value = i.Length > 1 ? i[1] : string.Empty });

            return connectionParams.ToDictionary(o => o.Name, o => o.Value);
        }

        public static void AddParameterToCommand(this ClickHouseCommand command, string name, object value)
        {
            command.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = name,
                Value = value
            });
        }

        public static void AddParameterToCommand(this ClickHouseCommand command, string name, DbType type, object value)
        {
            command.Parameters.Add(new ClickHouseDbParameter
            {
                ParameterName = name,
                DbType = type,
                Value = value
            });
        }

        public static DateTime GetAllowDateTime(this DateTime sourceValue)
        {
            return (sourceValue < _minDateTime ? _minDateTime : sourceValue);
        }

        public static DateTime? GetAllowDateTime(this DateTime? sourceValue)
        {
            return (sourceValue == null || sourceValue < _minDateTime ? _minDateTime : sourceValue);
        }

        public static void CreateDatabaseIfNotExist(string connectionSettings)
        {
            string databaseName = GetDatabaseName(connectionSettings, out _, out _);
            ExecNonReaderQuery(connectionSettings, $"CREATE DATABASE IF NOT EXISTS {databaseName}");
        }

        public static void DropDatabaseIfExist(string connectionSettings)
        {
            string databaseName = GetDatabaseName(connectionSettings, out _, out _);
            ExecNonReaderQuery(connectionSettings, $"DROP DATABASE IF EXISTS {databaseName}");
        }

        private static string GetDatabaseName(string connectionSettings, out string paramName, out string paramValue)
        {
            var connectionParams = GetConnectionParams(connectionSettings);
            var databaseParam = connectionParams.FirstOrDefault(e => e.Key.ToUpper() == "DATABASE");
            string databaseName = databaseParam.Value;
            paramName = databaseParam.Key;
            paramValue = databaseParam.Value;

            return databaseName;
        }

        private static void ExecNonReaderQuery(string connectionSettings, string commandTest)
        {
            string databaseName = GetDatabaseName(connectionSettings, out var paramName, out var paramValue);

            if (databaseName != null)
            {
                string connectionStringDefault = connectionSettings.Replace(
                    $"{paramName}={paramValue}",
                    $"Database=default"
                );
                using (var defaultConnection = new ClickHouseConnection(connectionStringDefault))
                {
                    defaultConnection.Open();
                    var cmdDefault = defaultConnection.CreateCommand();
                    cmdDefault.CommandText = commandTest;
                    cmdDefault.ExecuteNonQuery();
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using ClickHouse.Client.ADO;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse.Helpers
{
    public static class ClickHouseHelpers
    {
        private static readonly DateTime _minDateTime = new DateTime(1970, 1, 1);

        public static Dictionary<string, string> GetConnectionParams(string connectionString)
        {
            var connectionParams = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split('=', StringSplitOptions.RemoveEmptyEntries))
                .Select(i => new { Name = i[0], Value = i.Length > 1 ? i[1] : string.Empty });

            return connectionParams.ToDictionary(o => o.Name, o => o.Value);
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
            var connectionParams = GetConnectionParams(connectionSettings);
            var databaseParam = connectionParams.FirstOrDefault(e => e.Key.ToUpper() == "DATABASE");
            string databaseName = databaseParam.Value;

            if (databaseName != null)
            {
                string connectionStringDefault = connectionSettings.Replace(
                    $"{databaseParam.Key}={databaseParam.Value}",
                    $"Database=default"
                );
                using (var defaultConnection = new ClickHouseConnection(connectionStringDefault))
                {
                    defaultConnection.Open();
                    var cmdDefault = defaultConnection.CreateCommand();
                    cmdDefault.CommandText = $"CREATE DATABASE IF NOT EXISTS {databaseName}";
                    cmdDefault.ExecuteNonQuery();
                }
            }
        }
        public static void DropDatabaseIfExist(string connectionSettings)
        {
            var connectionParams = GetConnectionParams(connectionSettings);
            var databaseParam = connectionParams.FirstOrDefault(e => e.Key.ToUpper() == "DATABASE");
            string databaseName = databaseParam.Value;

            if (databaseName != null)
            {
                string connectionStringDefault = connectionSettings.Replace(
                    $"{databaseParam.Key}={databaseParam.Value}",
                    $"Database=default"
                );
                using (var defaultConnection = new ClickHouseConnection(connectionStringDefault))
                {
                    defaultConnection.Open();
                    var cmdDefault = defaultConnection.CreateCommand();
                    cmdDefault.CommandText = $"DROP DATABASE IF EXISTS {databaseName}";
                    cmdDefault.ExecuteNonQuery();
                }
            }
        }
    }
}

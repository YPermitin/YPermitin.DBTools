using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using YY.DBTools.SQLServer.XEvents.ToClickHouse.Helpers;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse.Models
{
    public class XEventData
    {
        private static readonly List<string> _commonActions = new List<string>()
        {
            "username",
            "nt_username",
            "session_nt_username",
            "session_id",
            "plan_handle",
            "is_system",
            "execution_plan_guid",
            "database_name",
            "database_id",
            "numa_node_id",
            "cpu_id",
            "process_id",
            "sql_text",
            "client_app_name",
            "client_hostname",
            "client_pid",
            "query_hash",
            "server_instance_name",
            "server_principal_name",
            "server_principal_sid"
        };
        private static readonly List<string> _commonFields = new List<string>()
        {
            "cpu_time",
            "duration",
            "physical_reads",
            "logical_reads",
            "writes",
            "row_count"
        };

        private readonly ExtendedEvent _eventData;

        public XEventData(string fileName, long eventNumber, ExtendedEvent EventData)
        {
            FileName = fileName;
            EventNumber = eventNumber;
            _eventData = EventData;
        }

        #region Common

        public long Id => _eventData.Id;

        public DateTimeOffset Timestamp => _eventData.Timestamp;

        public string EventName => _eventData.Name;

        public Guid UUID => _eventData.UUID;

        public string FileName { get; }

        public long EventNumber { get; }

        #endregion

        #region Actions

        public string Username => _eventData.Actions.GetStringValueByKey("username");
        public string UsernameNT => _eventData.Actions.GetStringValueByKey("nt_username");
        public string UsernameSessionNT => _eventData.Actions.GetStringValueByKey("session_nt_username");
        public long? SessionId => _eventData.Actions.GetLongValueByKey("session_id");
        public string PlanHandle
        {
            get
            {
                byte[] souceData = _eventData.Actions.GetByteArrayValueByKey("plan_handle");
                if (souceData != null)
                {
                    using (MemoryStream inMemoryData = new MemoryStream(souceData))
                    {
                        return new BinaryFormatter().Deserialize(inMemoryData) as string;
                    }
                }
                return null;
            }
        }
        public bool? IsSystem => _eventData.Actions.GetBoolValueByKey("is_system");
        public Guid? ExecutionPlanGuid => _eventData.Actions.GetGuidValueByKey("execution_plan_guid");
        public string DatabaseName => _eventData.Actions.GetStringValueByKey("database_name");
        public long? DatabaseId => _eventData.Actions.GetLongValueByKey("database_id");
        public long? NumaNodeId => _eventData.Actions.GetLongValueByKey("numa_node_id");
        public long? CpuId => _eventData.Actions.GetLongValueByKey("cpu_id");
        public long? ProcessId => _eventData.Actions.GetLongValueByKey("process_id");
        public string SQLText => _eventData.Actions.GetStringValueByKey("sql_text")?.ClearSQLQuery();
        public string SQLTextHash => SQLText?.GetQueryHash(true);
        public string ClientAppName => _eventData.Actions.GetStringValueByKey("client_app_name");
        public string ClientHostname => _eventData.Actions.GetStringValueByKey("client_hostname");
        public long? ClientId => _eventData.Actions.GetLongValueByKey("client_pid");
        public string QueryHash
        {
            get
            {
                byte[] souceData = _eventData.Actions.GetByteArrayValueByKey("query_hash");
                if (souceData != null)
                {
                    using (MemoryStream inMemoryData = new MemoryStream(souceData))
                    {
                        return new BinaryFormatter().Deserialize(inMemoryData) as string;
                    }
                }
                return null;
            }
        }
        public string ServerInstanceName => _eventData.Actions.GetStringValueByKey("server_instance_name");
        public string ServerPrincipalName => _eventData.Actions.GetStringValueByKey("server_principal_name");
        public long? ServerPrincipalId => _eventData.Actions.GetLongValueByKey("server_principal_sid");

        #endregion

        #region Fields

        public long? CpuTime => _eventData.Fields.GetLongValueByKey("cpu_time");
        public long? Duration => _eventData.Fields.GetLongValueByKey("duration");
        public long? PhysicalReads => _eventData.Fields.GetLongValueByKey("physical_reads");
        public long? LogicalReads => _eventData.Fields.GetLongValueByKey("logical_reads");
        public long? Writes => _eventData.Fields.GetLongValueByKey("writes");
        public long? RowCount => _eventData.Fields.GetLongValueByKey("row_count");

        #endregion

        #region FullEventData

        private IReadOnlyDictionary<string, object> AllActions => _eventData.Actions;
        private IReadOnlyDictionary<string, object> AllFields => _eventData.Fields;

        #endregion

        #region Public Methods

        public string GetActionsAsJSON(bool excludeCommonActions = true)
        {
            var actionsForJson = AllActions
                .Where(e => !excludeCommonActions || !_commonActions.Contains(e.Key))
                .Select(e => e)
                .ToDictionary(e => e.Key, e => e.Value);

            string actionsAsJson = JsonConvert.SerializeObject(actionsForJson);

            return actionsAsJson;
        }

        public string GetFieldsAsJSON(bool excludeCommonFields = true)
        {
            var actionsForJson = AllFields
                .Where(e => !excludeCommonFields || !_commonFields.Contains(e.Key))
                .Select(e => e)
                .ToDictionary(e => e.Key, e => e.Value);

            string actionsAsJson = JsonConvert.SerializeObject(actionsForJson);

            return actionsAsJson;
        }

        #endregion
    }
}

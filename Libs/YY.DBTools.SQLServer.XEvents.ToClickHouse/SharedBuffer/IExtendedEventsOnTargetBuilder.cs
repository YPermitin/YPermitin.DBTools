using System.Collections.Generic;
using System.Threading.Tasks;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer
{
    public interface IExtendedEventsOnTargetBuilder
    {
        IXEventsOnTarget CreateTarget(XEventsExportSettings settings, KeyValuePair<LogBufferItemKey, LogBufferItem> logBufferItem);
        IDictionary<string, ExtendedEventsPosition> GetCurrentLogPositions(XEventsExportSettings settings, ExtendedEventsLogBase xEventsLog);
        Task SaveRowsData(XEventsExportSettings settings, Dictionary<LogBufferItemKey, LogBufferItem> dataFromBuffer);
    }
}

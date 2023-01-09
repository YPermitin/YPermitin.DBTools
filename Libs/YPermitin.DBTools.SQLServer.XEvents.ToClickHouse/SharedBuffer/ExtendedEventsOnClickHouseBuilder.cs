using System.Collections.Generic;
using System.Threading.Tasks;

namespace YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer
{
    public class ExtendedEventsOnClickHouseBuilder : IExtendedEventsOnTargetBuilder
    {
        public IXEventsOnTarget CreateTarget(XEventsExportSettings settings, KeyValuePair<LogBufferItemKey, LogBufferItem> logBufferItem)
        {
            IXEventsOnTarget target = new ExtendedEventsOnClickHouse(settings.ConnectionString,
                logBufferItem.Key.Settings.Portion);
            target.SetLogInformation(logBufferItem.Key.Settings.XEventsLog);

            return target;
        }

        public IDictionary<string, ExtendedEventsPosition> GetCurrentLogPositions(XEventsExportSettings settings, ExtendedEventsLogBase xEventsLog)
        {
            using (ClickHouseContext context = new ClickHouseContext(settings.ConnectionString))
            {
                return context.GetCurrentLogPositions(xEventsLog);
            }
        }

        public async Task SaveRowsData(XEventsExportSettings settings, Dictionary<LogBufferItemKey, LogBufferItem> dataFromBuffer)
        {
            using (ClickHouseContext context = new ClickHouseContext(settings.ConnectionString))
            {
                await context.SaveRowsData(dataFromBuffer);
            }
        }
    }
}

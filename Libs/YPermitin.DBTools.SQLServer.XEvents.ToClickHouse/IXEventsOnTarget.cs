using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.Models;

namespace YPermitin.DBTools.SQLServer.XEvents.ToClickHouse
{
    public interface IXEventsOnTarget
    {
        Task<ExtendedEventsPosition> GetLastPosition(string fileName);
        Task SaveLogPosition(ExtendedEventsPosition position);
        int GetPortionSize();
        Task Save(XEventData eventData);
        Task Save(List<XEventData> eventsData);
        void SetLogInformation(ExtendedEventsLogBase xEventsLog);
        Task<bool> LogFileChanged(FileInfo logFileInfo);
    }
}

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YY.DBTools.SQLServer.XEvents.ToClickHouse.Models;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse
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

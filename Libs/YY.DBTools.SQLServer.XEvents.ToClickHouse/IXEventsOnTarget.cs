using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YY.DBTools.SQLServer.XEvents.ToClickHouse.Models;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse
{
    public interface IXEventsOnTarget
    {
        ExtendedEventsPosition GetLastPosition();
        Task SaveLogPosition(FileInfo logFileInfo, ExtendedEventsPosition position, bool finishReadFile);
        int GetPortionSize();
        Task Save(XEventData eventData);
        Task Save(IList<XEventData> eventsData);
        Task<bool> LogFileLoaded(string fileName);
    }
}

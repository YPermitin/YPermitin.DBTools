using System;
using System.Threading;
using System.Threading.Tasks;

namespace YPermitin.DBTools.SQLServer.XEvents.ToClickHouse
{
    public interface IXEventExportMaster : IDisposable
    {
        void SetTarget(IXEventsOnTarget target);
        void SetXEventsPath(string eventPath);
        Task StartSendEventsToStorage();
        Task StartSendEventsToStorage(CancellationToken cancellationToken);
    }
}

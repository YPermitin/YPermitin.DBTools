using System.Threading;
using System.Threading.Tasks;

namespace YY.DBTools.SQLServer.XEvents
{
    public interface IExtendedEventsReader
    {
        Task StartReadEvents();
        Task StartReadEvents(CancellationToken cancellationToken);
        Task<long> Count();
        Task<long> Count(CancellationToken cancellationToken);
        void Reset();
        void NextFile();
        ExtendedEventsPosition GetCurrentPosition();
    }
}

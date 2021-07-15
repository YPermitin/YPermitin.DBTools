using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YY.DBTools.SQLServer.XEvents.ToClickHouse;
using YY.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer;
using YY.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer.EventArgs;

namespace YY.DBTools.SQLServer.ExtendedEventsToClickHouse
{
    public class XEventToClickHouse
    {
        private readonly ILogger _logger;
        private readonly XEventsExportApplicationSettings _settings;
        private long _totalRows;
        private long _lastPortionRows;

        public XEventToClickHouse(ILogger<XEventToClickHouse> logger, XEventsExportApplicationSettings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public async Task Run()
        {
            await Run(CancellationToken.None);
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            ExtendedEventsExport export = new ExtendedEventsExport(
                _settings, 
                new ExtendedEventsOnClickHouseBuilder());
            export.OnErrorEvent += OnErrorExportData;
            export.OnSendLogEvent += OnExportData;
            await export.StartExport(cancellationToken);
        }

        #region Events

        private static void OnErrorExportData(OnErrorExportSharedBufferEventArgs e)
        {
            Console.WriteLine($"Log name: {e?.Exception?.Settings?.Name ?? "Unknown"}\n" +
                              $"Error info: {e.Exception.ToString()}");
        }

        private void OnExportData(BeforeExportDataEventArgs e)
        {
            if(e.Cancel) return;

            _lastPortionRows = e.Rows.Count;
            _totalRows += _lastPortionRows;

            string infoMessage = 
                  $"[{DateTime.Now}] Last portion for export: {_lastPortionRows}                  \n"
                + $"[{DateTime.Now}] Total events: {_totalRows}                  ";
            _logger.LogInformation(infoMessage);

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(infoMessage);
        }

        #endregion
    }
}

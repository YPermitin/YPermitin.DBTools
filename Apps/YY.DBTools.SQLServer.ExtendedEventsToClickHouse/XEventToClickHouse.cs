using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YY.DBTools.SQLServer.XEvents.ToClickHouse;

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
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) break;

                using (IXEventExportMaster export =
                    new XEventExportMaster(BeforeExportData, AfterExportData, OnErrorExportData))
                {
                    export.SetXEventsPath(_settings.XEventsPath);
                    IXEventsOnTarget target =
                        new ExtendedEventsOnClickHouse(_settings.ConnectionString, _settings.Portion);
                    export.SetTarget(target);
                    await export.StartSendEventsToStorage(cancellationToken);
                }

                if (_settings.UseWatchMode)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    await Task.Delay(_settings.DelayMs, cancellationToken);
                }
                else
                {
                    break;
                }

                if (cancellationToken.IsCancellationRequested) break;
            }
        }

        #region Events

        private static void OnErrorExportData(OnErrorExportDataEventArgs e)
        {
            Console.WriteLine(e.Exception.ToString());
        }

        private void BeforeExportData(BeforeExportDataEventArgs e)
        {
            _lastPortionRows = e.Rows.Count;
            _totalRows += e.Rows.Count;

            string infoMessage = $"[{DateTime.Now}] Last potrion for export: {e.Rows.Count}                  ";
            _logger.LogInformation(infoMessage);

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(infoMessage);
        }

        private void AfterExportData(AfterExportDataEventArgs e)
        {
            var infoMessage = $"[{DateTime.Now}] Total events exported: {_totalRows}                        ";
            _logger.LogInformation(infoMessage);
            Console.WriteLine(infoMessage);

            infoMessage = $"[{DateTime.Now}] Last portion {_lastPortionRows}                                     ";
            _logger.LogInformation(infoMessage);
            Console.WriteLine(infoMessage);

            infoMessage = $"[{DateTime.Now}] Last event period: {e.CurrentPosition.EventPeriod}                ";
            _logger.LogInformation(infoMessage);
            Console.WriteLine(infoMessage);

            infoMessage = $"[{DateTime.Now}] Last event ID: {e.CurrentPosition.EventUUID}                ";
            _logger.LogInformation(infoMessage);
            Console.WriteLine(infoMessage);

            infoMessage = $"[{DateTime.Now}] File: {e.CurrentPosition.CurrentFileData}                ";
            _logger.LogInformation(infoMessage);
            Console.WriteLine(infoMessage);

            Console.WriteLine();
            Console.WriteLine();
            if (_settings.AllowInteractiveActions)
                Console.WriteLine("Press 'CTRL + C' to cancel export operation...");
            else
                Console.WriteLine();
        }

        #endregion
    }
}

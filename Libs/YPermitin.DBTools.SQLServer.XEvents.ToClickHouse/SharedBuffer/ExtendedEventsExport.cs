using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer.EventArgs;
using YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer.Exceptions;

namespace YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer
{
    public class ExtendedEventsExport
    {
        #region Private Members

        private readonly XEventsExportSettings _settings;
        private readonly LogBuffers _logBuffers;
        private readonly IExtendedEventsOnTargetBuilder _xEventsTargetBuilder;

        #endregion

        #region Constructors

        public ExtendedEventsExport(XEventsExportSettings settings, IExtendedEventsOnTargetBuilder xEventsTargetBuilder)
        {
            _settings = settings;
            _logBuffers = new LogBuffers();
            _xEventsTargetBuilder = xEventsTargetBuilder;

            foreach (var logSourceSettings in _settings.LogSources)
            {
                _logBuffers.LogPositions.TryAdd(logSourceSettings,
                    new ConcurrentDictionary<string, ExtendedEventsPosition>());

                var logPositions = xEventsTargetBuilder.GetCurrentLogPositions(settings, logSourceSettings.XEventsLog);
                foreach (var logPosition in logPositions)
                {
                    FileInfo logFileInfo = new FileInfo(logPosition.Value.CurrentFileData);
                    _logBuffers.LogPositions.AddOrUpdate(logSourceSettings,
                        (settingsKey) =>
                        {
                            var newPositions = new ConcurrentDictionary<string, ExtendedEventsPosition>();
                            newPositions.AddOrUpdate(logFileInfo.Name,
                                    (dirName) => logPosition.Value,
                                    (dirName, oldPosition) => logPosition.Value);
                            return newPositions;
                        },
                        (settingsKey, logBufferItemOld) =>
                        {
                            logBufferItemOld.AddOrUpdate(logFileInfo.Name,
                                    (dirName) => logPosition.Value,
                                    (dirName, oldPosition) => logPosition.Value);
                            return logBufferItemOld;
                        });
                }
            }
        }

        #endregion

        #region Public Methods

        public async Task StartExport()
        {
            await StartExport(CancellationToken.None);
        }

        public async Task StartExport(CancellationToken cancellationToken)
        {
            List<Task> exportJobs = new List<Task>();
            foreach (var logSource in _settings.LogSources)
            {
                Task logExportTask = Task.Run(() => LogExportJob(logSource, cancellationToken), cancellationToken);
                exportJobs.Add(logExportTask);
                await Task.Delay(1000, cancellationToken);
            }
            
            await Task.Delay(10000, cancellationToken);

            exportJobs.Add(Task.Run(() => SendLogFromBuffer(cancellationToken), cancellationToken));

            Task.WaitAll(exportJobs.ToArray(), cancellationToken);
        }

        #endregion

        #region Private Methods

        private async Task LogExportJob(
            XEventsExportSettings.LogSourceSettings settings,
            CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                bool bufferBlocked = (_logBuffers.TotalItemsCount >= _settings.Export.Buffer.MaxBufferSizeItemsCount);

                try
                {
                    if (!bufferBlocked)
                    {
                        using (XEventExportMaster exporter = new XEventExportMaster(
                            OnSend, null, OnErrorExportDataToBuffer
                            ))
                        {
                            exporter.SetXEventsPath(settings.SourcePath);

                            var target = new ExtendedEventsOnBuffer(_logBuffers, settings.Portion);
                            target.SetLogSettings(settings);
                            
                            exporter.SetTarget(target);
                            await exporter.StartSendEventsToStorage(cancellationToken);
                        }
                    }

                    if (bufferBlocked)
                    {
                        await Task.Delay(100, cancellationToken);
                    }
                    else if (_settings.WatchMode.Use)
                    {
                        await Task.Delay(_settings.WatchMode.Periodicity, cancellationToken);
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    RaiseOnError(new OnErrorExportSharedBufferEventArgs(
                        new ExportSharedBufferException("Log export job failed.", e, settings)));
                    await Task.Delay(60000, cancellationToken);
                }
            }
        }

        private async Task SendLogFromBuffer(CancellationToken cancellationToken)
        {
            DateTime lastExportDate = DateTime.UtcNow;
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                bool needExport = false;
                long itemsCountForExport = _logBuffers.TotalItemsCount;
                if (itemsCountForExport > 0)
                {
                    if (itemsCountForExport >= _settings.Export.Buffer.MaxItemCountSize)
                    {
                        needExport = true;
                    }
                    else
                    {
                        var createTimeLeftMs = (DateTime.UtcNow - lastExportDate).TotalMilliseconds;
                        if (lastExportDate != DateTime.MinValue &&
                            createTimeLeftMs >= _settings.Export.Buffer.MaxSaveDurationMs)
                        {
                            needExport = true;
                        }
                    }
                }

                if (needExport)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        var itemsToUpload = _logBuffers.LogBuffer
                            .Select(i => i.Key)
                            .OrderBy(i => i.Period)
                            .ToList();

                        var dataToUpload = _logBuffers.LogBuffer
                            .Where(i => itemsToUpload.Contains(i.Key))
                            .ToDictionary(k => k.Key, v => v.Value);

                        OnSendFromBuffer(new BeforeExportDataEventArgs()
                        {
                            Rows = dataToUpload
                                .SelectMany(e => e.Value.LogRows)
                                .Select(e => e.Value)
                                .ToList()
                        });

                        await _xEventsTargetBuilder.SaveRowsData(_settings, dataToUpload);

                        foreach (var itemToUpload in itemsToUpload)
                        {
                            _logBuffers.LogBuffer.TryRemove(itemToUpload, out _);
                        }
                        lastExportDate = DateTime.UtcNow;
                    }
                    catch (Exception e)
                    {
                        RaiseOnError(new OnErrorExportSharedBufferEventArgs(
                            new ExportSharedBufferException("Send log from buffer failed.", e, null)));
                        await Task.Delay(60000, cancellationToken);
                    }
                }

                await Task.Delay(1000, cancellationToken);

                if (!_settings.WatchMode.Use
                    && _logBuffers.TotalItemsCount == 0)
                {
                    break;
                }
            }
        }

        #endregion

        #region Events

        public delegate void OnSendLogFromSharedBufferEventArgsHandler(BeforeExportDataEventArgs args);
        public delegate void OnErrorExportSharedBufferEventArgsHandler(OnErrorExportSharedBufferEventArgs args);
        public event OnSendLogFromSharedBufferEventArgsHandler OnSendLogEvent;
        public event OnErrorExportSharedBufferEventArgsHandler OnErrorEvent;

        protected void OnSend(BeforeExportDataEventArgs args)
        {
            bool bufferBlocked = (_logBuffers.TotalItemsCount >= _settings.Export.Buffer.MaxBufferSizeItemsCount);
            while (bufferBlocked)
            {
                Thread.Sleep(1000);
                bufferBlocked = (_logBuffers.TotalItemsCount >= _settings.Export.Buffer.MaxBufferSizeItemsCount);
            }
        }

        protected void OnSendFromBuffer(BeforeExportDataEventArgs args)
        {
            OnSendLogEvent?.Invoke(args);
        }

        protected void RaiseOnError(OnErrorExportSharedBufferEventArgs args)
        {
            OnErrorEvent?.Invoke(args);
        }
        private void OnErrorExportDataToBuffer(OnErrorExportDataEventArgs e)
        {
            RaiseOnError(new OnErrorExportSharedBufferEventArgs(
                new ExportSharedBufferException("Export data to buffer failed.", e.Exception, null)));
        }

        #endregion
    }
}

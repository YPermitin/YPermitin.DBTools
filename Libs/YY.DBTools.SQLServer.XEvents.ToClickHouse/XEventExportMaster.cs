using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YY.DBTools.SQLServer.XEvents.EventArguments;
using YY.DBTools.SQLServer.XEvents.ToClickHouse.Models;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse
{
    public sealed class XEventExportMaster : IXEventExportMaster, IDisposable
    {
        #region Private Member Variables

        private string _eventPath;
        private IXEventsOnTarget _target;
        private ExtendedEventsReader _reader;
        private readonly List<XEventData> _dataToSend;
        private int _portionSize;

        public delegate void BeforeExportDataHandler(BeforeExportDataEventArgs e);
        private event BeforeExportDataHandler _beforeExportData;
        public delegate void AfterExportDataHandler(AfterExportDataEventArgs e);
        private event AfterExportDataHandler _afterExportData;
        public delegate void OnErrorExportDataHandler(OnErrorExportDataEventArgs e);
        private event OnErrorExportDataHandler _onErrorExportData;

        #endregion

        #region Constructor

        public XEventExportMaster() : this(null, null, null)
        {
        }
        public XEventExportMaster(
            BeforeExportDataHandler BeforeExportData,
            AfterExportDataHandler AfterExportData,
            OnErrorExportDataHandler OnErrorExportData)
        {
            _dataToSend = new List<XEventData>();
            _portionSize = 0;

            _beforeExportData = BeforeExportData;
            _afterExportData = AfterExportData;
            _onErrorExportData = OnErrorExportData;
        }

        #endregion

        #region Public Methods

        public void SetXEventsPath(string eventPath)
        {
            _eventPath = eventPath;
            if (!string.IsNullOrEmpty(_eventPath))
            {
                _reader = new ExtendedEventsReader(eventPath,
                    ExtendedEventsReader_OnReadEvent,
                    null,
                    ExtendedEventsReader_BeforeReadFile,
                    ExtendedEventsReader_AfterReadFile,
                    ExtendedEventsReader_OnErrorEvent);
            }
        }
        public void SetTarget(IXEventsOnTarget target)
        {
            _target = target;
            if (_target != null)
            {
                _portionSize = _target.GetPortionSize();
            }
        }

        public async Task StartSendEventsToStorage()
        {
            await StartSendEventsToStorage(CancellationToken.None);
        }
        public async Task StartSendEventsToStorage(CancellationToken cancellationToken)
        {
            if (_reader == null || _target == null || _eventPath == null)
                await Task.CompletedTask;

            await _reader.StartReadEvents(cancellationToken);
        }
        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Reset();
                _reader.Dispose();
                _reader = null;
            }
        }

        #endregion

        #region Private Methods

        private async Task SendDataCurrentPortion(ExtendedEventsReader reader)
        {
            RiseBeforeExportData(out var cancel);
            if (!cancel)
            {
                await _target.Save(_dataToSend);
                RiseAfterExportData(reader.GetCurrentPosition());
            }

            if (reader.CurrentFile != null)
            {
                await _target.SaveLogPosition(
                    new FileInfo(reader.CurrentFile),
                    reader.GetCurrentPosition(),
                    false);
            }
            _dataToSend.Clear();
        }

        private void RiseAfterExportData(ExtendedEventsPosition currentPosition)
        {
            AfterExportDataHandler handlerAfterExportData = _afterExportData;
            handlerAfterExportData?.Invoke(new AfterExportDataEventArgs()
            {
                CurrentPosition = currentPosition
            });

        }
        private void RiseBeforeExportData(out bool cancel)
        {
            BeforeExportDataHandler handlerBeforeExportData = _beforeExportData;
            if (handlerBeforeExportData != null)
            {
                BeforeExportDataEventArgs beforeExportArgs = new BeforeExportDataEventArgs()
                {
                    Rows = _dataToSend
                };
                handlerBeforeExportData.Invoke(beforeExportArgs);
                cancel = beforeExportArgs.Cancel;
            }
            else
            {
                cancel = false;
            }
        }

        #endregion

        #region Events

        private void ExtendedEventsReader_OnReadEvent(ExtendedEventsReader sender, OnReadEventArgs args)
        {
            if (sender.CurrentRow == null)
                return;

            var fileInfo = new FileInfo(sender.CurrentFile);
            var eventData = new XEventData(fileInfo.Name, sender.CurrentFileEventNumber, args.EventData);
            _dataToSend.Add(eventData);

            if (_dataToSend.Count >= _portionSize)
                SendDataCurrentPortion(sender).Wait();
        }
        private void ExtendedEventsReader_BeforeReadFile(ExtendedEventsReader sender, BeforeReadFileEventArgs args)
        {
            // TODO: Отменить обработку файла, если он уже был обработан ранее
            var taskLogFileLoaded = _target.LogFileLoaded(args.FileName);
            taskLogFileLoaded.Wait();
            if (taskLogFileLoaded.Result)
                args.Cancel = true;
        }
        private void ExtendedEventsReader_AfterReadFile(ExtendedEventsReader sender, AfterReadFileEventArgs args)
        {
            FileInfo _lastEventLogDataFileInfo = new FileInfo(args.FileName);

            if (_dataToSend.Count > 0)
                SendDataCurrentPortion(sender).Wait();

            ExtendedEventsPosition position = sender.GetCurrentPosition();
            _target.SaveLogPosition(_lastEventLogDataFileInfo, position, true);
        }
        private void ExtendedEventsReader_OnErrorEvent(ExtendedEventsReader sender, OnErrorEventArgs args)
        {
            OnErrorExportDataHandler handlerOnErrorExportData = _onErrorExportData;
            handlerOnErrorExportData?.Invoke(new OnErrorExportDataEventArgs()
            {
                Exception = args.Exception
            });
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YPermitin.DBTools.Core.Helpers;
using YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.Models;

namespace YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer
{
    public class ExtendedEventsOnBuffer : IXEventsOnTarget
    {
        #region Private Member Variables

        private const int _defaultPortion = 1000;
        private readonly int _portion;
        private ExtendedEventsPosition _lastTechJournalFilePosition;
        private XEventsExportSettings.LogSourceSettings _logSettings;
        private readonly LogBuffers _logBuffers;
        private IList<XEventData> _rowsDataForPosition;
        private ExtendedEventsLogBase _xEventsLog;


        #endregion

        #region Constructor

        public ExtendedEventsOnBuffer() : this(null, _defaultPortion)
        {

        }
        public ExtendedEventsOnBuffer(int portion) : this(null, portion)
        {
            _portion = portion;
        }
        public ExtendedEventsOnBuffer(LogBuffers buffers, int portion)
        {
            _portion = portion;
            _rowsDataForPosition = new List<XEventData>();
            _logBuffers = buffers;
        }

        #endregion

        #region Public Methods

        public void SetLogSettings(XEventsExportSettings.LogSourceSettings logSettings)
        {
            _logSettings = logSettings;
        }
        public void SetLastPosition(ExtendedEventsPosition position)
        {
            _lastTechJournalFilePosition = position;
        }
        public async Task<ExtendedEventsPosition> GetLastPosition(string fileName)
        {
            if (_lastTechJournalFilePosition != null)
                return _lastTechJournalFilePosition;

            ExtendedEventsPosition position = await _logBuffers.GetLastPosition(_logSettings, fileName);

            _lastTechJournalFilePosition = position;
            return position;
        }
        public async Task SaveLogPosition(ExtendedEventsPosition position)
        {
            _lastTechJournalFilePosition = position;
            await _logBuffers.SaveLogsAndPosition(_logSettings, position, _rowsDataForPosition);
            _rowsDataForPosition.Clear();
        }
        public int GetPortionSize()
        {
            return _portion;
        }
        public async Task Save(XEventData eventData)
        {
            await Save(new List<XEventData>()
            {
                eventData
            });
        }

        public async Task Save(List<XEventData> eventsData)
        {
            await Task.Delay(1000);
            _rowsDataForPosition.Clear();
            _rowsDataForPosition = eventsData.ToList();
        }

        public void SetLogInformation(ExtendedEventsLogBase xEventsLog)
        {
            _xEventsLog = xEventsLog;
        }

        public async Task<bool> LogFileChanged(FileInfo logFileInfo)
        {
            ExtendedEventsPosition position = await _logBuffers.GetLastPosition(_logSettings, logFileInfo.Name);

            if (position == null)
                return true;

            if (position.FinishReadFile)
            {
                if (position.LogFileCreateDate.Truncate(TimeSpan.FromSeconds(1)) != logFileInfo.CreationTimeUtc.Truncate(TimeSpan.FromSeconds(1))
                    || position.LogFileModificationDate.Truncate(TimeSpan.FromSeconds(1)) != logFileInfo.LastWriteTimeUtc.Truncate(TimeSpan.FromSeconds(1)))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}

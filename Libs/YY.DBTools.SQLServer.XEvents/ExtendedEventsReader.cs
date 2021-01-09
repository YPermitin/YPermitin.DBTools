using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SqlServer.XEvent.XELite;
using YY.DBTools.SQLServer.XEvents.EventArguments;

namespace YY.DBTools.SQLServer.XEvents
{
    public sealed class ExtendedEventsReader : IExtendedEventsReader, IDisposable
    {
        private Stream _stream;
        private readonly string _logFileDirectoryPath;
        private string[] _logFilesWithData;
        private int _indexCurrentFile;
        private long _currentFileEventNumber;
        private readonly bool _logFileSourcePathIsDirectory;

        private IXEvent _currentRow;
        private ExtendedEventsPosition _position;

        #region Constructors

        public ExtendedEventsReader(string logFilePath,
            OnReadEventHandler onReadEvent): this(logFilePath, onReadEvent, null, null, null, null)
        {
        }

        public ExtendedEventsReader(string logFilePath,
            OnReadEventHandler onReadEvent,
            BeforeReadFileHandler beforeReadFile,
            AfterReadFileHandler afterReadFile) : this(logFilePath, onReadEvent, null, beforeReadFile, afterReadFile, null)
        {
        }

        public ExtendedEventsReader(string logFilePath, 
            OnReadEventHandler onReadEvent,
            OnReadMetadataHandler onReadMetadata,
            BeforeReadFileHandler beforeReadFile,
            AfterReadFileHandler afterReadFile,
            OnErrorEventHandler onError)
        {
            OnReadMetadata = onReadMetadata;
            OnReadEvent = onReadEvent;
            BeforeReadFile = beforeReadFile;
            AfterReadFile = afterReadFile;

            if (File.GetAttributes(logFilePath).HasFlag(FileAttributes.Directory))
            {
                _logFileDirectoryPath = logFilePath;
                _logFileSourcePathIsDirectory = true;
                UpdateEventLogFilesFromDirectory();
            }
            else
            {
                _logFileSourcePathIsDirectory = false;
                _logFilesWithData = new[] { logFilePath };
                _logFileDirectoryPath = new FileInfo(_logFilesWithData[0]).Directory?.FullName;
            }
        }

        #endregion

        #region Public Methods

        public string CurrentFile
        {
            get
            {
                if (_logFilesWithData.Length <= _indexCurrentFile)
                    return null;

                return _logFilesWithData[_indexCurrentFile];
            }
        }
        public IXEvent CurrentRow => _currentRow;
        public long CurrentFileEventNumber => _currentFileEventNumber;

        #endregion

        #region Public Methods

        public async Task StartReadEvents()
        {
            await StartReadEvents(CancellationToken.None);
        }
        public async Task StartReadEvents(CancellationToken cancellationToken)
        {
            while (CurrentFile != null)
            {
                if (!InitializeReadFileStream())
                    return;

                RaiseBeforeReadFileEvent(out bool cancelBeforeReadFile);
                if (cancelBeforeReadFile)
                {
                    NextFile();
                    await StartReadEvents(cancellationToken);
                    return;
                }

                _position = null;
                _currentRow = null;
                _currentFileEventNumber = 1;
                try
                {
                    XEFileEventStreamer xeReader = new XEFileEventStreamer(_stream);
                    await xeReader.ReadEventStream(() =>
                        {
                            _position = new ExtendedEventsPosition(
                                _currentFileEventNumber,
                                CurrentFile,
                                null,
                                null);
                            RaiseOnReadMetadata(new OnReadMetadataArgs(_position));
                            return Task.CompletedTask;
                        },
                        (eventData) =>
                        {
                            _position = new ExtendedEventsPosition(
                                _currentFileEventNumber,
                                CurrentFile,
                                eventData.UUID.ToString(),
                                eventData.Timestamp);

                            _currentRow = eventData;
                            RaiseOnRead(new OnReadEventArgs(_currentRow, _position, _currentFileEventNumber));
                            _currentFileEventNumber++;

                            return Task.CompletedTask;
                        }, cancellationToken);
                    
                    NextFile();
                }
                catch (Exception ex)
                {
                    RaiseOnError(new OnErrorEventArgs(ex));
                    break;
                }
            }
        }
        public ExtendedEventsPosition GetCurrentPosition()
        {
            return _position;
        }
        public async Task<long> Count()
        {
            return await Count(CancellationToken.None);
        }
        public async Task<long> Count(CancellationToken cancellationToken)
        {
            long eventCount = 0;

            foreach (var logFile in _logFilesWithData)
            {
                using (Stream logFileStream = new FileStream(logFile,
                    FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
                    81920, false))
                {
                    XEFileEventStreamer xeReader = new XEFileEventStreamer(logFileStream);
                    await xeReader.ReadEventStream((eventData) =>
                    {
                        eventCount++;
                        return Task.CompletedTask;
                    }, CancellationToken.None);
                }
            }

            return eventCount;
        }
        public void Reset()
        {
            if (_stream != null)
            {
                _stream?.Dispose();
                _stream = null;
            }

            _indexCurrentFile = 0;
            UpdateEventLogFilesFromDirectory();
            _currentFileEventNumber = 0;
            _currentRow = null;
        }
        public void NextFile()
        {
            RaiseAfterReadFile(new AfterReadFileEventArgs(CurrentFile));

            if (_stream != null)
            {
                _stream?.Dispose();
                _stream = null;
            }

            _indexCurrentFile += 1;
        }
        public void Dispose()
        {
            if (_stream != null)
            {
                _stream?.Dispose();
                _stream = null;
            }
        }

        #endregion

        #region Private Methods

        private void UpdateEventLogFilesFromDirectory()
        {
            if (_logFileSourcePathIsDirectory)
            {
                _logFilesWithData = Directory
                    .GetFiles(_logFileDirectoryPath, "*.xel")
                    .OrderBy(i => i)
                    .ToArray();
            }
        }
        private bool InitializeReadFileStream()
        {
            if (_stream == null)
            {
                if (_logFilesWithData.Length <= _indexCurrentFile)
                {
                    _currentRow = null;
                    return false;
                }

                InitializeStream(_indexCurrentFile);
                _currentFileEventNumber = 0;
            }

            return true;
        }
        private void InitializeStream(int fileIndex = 0)
        {
            FileStream fs = new FileStream(_logFilesWithData[fileIndex], 
                FileMode.Open, 
                FileAccess.Read, 
                FileShare.ReadWrite,
                81920, 
                false);
            _stream = fs;
        }
        private void RaiseBeforeReadFileEvent(out bool cancel)
        {
            BeforeReadFileEventArgs beforeReadFileArgs = new BeforeReadFileEventArgs(CurrentFile);
            if (_currentFileEventNumber == 0)
                RaiseBeforeReadFile(beforeReadFileArgs);

            cancel = beforeReadFileArgs.Cancel;
        }

        #endregion

        #region Events

        public delegate void BeforeReadFileHandler(ExtendedEventsReader sender, BeforeReadFileEventArgs args);
        public delegate void AfterReadFileHandler(ExtendedEventsReader sender, AfterReadFileEventArgs args);
        public delegate void OnReadEventHandler(ExtendedEventsReader sender, OnReadEventArgs args);
        public delegate void OnReadMetadataHandler(ExtendedEventsReader sender, OnReadMetadataArgs args);
        public delegate void OnErrorEventHandler(ExtendedEventsReader sender, OnErrorEventArgs args);

        public event BeforeReadFileHandler BeforeReadFile;
        public event AfterReadFileHandler AfterReadFile;
        public event OnReadEventHandler OnReadEvent;
        public event OnReadMetadataHandler OnReadMetadata;
        public event OnErrorEventHandler OnErrorEvent;

        private void RaiseBeforeReadFile(BeforeReadFileEventArgs args)
        {
            BeforeReadFile?.Invoke(this, args);
        }
        private void RaiseAfterReadFile(AfterReadFileEventArgs args)
        {
            AfterReadFile?.Invoke(this, args);
        }
        private void RaiseOnReadMetadata(OnReadMetadataArgs args)
        {
            OnReadMetadata?.Invoke(this, args);
        }
        private void RaiseOnRead(OnReadEventArgs args)
        {
            OnReadEvent?.Invoke(this, args);
        }
        private void RaiseOnError(OnErrorEventArgs args)
        {
            OnErrorEvent?.Invoke(this, args);
        }

        #endregion
    }
}

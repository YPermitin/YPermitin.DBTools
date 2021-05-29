using System;

namespace YY.DBTools.SQLServer.XEvents
{
    public class ExtendedEventsPosition
    {
        public ExtendedEventsPosition(
            long eventNumber, 
            string currentFileData, 
            string eventUUID, 
            DateTimeOffset? eventPeriod,
            bool finishReadFile,
            DateTime logFileCreateDate,
            DateTime logFileModificationDate)
        {
            EventNumber = eventNumber;
            CurrentFileData = currentFileData;
            EventUUID = eventUUID;
            EventPeriod = eventPeriod;
            FinishReadFile = finishReadFile;
            LogFileCreateDate = logFileCreateDate;
            LogFileModificationDate = logFileModificationDate;
        }

        public long EventNumber { get; }
        public string CurrentFileData { get; }
        public string EventUUID { get; }
        public DateTimeOffset? EventPeriod { get; }
        public bool FinishReadFile { get; }
        public DateTime LogFileCreateDate { get; }
        public DateTime LogFileModificationDate { get; }
    }
}

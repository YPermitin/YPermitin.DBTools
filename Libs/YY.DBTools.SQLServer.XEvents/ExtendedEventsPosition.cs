using System;

namespace YY.DBTools.SQLServer.XEvents
{
    public class ExtendedEventsPosition
    {
        public ExtendedEventsPosition(long eventNumber, string currentFileData, 
            string eventUUID, DateTimeOffset? eventPeriod)
        {
            EventNumber = eventNumber;
            CurrentFileData = currentFileData;
            EventUUID = eventUUID;
            EventPeriod = eventPeriod;
        }

        public long EventNumber { get; }
        public string CurrentFileData { get; }
        public string EventUUID { get; }
        public DateTimeOffset? EventPeriod { get; }
    }
}

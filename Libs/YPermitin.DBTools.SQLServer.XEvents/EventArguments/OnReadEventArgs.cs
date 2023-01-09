using System;

namespace YPermitin.DBTools.SQLServer.XEvents.EventArguments
{
    public sealed class OnReadEventArgs : EventArgs
    {
        public OnReadEventArgs(ExtendedEvent rowData, ExtendedEventsPosition position, long eventNumber)
        {
            EventData = rowData;
            EventNumber = eventNumber;
            Position = position;
        }

        public ExtendedEvent EventData { get; }
        public long EventNumber { get; }
        private ExtendedEventsPosition Position { get; }
    }
}

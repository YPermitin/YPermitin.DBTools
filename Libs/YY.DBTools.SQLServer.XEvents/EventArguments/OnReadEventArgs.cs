using System;
using Microsoft.SqlServer.XEvent.XELite;

namespace YY.DBTools.SQLServer.XEvents.EventArguments
{
    public sealed class OnReadEventArgs : EventArgs
    {
        public OnReadEventArgs(IXEvent rowData, ExtendedEventsPosition position, long eventNumber)
        {
            EventData = rowData;
            EventNumber = eventNumber;
            Position = position;
        }

        public IXEvent EventData { get; }
        public long EventNumber { get; }
        private ExtendedEventsPosition Position { get; }
    }
}

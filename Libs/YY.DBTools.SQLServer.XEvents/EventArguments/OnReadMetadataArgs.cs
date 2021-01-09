using System;

namespace YY.DBTools.SQLServer.XEvents.EventArguments
{
    public sealed class OnReadMetadataArgs : EventArgs
    {
        public OnReadMetadataArgs(ExtendedEventsPosition position)
        {
            Position = position;
        }

        private ExtendedEventsPosition Position { get; }
    }
}

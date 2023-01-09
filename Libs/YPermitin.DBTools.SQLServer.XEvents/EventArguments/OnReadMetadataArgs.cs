using System;

namespace YPermitin.DBTools.SQLServer.XEvents.EventArguments
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

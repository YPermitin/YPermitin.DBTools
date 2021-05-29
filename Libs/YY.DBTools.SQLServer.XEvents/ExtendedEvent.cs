using System;
using System.Collections.Generic;
using Microsoft.SqlServer.XEvent.XELite;

namespace YY.DBTools.SQLServer.XEvents
{
    public class ExtendedEvent
    {
        private readonly IXEvent _sourceEvent;

        public long Id { get; }

        public string Name => _sourceEvent.Name;

        public Guid UUID => _sourceEvent.UUID;

        public DateTimeOffset Timestamp => _sourceEvent.Timestamp;

        public IReadOnlyDictionary<string, object> Fields => _sourceEvent.Fields;

        public IReadOnlyDictionary<string, object> Actions => _sourceEvent.Actions;

        public ExtendedEvent(long id, IXEvent sourceEvent)
        {
            Id = id;
            _sourceEvent = sourceEvent;
        }
    }
}

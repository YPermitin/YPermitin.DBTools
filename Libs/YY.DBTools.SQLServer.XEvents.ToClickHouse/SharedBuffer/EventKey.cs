using System;
using System.IO;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer
{
    public class EventKey
    {
        public Guid Id { get; set; }
        public FileInfo File { get; set; }
    }
}

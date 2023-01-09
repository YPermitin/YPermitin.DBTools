using System.Collections.Generic;
using YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.Models;

namespace YPermitin.DBTools.SQLServer.XEvents.ToClickHouse
{
    public sealed class BeforeExportDataEventArgs
    {
        public BeforeExportDataEventArgs()
        {
            Cancel = false;
        }

        public IReadOnlyList<XEventData> Rows { set; get; }
        public bool Cancel { set; get; }
    }
}

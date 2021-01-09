using System.Collections.Generic;
using YY.DBTools.SQLServer.XEvents.ToClickHouse.Models;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse
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

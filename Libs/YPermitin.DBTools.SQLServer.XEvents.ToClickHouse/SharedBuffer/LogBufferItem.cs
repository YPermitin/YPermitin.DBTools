using System.Collections.Concurrent;
using YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.Models;

namespace YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.SharedBuffer
{
    /// <summary>
    /// Элемент буфера логов
    /// </summary>
    public class LogBufferItem
    {
        /// <summary>
        /// Количество записей лога в буфере
        /// </summary>
        public long ItemsCount => LogRows.Count;

        /// <summary>
        /// Актуальная позиция чтения файла лога
        /// </summary>
        public ExtendedEventsPosition LogPosition { get; set; }

        /// <summary>
        /// Записи логов
        /// </summary>
        public ConcurrentDictionary<EventKey, XEventData> LogRows { get; set; }
        
        public LogBufferItem()
        {
            LogRows = new ConcurrentDictionary<EventKey, XEventData>();
        }
    }
}

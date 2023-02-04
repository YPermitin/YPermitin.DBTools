using YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models.Settings;

namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models
{
    public class BCPConfig
    {
        /// <summary>
        /// Настройки подключения к SQL Server
        /// </summary>
        public ConnectionSettings ConnectionSettings { get; set; }

        /// <summary>
        /// Настройки испорта / экспорта
        /// </summary>
        public ImportExportSettings ImportExportSettings { get; set; }

        /// <summary>
        /// Настройки BULK-операций
        /// </summary>
        public BulkSettings BulkSettings { get; set; }
        
        /// <summary>
        /// Дополнительные настройки
        /// </summary>
        public AdditionalSettings AdditionalSettings { get; set; }
        
        public BCPConfig()
        {
            ConnectionSettings = new ConnectionSettings();
            BulkSettings = new BulkSettings();
            ImportExportSettings = new ImportExportSettings();
            AdditionalSettings = new AdditionalSettings();
        }
    }
}

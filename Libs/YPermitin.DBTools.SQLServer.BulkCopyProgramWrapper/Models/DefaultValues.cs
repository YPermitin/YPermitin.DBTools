using YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Enums;

namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models
{
    /// <summary>
    /// Значения по умолчанию
    /// </summary>
    public static class DefaultValues
    {
        /// <summary>
        /// Размер пакета в байтах
        /// </summary>
        public static int PacketSize = 4096;

        /// <summary>
        /// Максимальное количество ошибок при выполнении операции
        /// </summary>
        public static int MaxErrors = 10;

        /// <summary>
        /// Количество записей в пакете при импорте данных
        /// </summary>
        public static int BatchSize = 0;

        /// <summary>
        /// Разделитель записей (строк) в файле выгрузки
        /// </summary>
        public static string RowTerminator = "\n";

        /// <summary>
        /// Путь для запуска утилиты BCP
        /// </summary>
        public static string UtilityPath = "bcp";

        /// <summary>
        /// Версии типов данных SQL Server
        /// </summary>
        public static SQLServerDataTypeVersion DataTypeVersion = SQLServerDataTypeVersion.Latest;

        /// <summary>
        /// Направление потока данных
        /// </summary>
        public static BulkCopyDirection BulkCopyDirection = BulkCopyDirection.Out;

        /// <summary>
        /// Таймаут установки соединения со SQL Server
        /// </summary>
        public static int LoginTimeout = 15;

        /// <summary>
        /// Использовать доверенное соединение с использованием встроенной системы безопасности.
        /// </summary>
        public static bool UseTrustedConnection = true;
    }
}

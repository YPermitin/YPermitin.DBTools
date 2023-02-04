namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Enums
{
    /// <summary>
    /// Направление BULK-операции
    /// </summary>
    public enum BulkCopyDirection
    {
        /// <summary>
        /// Копирует из файла в таблицу или представление базы данных.
        /// </summary>
        In,

        /// <summary>
        /// Копирует файл в таблицу или представление базы данных.
        /// Если указать существующий файл, он будет перезаписан.
        /// При извлечении данных утилита BCP представляет пустую строку как NULL, а NULL-строки как пустые строки.
        /// </summary>
        Out,

        /// <summary>
        /// Копирует результат произвольного запроса в файл. Запрос должен обязательно быть указан.
        /// </summary>
        QueryOut,

        /// <summary>
        /// Создает файл форматирования на основе указанной опции BULK-операции (раздел BulkSettings)
        /// (UseNativeDataTypes, UseNativeDataTypesWithCharactersSupport, UseUnicodeCharacters или UseCharacterType)
        /// и разделителей таблицы или представления.
        /// 
        /// При массовом копировании данных команда bcp может ссылаться на файл форматирования,
        /// что избавляет вас от необходимости повторно вводить информацию о формате в интерактивном режиме
        /// (последнее невозможно при работе через библиотеку).
        ///
        /// Для параметра формата требуется указать опцию FormatFile в разделе ImportExportSettings.
        /// При необходимости форматирования его в формате XML нужно указать опцию UseXMLFormatFile.
        /// 
        /// Для детальной информации смотрите официальную документацию "Create a Format File (SQL Server)" [https://learn.microsoft.com/en-us/sql/relational-databases/import-export/create-a-format-file-sql-server?view=sql-server-ver16].
        /// </summary>
        Format
    }
}

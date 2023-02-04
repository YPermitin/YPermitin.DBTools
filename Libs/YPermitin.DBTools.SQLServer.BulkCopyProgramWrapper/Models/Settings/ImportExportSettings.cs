using System.Text;
using YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Enums;

namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models.Settings
{
    /// <summary>
    /// Настройки импорта / экспорта
    /// </summary>
    public class ImportExportSettings : ICommonSettings
    {
        /// <summary>
        /// Направление BULK-операции.
        /// </summary>
        public BulkCopyDirection Direction { get; set; }

        /// <summary>
        /// Имя владельца таблицы или представления.
        /// Необязательный параметр, если пользователь, выполняющий операцию, является владельцем таблицы или представления.
        /// Если схема не указана и пользователь, выполняющий операцию, не является владельцом указанной таблицы или представления,
        /// SQL Server возвращает сообщение об ошибке, и операция отменяется.
        /// </summary>
        public string? Schema { get; set; }

        /// <summary>
        /// Имя целевой таблицы при импорте данных в SQL Server (in),
        /// и исходная таблица при экспорте данных из SQL Server (out).
        /// </summary>
        public string? TableName { get; set; }

        /// <summary>
        /// Имя целевого представления при копировании данных в SQL Server (in),
        /// и исходное представление при копировании данных с SQL Server (out).
        /// Только представления, в которых все столбцы ссылаются на одну и ту же таблицу, могут использоваться в качестве целевых представлений.
        /// Подробная информация в документации.
        /// [https://learn.microsoft.com/en-us/sql/t-sql/statements/insert-transact-sql?view=sql-server-ver16]
        /// </summary>
        public string? ViewName { get; set; }

        /// <summary>
        /// Запрос Transact-SQL, который возвращает набор результатов.
        ///
        /// Если запрос возвращает несколько наборов результатов, в файл данных копируется только первый набор результатов;
        /// последующие наборы результатов игнорируются. Используйте двойные кавычки вокруг текста запроса,
        /// и одинарные кавычки вокруг всего, что включено в запрос.
        ///
        /// Направление BULK-операции должен быть установлен в QueryOut при массовом копировании данных из запроса.
        ///
        /// Запрос может ссылаться на хранимую процедуру, если все таблицы, на которые есть ссылки внутри хранимой процедуры, существуют
        /// для выполнения выражения утилитой BCP. Например, если хранимая процедура создает временную таблицу,
        /// оператор bcp завершается ошибкой, поскольку временная таблица доступна только во время выполнения, а не во время выполнения оператора.
        /// В этом случае рассмотрите возможность вставки результатов хранимой процедуры в таблицу,
        /// а затем используйте BCP для копирования данных из таблицы в файл данных.
        /// </summary>
        public string? Query { get; set; }

        /// <summary>
        /// Полный путь к файлу данных. Когда данные массово импортируются в SQL Server,
        /// файл данных содержит данные, которые необходимо скопировать в указанную таблицу или представление.
        /// При массовом экспорте данных из SQL Server файл данных содержит скопированные данные
        /// из таблицы или представления. Путь может содержать от 1 до 255 символов.
        /// Файл данных может содержать максимум 2^63 - 1 строк.
        /// </summary>
        public string? DataFile { get; set; }

        /// <summary>
        /// Задает количество строк в одном пакете импортируемых данных.
        /// Каждый пакет импортируется и регистрируется как отдельной транзакции.
        /// 
        /// По умолчанию все строки в файле данных импортируются как один пакет в одной транзакции.
        /// Чтобы распределить строки между несколькими пакетами, укажите размер пакета меньше, чем количество строк в файле данных.
        /// Если транзакция для любого пакета завершается неудачно, откатываются только операции из текущего пакета.
        /// Пакеты с уже завиксированными транзакциями откатываться не будут.
        /// </summary>
        public long BatchSize { get; set; }

        /// <summary>
        /// Задает полный путь к файлу форматирования.
        ///
        /// Значение этой опции зависит от контекста вполнения:
        ///    * При использовании направления BULK-операции как Format, то по указанному пути будет сохранен сгенерированный файл форматирования.
        ///    * Если выполняется направление BULK-операции In или Out, то по указанному пути уже должен существовать файл форматирования для использования.
        ///
        /// Использование файла форматирования для направления BULK-операций In или Out не обязательно и зависит от других настроек.
        /// </summary>
        public string? FormatFile { get; set; }

        /// <summary>
        /// Использовать формат XML для файла формата.
        /// Используется с указанием направления BULK-операции Format и параметра с путем к файлу FormatFile.
        /// </summary>
        public bool UseXMLFormatFile { get; set; }
        
        public ImportExportSettings()
        {
            BatchSize = DefaultValues.BatchSize;
            Direction = DefaultValues.BulkCopyDirection;
        }

        public ImportExportSettings ExportFromQueryToFile(string query, string? dataFile)
        {
            Direction = BulkCopyDirection.QueryOut;
            Schema = null;
            TableName = null;
            ViewName = null;
            Query = query;
            DataFile = dataFile;
            BatchSize = DefaultValues.BatchSize;
            FormatFile = null;
            UseXMLFormatFile = false;

            return this;
        }

        public ImportExportSettings ExportFromTableToFile(string table, string? dataFile, string? schema = null)
        {
            Direction = BulkCopyDirection.Out;
            Schema = schema;
            TableName = table;
            ViewName = null;
            Query = null;
            DataFile = dataFile;
            BatchSize = DefaultValues.BatchSize;
            FormatFile = null;
            UseXMLFormatFile = false;

            return this;
        }

        public ImportExportSettings ExportFromViewToFile(string view, string? dataFile, string? schema = null)
        {
            Direction = BulkCopyDirection.Out;
            Schema = schema;
            TableName = null;
            ViewName = view;
            Query = null;
            DataFile = dataFile;
            BatchSize = DefaultValues.BatchSize;
            FormatFile = null;
            UseXMLFormatFile = false;

            return this;
        }

        public ImportExportSettings ExportFormatFileOnlyForTable(string table, string formatFile, string? schema = null, bool useXMLFormatFile = false)
        {
            Direction = BulkCopyDirection.Format;
            Schema = schema;
            TableName = table;
            ViewName = null;
            Query = null;
            DataFile = null;
            BatchSize = DefaultValues.BatchSize;
            FormatFile = formatFile;
            UseXMLFormatFile = useXMLFormatFile;

            return this;
        }

        public ImportExportSettings ExportFormatFileOnlyForView(string view, string formatFile, string? schema = null, bool useXMLFormatFile = false)
        {
            Direction = BulkCopyDirection.Format;
            Schema = schema;
            TableName = null;
            ViewName = view;
            Query = null;
            DataFile = null;
            BatchSize = DefaultValues.BatchSize;
            FormatFile = formatFile;
            UseXMLFormatFile = useXMLFormatFile;

            return this;
        }

        public ImportExportSettings WithFormatFile(string formatFile, string? schema = null)
        {
            switch (Direction)
            {
                case BulkCopyDirection.Format:
                    // Отдельный файл формата устанавливается для операций импорта и экспорта данных.
                    // Для операций выгрузки только файла формата не имеет смысла.
                    FormatFile = null;
                    return this;
                case BulkCopyDirection.QueryOut:
                    // Для выгрузки данных по произвольному запросу файл формата не поддерживается.
                    FormatFile = null;
                    return this;
                case BulkCopyDirection.Out:
                    FormatFile = formatFile;
                    return this;
                case BulkCopyDirection.In:
                    FormatFile = formatFile;
                    return this;
            }

            return this;
        }

        public ImportExportSettings ImportFromFileToTable(string table, string? dataFile, string? schema = null, long? batchSize = null)
        {
            Direction = BulkCopyDirection.In;
            Schema = schema;
            TableName = table;
            ViewName = null;
            Query = null;
            DataFile = dataFile;
            BatchSize = batchSize ?? DefaultValues.BatchSize;
            FormatFile = null;
            UseXMLFormatFile = false;

            return this;
        }

        /// <summary>
        /// Добавить параметры командной строки к стоке
        /// </summary>
        /// <param name="bcpArguments">Объект StringBuilder для формирования строки запуска утилиты BCP</param>
        public void AddCommandLineParameters(StringBuilder bcpArguments)
        {
            #region Data source definition

            if (!string.IsNullOrEmpty(Query))
            {
                bcpArguments.Append("\"");
                bcpArguments.Append(Query);
                bcpArguments.Append("\"");
            }
            else
            {
                if (!string.IsNullOrEmpty(Schema))
                {
                    bcpArguments.Append(Schema);
                }
                if (!string.IsNullOrEmpty(TableName))
                {
                    if (!string.IsNullOrEmpty(Schema))
                    {
                        bcpArguments.Append(".");
                    }
                    bcpArguments.Append(TableName);
                }
                else if (!string.IsNullOrEmpty(ViewName))
                {
                    if (!string.IsNullOrEmpty(Schema))
                    {
                        bcpArguments.Append(".");
                    }
                    bcpArguments.Append(ViewName);
                }
            }

            #endregion

            #region Bulk copy direction

            switch (Direction)
            {
                case BulkCopyDirection.Out:
                    bcpArguments.Append(" ");
                    bcpArguments.Append("out \"");
                    bcpArguments.Append(DataFile);
                    bcpArguments.Append("\"");
                    break;
                case BulkCopyDirection.In:
                    bcpArguments.Append(" ");
                    bcpArguments.Append("in \"");
                    bcpArguments.Append(DataFile);
                    bcpArguments.Append("\"");
                    break;
                case BulkCopyDirection.QueryOut:
                    bcpArguments.Append(" ");
                    bcpArguments.Append("queryout \"");
                    bcpArguments.Append(DataFile);
                    bcpArguments.Append("\"");
                    break;
                case BulkCopyDirection.Format:
                    bcpArguments.Append(" ");
                    bcpArguments.Append("format \"");
                    bcpArguments.Append(FormatFile);
                    bcpArguments.Append("\"");
                    break;
                default:
                    throw new Exception($"Unsupported bulk copy direction: {Direction}");
            }

            #endregion

            #region Batch size

            if (BatchSize > 0)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-b ");
                bcpArguments.Append(BatchSize);
            }

            #endregion
            
            #region Format file

            if (!string.IsNullOrEmpty(FormatFile))
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-f ");
                bcpArguments.Append("\"");
                bcpArguments.Append(FormatFile);
                bcpArguments.Append("\"");
            }

            #endregion
            
            #region Used with the format and -f format_file options, generates an XML-based format file instead of the default non-XML format file. 

            if (UseXMLFormatFile)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-x");
            }

            #endregion
        }
    }
}

using System.Text;
using YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Enums;
// ReSharper disable InvalidXmlDocComment

namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models.Settings
{
    /// <summary>
    /// Настройки BULK-операций
    /// </summary>
    public class BulkSettings : ICommonSettings
    {
        /// <summary>
        /// Символ конца строки.
        ///
        /// По умолчанию используется \n (символ новой строки). Используйте этот параметр, чтобы переопределить признак конца строки по умолчанию.
        /// 
        /// Подробная информация: Specify Field and Row Terminators (SQL Server)
        /// [https://learn.microsoft.com/eN-us/sql/relational-databases/import-export/specify-field-and-row-terminators-sql-server?view=azuresqldb-current].
        ///
        /// Если Вы укажете символ конца строки в шестнадцатеричном представлении в команде BCP,
        /// значение будет усечено до 0x00. Например, если вы укажете 0x410041, будет использоваться 0x41.
        /// </summary>
        public string? RowTerminator { get; set; }

        /// <summary>
        /// Символ разделителя полей.
        ///
        /// По умолчанию используется \t (символ табуляции). Используйте этот параметр, чтобы переопределить символ разделителя поля по умолчанию.
        /// 
        /// Подробная информация: Specify Field and Row Terminators (SQL Server)
        /// [https://learn.microsoft.com/eN-us/sql/relational-databases/import-export/specify-field-and-row-terminators-sql-server?view=azuresqldb-current].
        ///
        /// Если вы укажете символ конца поля в шестнадцатеричном представлении в команде BCP,
        /// значение будет усечено до 0x00. Например, если вы укажете 0x410041, будет использоваться 0x41.
        /// </summary>
        public string? FieldTerminator { get; set; }

        /// <summary>
        /// Задает специальные подскахки (hints) для использования BULK-операциями.
        ///
        /// Подробнее узнать обо всех возможных настройках для этого параметра обратитесь к документации.
        /// [https://learn.microsoft.com/ru-ru/sql/tools/bcp-utility?view=sql-server-ver16#h]
        /// </summary>
        public string? Hints { get; set; }

        /// <summary>
        /// Указывает номер первой строки для экспорта из таблицы или импорта из файла данных.
        /// Для этого параметра требуется значение больше (>) 0, но меньше (<) или равное (=) общему числу строк.
        /// При отсутствии этого параметра по умолчанию используется первая строка файла.
        ///
        /// Может быть положительным целым числом со значением до 2^63-1.
        /// </summary>
        public long FirstRow { get; set; }

        /// <summary>
        /// Указывает номер последней строки для экспорта из таблицы или импорта из файла данных.
        /// Для этого параметра требуется значение больше (>) 0, но меньше (<) или равное (=) номеру последней строки.
        /// При отсутствии этого параметра по умолчанию используется последняя строка файла.
        ///
        /// Может быть положительным целым числом со значением до 2^63-1.
        /// </summary>
        public long LastRow { get; set; }

        /// <summary>
        /// Данные валюты, даты и времени форматируются использованием заданного регионального формата
        /// для настройки локали клиентского компьютера.
        ///
        /// По умолчанию региональные настройки игнорируются.
        /// </summary>
        public bool UseClientRegionalSetting { get; set; }

        /// <summary>
        /// Указывает, что пустые столбцы должны сохранять NULL-значение во время операции,
        /// вместо того, чтобы вставлять значения по умолчанию для столбцов.
        ///
        /// Подробная информация: Keep Nulls or Use Default Values During Bulk Import (SQL Server).
        /// [https://learn.microsoft.com/en-us/sql/relational-databases/import-export/keep-nulls-or-use-default-values-during-bulk-import-sql-server?view=sql-server-ver16]
        /// </summary>
        public bool EmptyColumnsShouldRetainNullValue { get; set; }

        /// <summary>
        /// Указывает, что значение идентификатора или значения в импортированном файле данных должны использоваться для столбца идентификаторов.
        ///
        /// Если параметр не задан, значения идентификаторов для этого столбца в импортируемом файле данных игнорируются
        /// а SQL Server автоматически присваивает уникальные значения на основе начальных значений и значений приращения, указанных при создании таблицы.
        ///
        /// Подробная информация DBCC CHECKIDENT.
        /// [https://learn.microsoft.com/en-us/sql/t-sql/database-console-commands/dbcc-checkident-transact-sql?view=sql-server-ver16]
        ///
        /// Если файл данных не содержит значений для столбца идентификаторов в таблице или представлении,
        /// используйте файл формата, чтобы указать, что столбец идентификаторов в таблице или представлении должен быть пропущен при импорте данных;
        /// SQL Server автоматически присваивает столбцу уникальные значения.
        /// </summary>
        public string? IdentityValues { get; set; }

        /// <summary>
        /// Выполняет инструкцию SET QUOTED_IDENTIFIERS ON в соединении между утилитой BCP и экземпляром SQL Server.
        ///
        /// Используйте этот параметр, чтобы указать имя базы данных, владельца, таблицы или представления, содержащее пробел или одинарную кавычку.
        /// Заключите всю таблицу из трех частей или имя представления в кавычки ("").
        ///
        /// Подробная информация: Remarks, later in this topic.
        /// [https://learn.microsoft.com/en-us/sql/tools/bcp-utility?view=sql-server-ver16#remarks]
        /// </summary>
        public bool QuotedIdentifiers { get; set; }

        /// <summary>
        /// Выполняет BULK-операции, используя собственные типы данных (базы данных).
        ///
        /// Подробная информация: Use Native Format to Import or Export Data (SQL Server).
        /// [https://learn.microsoft.com/en-us/sql/relational-databases/import-export/use-native-format-to-import-or-export-data-sql-server?view=sql-server-ver16]
        /// </summary>
        public bool UseNativeDataTypes { get; set; }

        /// <summary>
        /// Выполняет BULK-операцию, используя собственные типы данных (базы данных) для несимвольных данных,
        /// и символы Unicode для символьных данных. Эта опция предлагает более производительную альтернативу опции UseUnicodeCharacters
        /// и предназначен для передачи данных из одного экземпляра SQL Server в другой с помощью файла данных.
        ///
        /// Используйте этот параметр при передаче данных, содержащих расширенные символы ANSI
        /// и вы хотите воспользоваться производительностью основного режима.
        ///
        /// Подробная информация: Use Unicode Native Format to Import or Export Data (SQL Server).
        /// [https://learn.microsoft.com/en-us/sql/relational-databases/import-export/use-unicode-native-format-to-import-or-export-data-sql-server?view=sql-server-ver16]
        ///
        /// Если вы экспортируете, а затем импортируете данные в одну и ту же схему таблицы с помощью BCP с этим параметром,
        /// вы можете увидеть предупреждение об усечении, если есть столбец фиксированной длины, не входящий в Юникод (например, char(10)).
        ///
        /// Предупреждение можно игнорировать. Один из способов устранить это предупреждение — использовать UseNativeDataTypes вместо UseNativeDataTypesWithCharactersSupport.
        /// </summary>
        public bool UseNativeDataTypesWithCharactersSupport { get; set; }

        /// <summary>
        /// Выполняет операцию массового копирования с использованием типов данных из более ранней версии SQL Server.
        ///   * 80 = SQL Server 2000 (8.x)
        ///   * 90 = SQL Server 2005 (9.x)
        ///   * 100 = SQL Server 2008 и SQL Server 2008 R2
        ///   * 110 = SQL Server 2012 (11.x)
        ///   * 120 = SQL Server 2014 (12.x)
        ///   * 130 = SQL Server 2016 (13.x)
        ///
        /// Например, чтобы сгенерировать данные для типов, не поддерживаемых SQL Server 2000 (8.x),
        /// но были представлены в более поздних версиях SQL Server, используйте параметр SQLServer2000.
        ///
        /// Подробная информация: Import Native and Character Format Data from Earlier Versions of SQL Server.
        /// [https://learn.microsoft.com/en-us/sql/relational-databases/import-export/import-native-and-character-format-data-from-earlier-versions-of-sql-server?view=sql-server-ver16]
        /// </summary>
        public SQLServerDataTypeVersion DataTypeVersion { get; set; }

        /// <summary>
        /// Кодировка файла данных, если он содержит символьный формат даных. Относится только к данным,
        /// которые содержат типы char, varchar или text со значениями символов больше 127 или меньше 32.
        ///
        /// Рекомендуем указать настройки кодировки для каждой колонки в файле форматирования,
        /// за исключением случаев, когда вы хотите, чтобы параметр 65001 имел приоритет над спецификацией сопоставления/кодовой страницы.
        ///
        /// ACP - ANSI/Microsoft Windows (ISO 1252).
        /// OEM - Кодировка по умолчанию, если параметр CharacterCodePage не указан.
        /// RAW - Преобразования из одной кодировки в другую не происходит. Это самый быстрый вариант, потому что преобразование не происходит.
        /// code_page - Конкретный номер кодировки; например 850.
        ///     Версии SQL Server до версии 13 (SQL Server 2016 (13.x)) не поддерживают кодовую страницу 65001 (кодировка UTF-8).
        ///     Версии, начинающиеся с 13, могут импортировать кодировку UTF-8 в более ранние версии SQL Server.
        /// </summary>
        public string? CharacterCodePage { get; set; }

        /// <summary>
        /// Выполняет операцию, используя символьный тип данных.
        ///
        /// Используется символьный тип данных с символом \t
        /// в качестве разделителя полей и набором символов \r\n в качестве разделителя строк.
        ///
        /// Несовместим с параметром UseUnicodeCharacters.
        ///
        /// Подробная информация: Use Character Format to Import or Export Data (SQL Server).
        /// [https://learn.microsoft.com/en-us/sql/relational-databases/import-export/use-character-format-to-import-or-export-data-sql-server?view=sql-server-ver16]
        /// </summary>
        public bool UseCharacterType { get; set; }

        /// <summary>
        /// Выполняет операцию массового копирования с использованием символов Unicode.
        /// Несовместим с параметром UseCharacterType.
        /// </summary>
        public bool UseUnicodeCharacters { get; set; }
        
        public BulkSettings()
        {
            DataTypeVersion = DefaultValues.DataTypeVersion;
            RowTerminator = DefaultValues.RowTerminator;
        }

        public BulkSettings WithNativeDataTypes(bool useNativeDataTypes, bool withCharactersSupport)
        {
            if (useNativeDataTypes)
            {
                if (withCharactersSupport)
                {
                    UseNativeDataTypes = false;
                    UseNativeDataTypesWithCharactersSupport = true;
                }
                else
                {
                    UseNativeDataTypes = true;
                    UseNativeDataTypesWithCharactersSupport = false;
                }

                UseUnicodeCharacters = false;
                UseCharacterType = false;
            }
            else
            {
                UseNativeDataTypes = false;
                UseNativeDataTypesWithCharactersSupport = false;
            }

            return this;
        }

        public BulkSettings WithCharacterType(bool useCharacterType, bool withUnicode)
        {
            if (useCharacterType)
            {
                if (withUnicode)
                {
                    UseUnicodeCharacters = true;
                    UseCharacterType = false;
                }
                else
                {
                    UseUnicodeCharacters = false;
                    UseCharacterType = true;
                }

                UseNativeDataTypes = false;
                UseNativeDataTypesWithCharactersSupport = false;
            }
            else
            {
                UseUnicodeCharacters = false;
                UseCharacterType = false;
            }

            return this;
        }

        public BulkSettings WithDataTypesVersion(SQLServerDataTypeVersion? dataTypeVersion = null)
        {
            DataTypeVersion = dataTypeVersion ?? SQLServerDataTypeVersion.Latest;

            return this;
        }

        public BulkSettings WithFilter(long? firstRow = null, long? lastRow = null)
        {
            FirstRow = firstRow ?? 0;
            LastRow = lastRow ?? 0;

            return this;
        }

        public BulkSettings WithTerminators(string rowTerminator, string fieldTerminator)
        {
            RowTerminator = rowTerminator;
            FieldTerminator = fieldTerminator;

            return this;
        }

        public BulkSettings WithHintsForImport(string hints)
        {
            Hints = hints;

            return this;
        }

        public BulkSettings WithClientRegionalSetting(bool useClientRegionalSetting = false)
        {
            UseClientRegionalSetting = useClientRegionalSetting;

            return this;
        }

        public BulkSettings WithEmptyColumnsShouldRetainNullValue(bool emptyColumnsShouldRetainNullValue = false)
        {
            EmptyColumnsShouldRetainNullValue = emptyColumnsShouldRetainNullValue;

            return this;
        }

        public BulkSettings WithIdentityValues(string identityValues)
        {
            IdentityValues = identityValues;

            return this;
        }

        public BulkSettings WithCharacterCodePage(string characterCodePage)
        {
            CharacterCodePage = characterCodePage;

            return this;
        }

        public BulkSettings WithQuotedIdentifiers(bool quotedIdentifiers = false)
        {
            QuotedIdentifiers = quotedIdentifiers;

            return this;
        }

        /// <summary>
        /// Добавить параметры командной строки к стоке
        /// </summary>
        /// <param name="bcpArguments">Объект StringBuilder для формирования строки запуска утилиты BCP</param>
        public void AddCommandLineParameters(StringBuilder bcpArguments)
        {
            #region Specifies the code page of the data in the data file. 

            if (!string.IsNullOrEmpty(CharacterCodePage))
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-C ");
                bcpArguments.Append(CharacterCodePage);
            }

            #endregion

            #region Identity values

            if (!string.IsNullOrEmpty(IdentityValues))
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-E ");
                bcpArguments.Append("\"");
                bcpArguments.Append(IdentityValues);
                bcpArguments.Append("\"");
            }

            #endregion

            #region QUOTED_IDENTIFIERS

            if (QuotedIdentifiers)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-q");
            }

            #endregion

            #region Row terminator

            if (!string.IsNullOrEmpty(RowTerminator) && RowTerminator != DefaultValues.RowTerminator)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-r");
                bcpArguments.Append(" ");
                bcpArguments.Append(RowTerminator);
            }

            #endregion

            #region Field terminator

            if (!string.IsNullOrEmpty(FieldTerminator))
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-t");
                bcpArguments.Append(" ");
                bcpArguments.Append(FieldTerminator);
            }

            #endregion

            #region Performs the operation using a character data type.

            if (UseCharacterType)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-c ");
            }

            #endregion

            #region First row

            if (FirstRow > 0)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-F ");
                bcpArguments.Append(FirstRow);
            }

            #endregion

            #region Last row

            if (LastRow > 0)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-L ");
                bcpArguments.Append(LastRow);
            }

            #endregion

            #region Hints

            if (!string.IsNullOrEmpty(Hints))
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-h ");
                bcpArguments.Append(Hints);
            }

            #endregion

            #region Empty columns should retain a null value

            if (EmptyColumnsShouldRetainNullValue)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-k");
            }

            #endregion

            #region Performs the bulk-copy operation using the native (database) data types of the data. 

            if (UseNativeDataTypes)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-n ");
            }

            #endregion

            #region Performs the bulk-copy operation using the native (database) data types of the data for noncharacter data, and Unicode characters for character data. 
            
            else if (UseNativeDataTypesWithCharactersSupport)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-N ");
            }

            #endregion

            #region Client regional setting

            if (UseClientRegionalSetting)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-R");
            }

            #endregion

            #region Data type version

            if (DataTypeVersion != SQLServerDataTypeVersion.Latest)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-V ");
                bcpArguments.Append(DataTypeVersion.ToString());
            }

            #endregion

            #region Performs the bulk copy operation using Unicode characters.

            if (UseUnicodeCharacters)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-w");
            }

            #endregion
        }
    }
}

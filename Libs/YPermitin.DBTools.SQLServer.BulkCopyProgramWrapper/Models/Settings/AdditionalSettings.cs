using System.Text;

namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models.Settings
{
    /// <summary>
    /// Дополнительные настройки
    /// </summary>
    public class AdditionalSettings : ICommonSettings
    {
        /// <summary>
        /// Указывает имя файла, который получает выходные данные, перенаправленные из командной строки.
        /// </summary>
        public string OutputFile { get; set; }

        /// <summary>
        /// Указывает имя файла ответов, содержащего ответы на вопросы командной строки для каждого поля данных, когда
        /// массовое копирование выполняется в интерактивном режиме. Для работы через библиотеку не поддерживается и оставлено для совместимости.
        /// </summary>
        public string InputFile { get; set; }

        /// <summary>
        /// Задает полный путь к файлу ошибок, используемому для хранения любых строк, которые утилита BCP не может передать из файла в базу данных.
        /// Сообщения об ошибках от команды BCP поступают на клиентский компьютер. Если этот параметр не используется, файл ошибок не создается.
        /// </summary>
        public string ErrorFile { get; set; }

        /// <summary>
        /// Указывает максимальное количество синтаксических ошибок, которые могут возникнуть до отмены операции BCP.
        /// Синтаксическая ошибка подразумевает ошибку преобразования данных в целевой тип данных.
        ///
        /// Общее значение параметра исключает любые ошибки, которые могут быть обнаружены только на сервере, например нарушения ограничений.
        ///
        /// Строка, которую утилита BCP не может скопировать, игнорируется и считается одной ошибкой.
        /// Если этот параметр не включен, значение по умолчанию равно 10.
        /// </summary>
        public int MaxErrors { get; set; }

        public AdditionalSettings()
        {
            MaxErrors = DefaultValues.MaxErrors;
        }

        public AdditionalSettings WithOutputFile(string outputFile)
        {
            OutputFile = outputFile;

            return this;
        }

        public AdditionalSettings WithErrorFileForImport(string errorFile)
        {
            ErrorFile = errorFile;

            return this;
        }

        public AdditionalSettings WithInputFile(string inputFile)
        {
            InputFile = inputFile;

            return this;
        }

        public AdditionalSettings WithMaxErrors(int? maxErrors)
        {
            MaxErrors = maxErrors ?? DefaultValues.MaxErrors;

            return this;
        }

        /// <summary>
        /// Добавить параметры командной строки к стоке
        /// </summary>
        /// <param name="bcpArguments">Объект StringBuilder для формирования строки запуска утилиты BCP</param>
        public void AddCommandLineParameters(StringBuilder bcpArguments)
        {
            #region Max. errors

            if (MaxErrors > 0 && MaxErrors != DefaultValues.MaxErrors)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-m ");
                bcpArguments.Append(MaxErrors);
            }

            #endregion

            #region Output file

            if (!string.IsNullOrEmpty(OutputFile))
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-o ");
                bcpArguments.Append("\"");
                bcpArguments.Append(OutputFile);
                bcpArguments.Append("\"");
            }

            #endregion

            #region Error file

            if (!string.IsNullOrEmpty(ErrorFile))
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-e ");
                bcpArguments.Append("\"");
                bcpArguments.Append(ErrorFile);
                bcpArguments.Append("\"");
            }

            #endregion

            #region Input file

            if (!string.IsNullOrEmpty(InputFile))
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-i ");
                bcpArguments.Append("\"");
                bcpArguments.Append(InputFile);
                bcpArguments.Append("\"");
            }

            #endregion
        }
    }
}

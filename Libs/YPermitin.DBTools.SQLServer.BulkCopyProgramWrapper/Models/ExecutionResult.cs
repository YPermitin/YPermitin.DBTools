namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models
{
    /// <summary>
    /// Результат выполнения команды
    /// </summary>
    public class ExecutionResult
    {
        /// <summary>
        /// Признак успешного запуска
        /// </summary>
        public bool Success
        {
            get
            {
                bool hasError = ExitCode != 0 
                                || !string.IsNullOrEmpty(ErrorMessage);

                return !hasError;
            }
        }

        /// <summary>
        /// Сообщение по результатам запуска.
        ///
        /// Примечание: будет пустым, если задан параметр вывода всех сообщений в файл (параметр "AdditionalSettings:OutputFile")
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Сообщение об ошибке по результатам запуска.
        ///
        /// Примечание: будет пустым, если задан параметр вывода всех сообщений в файл (параметр "AdditionalSettings:OutputFile")
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Код возарата утилиты BCP.
        ///
        /// 0 - успешный запуск, 1 - были ошибки при запуске.
        /// </summary>
        public int ExitCode { get; set; }
    }
}
